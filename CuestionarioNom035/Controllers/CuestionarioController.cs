using CuestionarioNom035.Data;
using CuestionarioNom035.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NOM35.Web.Services;
using NOM35.Web.ViewModels;
using NOM35.Web.Models;
namespace CuestionarioNom035.Controllers;

// Guía III: anónima, nunca se liga a la sesión de MEAX One
[AllowAnonymous]
public class CuestionarioController : Controller
{
    private const int UltimaPregunta = 72;

    private readonly Nom35DbContext _db;
    private readonly IFlujoService _flujo;

    public CuestionarioController(Nom35DbContext db, IFlujoService flujo)
    { _db = db; _flujo = flujo; }

    [HttpGet]
    public IActionResult Inicio()
    {
        return View(new InicioVM { AreasDisponibles = AreasSelect() });
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Inicio(InicioVM vm)
    {
        // repoblar áreas por si hay error
        vm.AreasDisponibles = AreasSelect();

        if (!ModelState.IsValid) return View(vm);

        // Validar que el área exista en catálogo
        var areasPermitidas = new HashSet<string>(CatalogoAreas.Areas(), StringComparer.OrdinalIgnoreCase);
        if (!areasPermitidas.Contains(vm.Area))
        {
            ModelState.AddModelError(nameof(vm.Area), "Área no válida.");
            return View(vm);
        }

        // Validar subárea contra el área seleccionada
        var subareas = CatalogoAreas.Subareas(vm.Area);
        var okSub = subareas.Any(s => string.Equals(s, vm.Subarea, StringComparison.OrdinalIgnoreCase));
        if (!okSub)
        {
            ModelState.AddModelError(nameof(vm.Subarea), "Subárea no válida para el área seleccionada.");
            return View(vm);
        }

        if (vm.TipoEmpleado is null)
        {
            ModelState.AddModelError(nameof(vm.TipoEmpleado), "Selecciona el tipo de empleado.");
            return View(vm);
        }

        var numeroEmpleado = vm.NumeroEmpleado.Trim();
        var anio = FechaHelper.AhoraMexico().Year;

        // Crear/obtener participante
        var part = await _db.Participantes
            .FirstOrDefaultAsync(p => p.NumeroEmpleado == numeroEmpleado);

        if (part is not null)
        {
            // Un solo cuestionario por empleado por año
            var yaTermino = await _db.Cuestionarios
                .AnyAsync(c => c.ParticipanteId == part.Id && c.Anio == anio && c.Fin != null);
            if (yaTermino)
            {
                ModelState.AddModelError(nameof(vm.NumeroEmpleado),
                    $"Este número de empleado ya contestó el cuestionario {anio}.");
                return View(vm);
            }

            // Si dejó uno a medias, continúa donde se quedó
            var enProceso = await _db.CuestionarioSesiones
                .Where(s => s.Cuestionario.ParticipanteId == part.Id && s.Cuestionario.Anio == anio)
                .OrderByDescending(s => s.Cuestionario.Inicio)
                .FirstOrDefaultAsync();
            if (enProceso is not null)
                return RedirectToAction(nameof(Pregunta), new { id = enProceso.Token });

            part.AreaNombre = vm.Area;
            part.SubareaNombre = vm.Subarea;
            part.TipoEmpleado = vm.TipoEmpleado.Value;
        }
        else
        {
            part = new Participante
            {
                NumeroEmpleado = numeroEmpleado,
                AreaNombre = vm.Area,
                SubareaNombre = vm.Subarea,
                TipoEmpleado = vm.TipoEmpleado.Value
            };
            _db.Participantes.Add(part);
        }

        var ses = new CuestionarioSesion
        {
            IndiceActual = 1,
            Cuestionario = new Cuestionario
            {
                Participante = part,
                Anio = anio,
                AreaNombre = vm.Area,
                SubareaNombre = vm.Subarea,
                TipoEmpleado = vm.TipoEmpleado.Value
            }
        };
        _db.CuestionarioSesiones.Add(ses);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Pregunta), new { id = ses.Token });
    }

    // GET: /Cuestionario/Pregunta/{token}
    [HttpGet]
    public async Task<IActionResult> Pregunta(Guid id)
    {
        var ses = await CargarSesionAsync(id);
        if (ses is null) return NotFound();

        // Si ya terminó, ir al agradecimiento
        if (ses.Cuestionario.Fin is not null || ses.IndiceActual > UltimaPregunta)
            return await AvanzarAsync(ses, UltimaPregunta + 1);

        // Mostrar filtros cuando toca y el flag está sin responder
        if (ses.IndiceActual == 65 && ses.Cuestionario.AtiendeClientes is null)
            return View("FiltroClientes", new FiltroClientesVM { SesionId = ses.Token });

        if (ses.IndiceActual == 69 && ses.Cuestionario.EsJefe is null)
            return View("FiltroJefe", new FiltroJefeVM { SesionId = ses.Token });

        // Si el índice actual NO debe mostrarse (por los filtros), saltar al siguiente visible
        if (!_flujo.DebeMostrar(ses.IndiceActual, ses.Cuestionario))
            return await AvanzarAsync(ses, _flujo.SiguienteNumero(ses.Cuestionario, ses.IndiceActual - 1));

        // Cargar la pregunta del índice actual (avanza si no existe)
        var pregunta = await _db.Preguntas
            .Include(p => p.Dimension).ThenInclude(d => d.Dominio).ThenInclude(o => o.Categoria)
            .FirstOrDefaultAsync(p => p.Numero == ses.IndiceActual);

        if (pregunta == null)
            return await AvanzarAsync(ses, _flujo.SiguienteNumero(ses.Cuestionario, ses.IndiceActual));

        var vm = new PreguntaVM
        {
            SesionId = ses.Token,
            AntiBackToken = ses.AntiBackToken ?? "",
            Numero = pregunta.Numero,
            Texto = pregunta.Texto,
            Categoria = pregunta.Dimension.Dominio.Categoria.Nombre,
            Dominio = pregunta.Dimension.Dominio.Nombre,
            Dimension = pregunta.Dimension.Nombre
        };

        return View(vm);
    }

    // POST: /Cuestionario/Responder
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Responder(PreguntaVM vm)
    {
        var ses = await CargarSesionAsync(vm.SesionId);
        if (ses is null) return NotFound();

        if (ses.Cuestionario.Fin is not null)
            return RedirectToAction(nameof(Gracias));

        if (vm.AntiBackToken != ses.AntiBackToken || vm.Numero != ses.IndiceActual)
            return BadRequest("Secuencia inválida. No se permite regresar.");

        if (vm.Seleccion is null || !Enum.IsDefined(vm.Seleccion.Value))
        {
            TempData["Error"] = "Selecciona una opción.";
            return RedirectToAction(nameof(Pregunta), new { id = ses.Token });
        }

        var pregunta = await _db.Preguntas.FirstAsync(p => p.Numero == vm.Numero);

        _db.Respuestas.Add(new Respuesta
        {
            CuestionarioId = ses.CuestionarioId,
            NumeroPregunta = vm.Numero,
            PreguntaId = pregunta.Id,
            Opcion = vm.Seleccion.Value
        });

        // SiguienteNumero se detiene en 65/69 si los filtros siguen sin responder
        return await AvanzarAsync(ses, _flujo.SiguienteNumero(ses.Cuestionario, ses.IndiceActual));
    }

    // POST: /Cuestionario/SetFiltroClientes
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetFiltroClientes(FiltroClientesVM vm)
    {
        var ses = await CargarSesionAsync(vm.SesionId);
        if (ses is null) return NotFound();

        // Solo se acepta cuando el flujo está detenido en este filtro
        if (vm.AtiendeClientes is null || ses.Cuestionario.Fin is not null ||
            ses.IndiceActual != 65 || ses.Cuestionario.AtiendeClientes is not null)
            return RedirectToAction(nameof(Pregunta), new { id = ses.Token });

        ses.Cuestionario.AtiendeClientes = vm.AtiendeClientes;
        // si dijo "No", saltamos 65-68 automáticamente
        return await AvanzarAsync(ses, _flujo.SiguienteNumero(ses.Cuestionario, 64));
    }

    // POST: /Cuestionario/SetFiltroJefe
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetFiltroJefe(FiltroJefeVM vm)
    {
        var ses = await CargarSesionAsync(vm.SesionId);
        if (ses is null) return NotFound();

        // Solo se acepta cuando el flujo está detenido en este filtro
        if (vm.EsJefe is null || ses.Cuestionario.Fin is not null ||
            ses.IndiceActual != 69 || ses.Cuestionario.EsJefe is not null)
            return RedirectToAction(nameof(Pregunta), new { id = ses.Token });

        ses.Cuestionario.EsJefe = vm.EsJefe;
        // si dijo "No", saltamos 69-72 automáticamente
        return await AvanzarAsync(ses, _flujo.SiguienteNumero(ses.Cuestionario, 68));
    }

    // GET: /Cuestionario/Gracias
    [HttpGet]
    public IActionResult Gracias() => View();

    private Task<CuestionarioSesion?> CargarSesionAsync(Guid token) =>
        _db.CuestionarioSesiones
            .Include(s => s.Cuestionario)
            .FirstOrDefaultAsync(s => s.Token == token);

    // Mueve la sesión a la pregunta indicada (o la cierra si ya no hay más) y guarda los cambios pendientes
    private async Task<IActionResult> AvanzarAsync(CuestionarioSesion ses, int siguiente)
    {
        if (siguiente > UltimaPregunta)
        {
            ses.IndiceActual = UltimaPregunta + 1;
            ses.Cuestionario.Fin ??= DateTime.UtcNow;
        }
        else
        {
            ses.IndiceActual = siguiente;
        }
        ses.AntiBackToken = Guid.NewGuid().ToString();

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
        {
            // Doble envío de la misma respuesta (índice único): se ignora y se retoma el flujo
            return RedirectToAction(nameof(Pregunta), new { id = ses.Token });
        }

        return ses.IndiceActual > UltimaPregunta
            ? RedirectToAction(nameof(Gracias))
            : RedirectToAction(nameof(Pregunta), new { id = ses.Token });
    }

    private static List<SelectListItem> AreasSelect() =>
        CatalogoAreas.Areas()
            .Select(a => new SelectListItem { Text = a, Value = a })
            .ToList();
}
