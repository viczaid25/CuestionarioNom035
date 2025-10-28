using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    [HttpGet]
    public IActionResult Inicio()
    {
        var vm = new InicioVM();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Inicio(InicioVM vm)
    {
        // Reponer el select si vuelves con errores
        if (vm.AreasDisponibles == null || vm.AreasDisponibles.Count == 0)
        {
            vm.AreasDisponibles = new()
        {
            new SelectListItem { Text = "MELX", Value = "MELX" },
            new SelectListItem { Text = "SAL",  Value = "SAL" },
            new SelectListItem { Text = "IT",   Value = "IT"  },
            new SelectListItem { Text = "PD",   Value = "PD"  },
            new SelectListItem { Text = "LGL",  Value = "LGL" },
            new SelectListItem { Text = "PRC",  Value = "PRC" },
            new SelectListItem { Text = "TOP",  Value = "TOP" },
            new SelectListItem { Text = "HR",   Value = "HR"  },
            new SelectListItem { Text = "FIN",  Value = "FIN" },
            new SelectListItem { Text = "QC",   Value = "QC"  },
            new SelectListItem { Text = "PC",   Value = "PC"  },
            new SelectListItem { Text = "FC",   Value = "FC"  }
        };
        }

        if (!ModelState.IsValid) return View(vm);

        var permitidas = new HashSet<string>(new[]
        { "MELX","SAL","IT","PD","LGL","PRC","TOP","HR","FIN","QC","PC","FC" },
            StringComparer.OrdinalIgnoreCase);

        if (!permitidas.Contains(vm.Area))
        {
            ModelState.AddModelError(nameof(vm.Area), "Área no válida.");
            return View(vm);
        }

        if (vm.TipoEmpleado is null)
        {
            ModelState.AddModelError(nameof(vm.TipoEmpleado), "Selecciona el tipo de empleado.");
            return View(vm);
        }

        // Obtén o crea el participante
        var part = await _db.Participantes
            .FirstOrDefaultAsync(p => p.NumeroEmpleado == vm.NumeroEmpleado);

        if (part is null)
        {
            part = new Participante
            {
                NumeroEmpleado = vm.NumeroEmpleado,
                AreaNombre = vm.Area,
                TipoEmpleado = vm.TipoEmpleado.Value
            };
            _db.Participantes.Add(part);
            await _db.SaveChangesAsync();
        }
        else
        {
            // si quieres actualizar área/tipo al reingresar:
            part.AreaNombre = vm.Area;
            part.TipoEmpleado = vm.TipoEmpleado.Value;
            await _db.SaveChangesAsync();
        }

        var c = new Cuestionario
        {
            ParticipanteId = part.Id,
            Anio = DateTime.Now.Year   // o DateTime.UtcNow.Year
        };
        _db.Cuestionarios.Add(c);
        await _db.SaveChangesAsync();

        var ses = new CuestionarioSesion { CuestionarioId = c.Id, IndiceActual = 1 };
        _db.CuestionarioSesiones.Add(ses);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Pregunta), new { id = ses.Id });
    }

    // Helper para rellenar el Select si regresas la vista con errores
    private static List<SelectListItem> ObtenerAreasFijas() => new()
{
    new SelectListItem { Text = "MELX", Value = "MELX" },
    new SelectListItem { Text = "SAL",  Value = "SAL" },
    new SelectListItem { Text = "IT",   Value = "IT"  },
    new SelectListItem { Text = "PD",   Value = "PD"  },
    new SelectListItem { Text = "LGL",  Value = "LGL" },
    new SelectListItem { Text = "PRC",  Value = "PRC" },
    new SelectListItem { Text = "TOP",  Value = "TOP" },
    new SelectListItem { Text = "HR",   Value = "HR"  },
    new SelectListItem { Text = "FIN",  Value = "FIN" },
    new SelectListItem { Text = "QC",   Value = "QC"  },
    new SelectListItem { Text = "PC",   Value = "PC"  },
    new SelectListItem { Text = "FC",   Value = "FC"  }
};

    // GET: /Cuestionario/Pregunta/{id}
    [HttpGet]
    public async Task<IActionResult> Pregunta(int id)
    {
        var ses = await _db.CuestionarioSesiones
            .Include(s => s.Cuestionario)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (ses is null) return NotFound();

        // Si ya terminó, ir a resumen
        if (ses.IndiceActual > 72)
            return RedirectToAction(nameof(Resumen), new { id = ses.CuestionarioId });

        // Mostrar filtros cuando toca y el flag está sin responder
        if (ses.IndiceActual == 65 && ses.Cuestionario.AtiendeClientes is null)
            return View("FiltroClientes", new FiltroClientesVM { SesionId = id });

        if (ses.IndiceActual == 69 && ses.Cuestionario.EsJefe is null)
            return View("FiltroJefe", new FiltroJefeVM { SesionId = id });

        // Si el índice actual NO debe mostrarse (por los filtros), saltar al siguiente visible
        if (!_flujo.DebeMostrar(ses.IndiceActual, ses.Cuestionario))
        {
            var next = _flujo.SiguienteNumero(ses.Cuestionario, ses.IndiceActual - 1);
            if (next > 72)
            {
                ses.IndiceActual = 73;
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Resumen), new { id = ses.CuestionarioId });
            }
            ses.IndiceActual = next;
            ses.AntiBackToken = Guid.NewGuid().ToString();
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Pregunta), new { id = ses.Id });
        }

        // Cargar la pregunta del índice actual (usa FirstOrDefault y avanza si no existe)
        var pregunta = await _db.Preguntas
            .Include(p => p.Dimension).ThenInclude(d => d.Dominio).ThenInclude(o => o.Categoria)
            .FirstOrDefaultAsync(p => p.Numero == ses.IndiceActual);

        if (pregunta == null)
        {
            // Si no hay pregunta con ese número, avanza al siguiente visible
            var next = _flujo.SiguienteNumero(ses.Cuestionario, ses.IndiceActual);
            if (next > 72)
            {
                ses.IndiceActual = 73;
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Resumen), new { id = ses.CuestionarioId });
            }
            ses.IndiceActual = next;
            ses.AntiBackToken = Guid.NewGuid().ToString();
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Pregunta), new { id = ses.Id });
        }

        var vm = new PreguntaVM
        {
            SesionId = ses.Id,
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

        // Chequeos de decisión inmediatos:
        if (ses.IndiceActual == 64 && ses.Cuestionario.AtiendeClientes is null)
        {
            ses.IndiceActual = 65;                 // para que GET muestre FiltroClientes
            ses.AntiBackToken = Guid.NewGuid().ToString();
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Pregunta), new { id = ses.Id });
        }

        if (ses.IndiceActual == 68 && ses.Cuestionario.EsJefe is null)
        {
            ses.IndiceActual = 69;                 // para que GET muestre FiltroJefe
            ses.AntiBackToken = Guid.NewGuid().ToString();
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Pregunta), new { id = ses.Id });
        }

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

    // POST: /Cuestionario/FiltroClientes
    [HttpPost]
    public async Task<IActionResult> FiltroClientes(FiltroClientesVM vm)
    {
        var ses = await _db.CuestionarioSesiones
            .Include(s => s.Cuestionario)
            .FirstOrDefaultAsync(s => s.Id == vm.SesionId);
        if (ses is null) return NotFound();

        ses.Cuestionario.AtiendeClientes = vm.AtiendeClientes;
        await _db.SaveChangesAsync();

        // Partimos desde 65 para calcular el próximo visible
        var next = _flujo.SiguienteNumero(ses.Cuestionario, 65);
        if (next > 72)
        {
            ses.IndiceActual = 73;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Resumen), new { id = ses.CuestionarioId });
        }

        ses.IndiceActual = next;
        ses.AntiBackToken = Guid.NewGuid().ToString();
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Pregunta), new { id = ses.Id });
    }

    // POST: /Cuestionario/FiltroJefe
    [HttpPost]
    public async Task<IActionResult> FiltroJefe(FiltroJefeVM vm)
    {
        var ses = await _db.CuestionarioSesiones
            .Include(s => s.Cuestionario)
            .FirstOrDefaultAsync(s => s.Id == vm.SesionId);
        if (ses is null) return NotFound();

        ses.Cuestionario.EsJefe = vm.EsJefe;
        await _db.SaveChangesAsync();

        // Partimos desde 69 para calcular el próximo visible
        var next = _flujo.SiguienteNumero(ses.Cuestionario, 69);
        if (next > 72)
        {
            ses.IndiceActual = 73;
            ses.Cuestionario.Fin = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Resumen), new { id = ses.CuestionarioId });
        }

        ses.IndiceActual = next;
        ses.AntiBackToken = Guid.NewGuid().ToString();
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Pregunta), new { id = ses.Id });
    }

}
