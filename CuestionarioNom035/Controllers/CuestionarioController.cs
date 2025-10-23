using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOM35.Web.Data;
using NOM35.Web.Models;
using NOM35.Web.Services;
using NOM35.Web.ViewModels;

namespace NOM35.Web.Controllers;

public class CuestionarioController : Controller
{
    private readonly Nom35DbContext _db;
    private readonly IFlujoService _flujo;
    private readonly IScoringService _score;

    public CuestionarioController(Nom35DbContext db, IFlujoService flujo, IScoringService score)
    { _db = db; _flujo = flujo; _score = score; }

    // GET: /Cuestionario/Inicio
    public IActionResult Inicio() => View(new InicioVM());

    // POST: crea Cuestionario + Sesión
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Inicio(InicioVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // Obtén o crea el participante
        var part = await _db.Participantes.FirstOrDefaultAsync(p => p.NumeroEmpleado == vm.NumeroEmpleado);
        if (part is null)
        {
            // valida que exista el Area
            var area = await _db.Areas.FindAsync(vm.AreaId);
            if (area is null)
            {
                ModelState.AddModelError(nameof(vm.AreaId), "Área no válida.");
                return View(vm);
            }
            part = new Participante { NumeroEmpleado = vm.NumeroEmpleado, AreaId = vm.AreaId };
            _db.Participantes.Add(part);
            await _db.SaveChangesAsync();
        }

        var c = new Cuestionario { ParticipanteId = part.Id };
        _db.Cuestionarios.Add(c);
        await _db.SaveChangesAsync();

        var ses = new CuestionarioSesion { CuestionarioId = c.Id, IndiceActual = 1 };
        _db.CuestionarioSesiones.Add(ses);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Pregunta), new { id = ses.Id });
    }

    // GET: /Cuestionario/Pregunta/{sesionId}
    public async Task<IActionResult> Pregunta(int id)
    {
        var ses = await _db.CuestionarioSesiones
            .Include(s => s.Cuestionario)
            .FirstAsync(s => s.Id == id);

        // Filtro clientes antes de 65
        if (ses.IndiceActual == 65 && ses.Cuestionario.AtiendeClientes is null)
            return View("FiltroClientes", new FiltroClientesVM { SesionId = id });

        // Filtro jefe antes de 69
        if (ses.IndiceActual == 69 && ses.Cuestionario.EsJefe is null)
            return View("FiltroJefe", new FiltroJefeVM { SesionId = id });

        var pregunta = await _db.Preguntas
            .Include(p => p.Dimension)
                .ThenInclude(d => d.Dominio)
                    .ThenInclude(o => o.Categoria)
            .FirstAsync(p => p.Numero == ses.IndiceActual);

        var vm = new PreguntaVM
        {
            SesionId = id,
            AntiBackToken = ses.AntiBackToken,
            Numero = pregunta.Numero,
            Texto = pregunta.Texto,
            Categoria = pregunta.Dimension.Dominio.Categoria.Nombre,
            Dominio = pregunta.Dimension.Dominio.Nombre,
            Dimension = pregunta.Dimension.Nombre
        };

        return View("Pregunta", vm);
    }

    // POST: /Cuestionario/Responder
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Responder(PreguntaVM vm)
    {
        var ses = await _db.CuestionarioSesiones
            .Include(s => s.Cuestionario)
            .FirstAsync(s => s.Id == vm.SesionId);

        if (vm.AntiBackToken != ses.AntiBackToken || vm.Numero != ses.IndiceActual)
            return BadRequest("Secuencia inválida. No se permite regresar.");

        var pregunta = await _db.Preguntas.FirstAsync(p => p.Numero == vm.Numero);

        _db.Respuestas.Add(new Respuesta
        {
            CuestionarioId = ses.CuestionarioId,
            NumeroPregunta = vm.Numero,
            PreguntaId = pregunta.Id,
            Opcion = vm.Seleccion
        });
        await _db.SaveChangesAsync();

        int next = _flujo.SiguienteNumero(ses.Cuestionario, ses.IndiceActual);
        if (next > 72)
        {
            ses.Cuestionario.Fin = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Resumen), new { id = ses.CuestionarioId });
        }

        ses.IndiceActual = next;
        ses.AntiBackToken = Guid.NewGuid().ToString();
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Pregunta), new { id = ses.Id });
    }

    // POST: /Cuestionario/SetFiltroClientes
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetFiltroClientes(FiltroClientesVM vm)
    {
        var ses = await _db.CuestionarioSesiones
            .Include(s => s.Cuestionario)
            .FirstAsync(s => s.Id == vm.SesionId);

        ses.Cuestionario.AtiendeClientes = vm.AtiendeClientes;
        // si dijo "No", saltamos 65-68 automáticamente
        int next = _flujo.SiguienteNumero(ses.Cuestionario, 64);
        ses.IndiceActual = next;
        ses.AntiBackToken = Guid.NewGuid().ToString();

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Pregunta), new { id = ses.Id });
    }

    // POST: /Cuestionario/SetFiltroJefe
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetFiltroJefe(FiltroJefeVM vm)
    {
        var ses = await _db.CuestionarioSesiones
            .Include(s => s.Cuestionario)
            .FirstAsync(s => s.Id == vm.SesionId);

        ses.Cuestionario.EsJefe = vm.EsJefe;
        // si dijo "No", saltamos 69-72 automáticamente
        int next = _flujo.SiguienteNumero(ses.Cuestionario, 68);
        ses.IndiceActual = next;
        ses.AntiBackToken = Guid.NewGuid().ToString();

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Pregunta), new { id = ses.Id });
    }

    public async Task<IActionResult> Resumen(int id)
    {
        var c = await _db.Cuestionarios
            .Include(x => x.Respuestas)
                .ThenInclude(r => r.Pregunta)
                    .ThenInclude(p => p.Dimension)
                        .ThenInclude(d => d.Dominio)
                            .ThenInclude(o => o.Categoria)
            .FirstAsync(x => x.Id == id);

        var (total, porCat, porDom, porDim) = _score.CalcularTotales(c);
        return View(new ResumenVM
        {
            CuestionarioId = id,
            Total = total,
            PorCategoria = porCat,
            PorDominio = porDom,
            PorDimension = porDim
        });
    }

    // GET: /Reportes/Validacion
    [HttpGet]
    public async Task<IActionResult> Validacion()
    {
        var preguntas = await _db.Preguntas
            .Include(p => p.Dimension)
                .ThenInclude(d => d.Dominio)
                    .ThenInclude(o => o.Categoria)
            .OrderBy(p => p.Numero)
            .ToListAsync();

        var vm = new ValidacionCargaVM
        {
            TotalPreguntas = preguntas.Count,
            PorCategoria = preguntas
                .GroupBy(p => p.Dimension.Dominio.Categoria.Nombre)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count()),
            PorDominio = preguntas
                .GroupBy(p => $"{p.Dimension.Dominio.Categoria.Nombre} > {p.Dimension.Dominio.Nombre}")
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count()),
            PorDimension = preguntas
                .GroupBy(p => $"{p.Dimension.Dominio.Categoria.Nombre} > {p.Dimension.Dominio.Nombre} > {p.Dimension.Nombre}")
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count()),
            Muestra = preguntas
                .Take(10)
                .Select(p => (p.Numero,
                              p.Texto,
                              p.Dimension.Dominio.Categoria.Nombre,
                              p.Dimension.Dominio.Nombre,
                              p.Dimension.Nombre))
                .ToList()
        };

        return View(vm);
    }
}
