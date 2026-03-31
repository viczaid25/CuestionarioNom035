// Ruta: NOM35.Web/Controllers/ReportesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using NOM35.Web.Models;
using NOM35.Web.Services;
using NOM35.Web.ViewModels;
using System.Linq;
using CuestionarioNom035.ViewModels;
using CuestionarioNom035.Data;
using Microsoft.AspNetCore.Authorization;


namespace CuestionarioNom035.Controllers
{
    [Route("Reportes")]
    [Authorize(Policy = "CanViewReports")]
    public class ReportesController : Controller
    {
        private readonly Nom35DbContext _db;
        private readonly IScoringService _score;

        public ReportesController(Nom35DbContext db, IScoringService score)
        {
            _db = db;
            _score = score;
        }

        // ---------------------------
        // GET /Reportes/Validacion
        // ---------------------------
        [HttpGet("Validacion")]
        public async Task<IActionResult> Validacion()
        {
            var total = await _db.Preguntas.CountAsync();
            ViewData["TotalPreguntas"] = total;
            return View();
        }

        // -----------------------------------------
        // GET /Reportes/Evaluacion/{id} (individual)
        // -----------------------------------------
        [HttpGet("Evaluacion/{id}")]
        public async Task<IActionResult> Evaluacion(int id)
        {
            var cuestionario = await _db.Cuestionarios
                .Include(c => c.Participante)
                .Include(c => c.Respuestas)
                    .ThenInclude(r => r.Pregunta)
                        .ThenInclude(p => p.Dimension)
                            .ThenInclude(d => d.Dominio)
                                .ThenInclude(o => o.Categoria)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cuestionario == null) return NotFound();

            // Cfinal = suma total
            double cfinal = cuestionario.Respuestas
                .Sum(r => _score.MapearValor(r.NumeroPregunta, r.Opcion));

            var resGlobal = EvaluacionHelper.CalificarCuestionario(cfinal);

            // CATEGORÍAS (sumas)
            var categorias = cuestionario.Respuestas
                .GroupBy(r => r.Pregunta.Dimension.Dominio.Categoria.Nombre)
                .Select(g => new
                {
                    Nombre = g.Key,
                    Suma = g.Sum(r => _score.MapearValor(r.NumeroPregunta, r.Opcion))
                })
                .OrderBy(x => x.Nombre)
                .ToList();

            // DOMINIOS (sumas)
            var dominios = cuestionario.Respuestas
                .GroupBy(r => r.Pregunta.Dimension.Dominio.Nombre)
                .Select(g => new
                {
                    Nombre = g.Key,
                    Suma = g.Sum(r => _score.MapearValor(r.NumeroPregunta, r.Opcion))
                })
                .OrderBy(x => x.Nombre)
                .ToList();

            var vm = new EvaluacionVM
            {
                Cfinal = cfinal,
                NivelGlobal = resGlobal.Nivel,
                NivelGlobalColor = resGlobal.Color,
                CatLabels = categorias.Select(x => x.Nombre).ToList(),
                CatValues = categorias.Select(x => (double)x.Suma).ToList(),
                DomLabels = dominios.Select(x => x.Nombre).ToList(),
                DomValues = dominios.Select(x => (double)x.Suma).ToList()
            };

            return View(vm);
        }

        // ----------------------------------------------------
        // GET /Reportes/Dashboard?anio=2025&area=IT (agregado)
        // ----------------------------------------------------
        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard(int? anio, string? area, string? subarea)
        {
            var areasFijas = CatalogoAreas.Areas();
            int anioActual = DateTime.Now.Year;
            int anioFiltro = anio ?? anioActual;
            var anios = Enumerable.Range(anioActual - 4, 5).Reverse().ToList();

            var query = _db.Cuestionarios
                .Include(c => c.Participante)
                .Include(c => c.Respuestas)
                    .ThenInclude(r => r.Pregunta)
                        .ThenInclude(p => p.Dimension)
                            .ThenInclude(d => d.Dominio)
                                .ThenInclude(o => o.Categoria)
                .Where(c => c.Anio == anioFiltro);

            if (!string.IsNullOrWhiteSpace(area))
                query = query.Where(c => c.Participante.AreaNombre == area);

            if (!string.IsNullOrWhiteSpace(subarea))
                query = query.Where(c => c.Participante.SubareaNombre == subarea);

            var cuestionarios = await query.ToListAsync();

            var vm = new ReportesDashboardVM
            {
                Area = area,
                Subarea = subarea,
                Anio = anioFiltro,
                AreasDisponibles = areasFijas,
                SubareasDisponibles = string.IsNullOrWhiteSpace(area) ? new() : CatalogoAreas.Subareas(area),
                AniosDisponibles = anios,
                TotalCuestionarios = cuestionarios.Count
            };

            // Selects
            vm.AniosSelect = vm.AniosDisponibles
                .Select(y => new SelectListItem { Text = y.ToString(), Value = y.ToString(), Selected = vm.Anio == y })
                .ToList();

            vm.AreasSelect = new List<SelectListItem> { new() { Text = "(Todas)", Value = "" } };
            vm.AreasSelect.AddRange(vm.AreasDisponibles.Select(a => new SelectListItem { Text = a, Value = a, Selected = vm.Area == a }));

            vm.SubareasSelect = new List<SelectListItem> { new() { Text = "(Todas)", Value = "" } };
            if (!string.IsNullOrWhiteSpace(area))
            {
                vm.SubareasSelect.AddRange(vm.SubareasDisponibles.Select(s =>
                    new SelectListItem { Text = s, Value = s, Selected = vm.Subarea == s }));
            }

            if (cuestionarios.Count == 0) return View(vm);

            // Cálculos (igual que antes) …
            var cfinales = cuestionarios
                .Select(c => c.Respuestas.Sum(r => _score.MapearValor(r.NumeroPregunta, r.Opcion))).ToList();
            vm.CfinalPromedio = cfinales.Average();

            var categorias = cuestionarios
                .SelectMany(c => c.Respuestas.GroupBy(r => r.Pregunta.Dimension.Dominio.Categoria.Nombre)
                    .Select(g => new { Categoria = g.Key, Suma = g.Sum(r => _score.MapearValor(r.NumeroPregunta, r.Opcion)) }))
                .GroupBy(x => x.Categoria)
                .Select(g => new { Nombre = g.Key, Prom = g.Average(x => x.Suma) })
                .OrderBy(x => x.Nombre).ToList();

            vm.CatLabels = categorias.Select(x => x.Nombre).ToList();
            vm.CatValues = categorias.Select(x => Math.Round(x.Prom, 2)).ToList();

            var dominios = cuestionarios
                .SelectMany(c => c.Respuestas.GroupBy(r => r.Pregunta.Dimension.Dominio.Nombre)
                    .Select(g => new { Dominio = g.Key, Suma = g.Sum(r => _score.MapearValor(r.NumeroPregunta, r.Opcion)) }))
                .GroupBy(x => x.Dominio)
                .Select(g => new { Nombre = g.Key, Prom = g.Average(x => x.Suma) })
                .OrderBy(x => x.Nombre).ToList();

            vm.DomLabels = dominios.Select(x => x.Nombre).ToList();
            vm.DomValues = dominios.Select(x => Math.Round(x.Prom, 2)).ToList();

            var idl = cuestionarios.Where(c => c.Participante.TipoEmpleado == TipoEmpleado.IDL).ToList();
            var dl = cuestionarios.Where(c => c.Participante.TipoEmpleado == TipoEmpleado.DL).ToList();

            vm.TotalIDL = idl.Count;
            vm.TotalDL = dl.Count;

            vm.CfinalPromedioIDL = idl.Count == 0 ? 0 :
                idl.Select(c => c.Respuestas.Sum(r => _score.MapearValor(r.NumeroPregunta, r.Opcion))).Average();

            vm.CfinalPromedioDL = dl.Count == 0 ? 0 :
                dl.Select(c => c.Respuestas.Sum(r => _score.MapearValor(r.NumeroPregunta, r.Opcion))).Average();

            if (!string.IsNullOrWhiteSpace(area))
            {
                vm.Evaluaciones = cuestionarios.OrderByDescending(c => c.Inicio)
                    .Select(c => new ReportesDashboardVM.ItemEvaluacion
                    {
                        Id = c.Id,
                        NumeroEmpleado = c.Participante.NumeroEmpleado,
                        Area = c.Participante.AreaNombre,
                        Subarea = c.Participante.SubareaNombre,
                        TipoEmpleado = c.Participante.TipoEmpleado.ToString(),
                        Fecha = c.Inicio.ToString("yyyy-MM-dd"),
                        Cfinal = c.Respuestas.Sum(r => _score.MapearValor(r.NumeroPregunta, r.Opcion))
                    })
                    .ToList();
            }

            return View(vm);
        }

    }
}
