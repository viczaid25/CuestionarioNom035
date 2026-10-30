using CuestionarioNom035.Auth;
using CuestionarioNom035.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOM35.Web.Models;
using NOM35.Web.ViewModels;

namespace CuestionarioNom035.Controllers;

/// <summary>
/// Bandeja y resultados del Diagnóstico ATS. Exclusivo de Servicio Médico.
/// </summary>
[Authorize(Policy = Permisos.ServicioMedico)]
public class ServicioMedicoController : Controller
{
    private readonly Nom35DbContext _db;

    public ServicioMedicoController(Nom35DbContext db) => _db = db;

    // GET: /ServicioMedico?estado=PendienteRevision&buscar=...
    [HttpGet]
    public async Task<IActionResult> Index(string? estado = null, string? buscar = null)
    {
        // Sin estado = pendientes; "Todos" = sin filtro
        AtsEstado? filtro = estado switch
        {
            null or "" => AtsEstado.PendienteRevision,
            "Todos" => null,
            _ => Enum.TryParse<AtsEstado>(estado, out var e) ? e : AtsEstado.PendienteRevision
        };

        var conteos = await _db.AtsEvaluaciones
            .GroupBy(e => e.Estado)
            .Select(g => new { g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Total);

        var query = _db.AtsEvaluaciones.Include(e => e.Respuestas).AsQueryable();
        if (filtro is not null) query = query.Where(e => e.Estado == filtro);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var b = buscar.Trim();
            query = query.Where(e => e.Nomina.Contains(b) || e.NombreCompleto.Contains(b) || e.PcLoginId.Contains(b));
        }

        var casos = await query.OrderByDescending(e => e.Inicio).Take(500).ToListAsync();

        return View(new AtsBandejaVM
        {
            Estado = filtro,
            Buscar = buscar,
            Conteos = conteos,
            Casos = casos.Select(e => new AtsBandejaVM.Item
            {
                Id = e.Id,
                Fecha = e.Inicio,
                Nomina = e.Nomina,
                NombreCompleto = e.NombreCompleto,
                Departamento = e.Departamento,
                Estado = e.Estado,
                RequiereValoracion = AtsCatalogo.RequiereValoracion(e.Respuestas)
            }).ToList()
        });
    }

    // GET: /ServicioMedico/Caso/{id}
    [HttpGet]
    public async Task<IActionResult> Caso(int id)
    {
        var vm = await CasoAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Caso(AtsResultadosVM resultados)
    {
        var ev = await _db.AtsEvaluaciones.FirstOrDefaultAsync(e => e.Id == resultados.EvaluacionId);
        if (ev is null) return NotFound();

        // Solo los que contestaron todo (algún "Sí" en la sección I) llevan resultados
        if (ev.Estado is not (AtsEstado.PendienteRevision or AtsEstado.Revisado))
            return RedirectToAction(nameof(Caso), new { id = ev.Id });

        if (!ModelState.IsValid)
        {
            var vm = (await CasoAsync(ev.Id))!;
            vm.Resultados = resultados;
            return View(vm);
        }

        ev.TipoEvaluacion = resultados.TipoEvaluacion;
        ev.SeguimientoExterno = resultados.SeguimientoExterno;
        ev.SeguimientoInterno = resultados.SeguimientoInterno;
        ev.DecisionAcudir = resultados.DecisionAcudir;
        ev.MotivoDecision = string.IsNullOrWhiteSpace(resultados.MotivoDecision) ? null : resultados.MotivoDecision.Trim();
        ev.RevisadoPor = UsuarioActual.Login(User);
        ev.FechaRevision = DateTime.UtcNow;
        ev.Estado = AtsEstado.Revisado;
        await _db.SaveChangesAsync();

        TempData["Ok"] = "Resultados guardados.";
        return RedirectToAction(nameof(Caso), new { id = ev.Id });
    }

    // GET: /ServicioMedico/Imprimir/{id} — formato MX-1401-F1 para firmas
    [HttpGet]
    public async Task<IActionResult> Imprimir(int id)
    {
        var vm = await CasoAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    private async Task<AtsCasoVM?> CasoAsync(int id)
    {
        var ev = await _db.AtsEvaluaciones
            .Include(e => e.Respuestas)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return null;

        return new AtsCasoVM
        {
            Evaluacion = ev,
            RequiereValoracion = AtsCatalogo.RequiereValoracion(ev.Respuestas),
            Resultados = new AtsResultadosVM
            {
                EvaluacionId = ev.Id,
                TipoEvaluacion = ev.TipoEvaluacion,
                // Si aún no se revisa, se sugiere el criterio de la norma
                SeguimientoExterno = ev.SeguimientoExterno ?? (ev.Estado == AtsEstado.PendienteRevision ? AtsCatalogo.RequiereValoracion(ev.Respuestas) : null),
                SeguimientoInterno = ev.SeguimientoInterno,
                DecisionAcudir = ev.DecisionAcudir,
                MotivoDecision = ev.MotivoDecision
            }
        };
    }
}
