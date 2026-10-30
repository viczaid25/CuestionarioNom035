using CuestionarioNom035.Auth;
using CuestionarioNom035.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOM35.Web.Models;
using NOM35.Web.ViewModels;

namespace CuestionarioNom035.Controllers;

/// <summary>
/// Diagnóstico ATS para el colaborador (requiere sesión de MEAX One). El colaborador nunca ve
/// resultados: al terminar solo se le agradece.
/// </summary>
public class AtsController : Controller
{
    private const int PasoConsentimientos = 5;

    private readonly Nom35DbContext _db;

    public AtsController(Nom35DbContext db) => _db = db;

    // GET: /Ats — datos del colaborador, o continúa el que dejó a medias
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var enProceso = await EnProcesoAsync();
        if (enProceso is not null) return IrAlPaso(enProceso);

        var nomina = UsuarioActual.Nomina(User);
        return View(new AtsDatosVM
        {
            Nomina = nomina,
            NominaDeSesion = nomina.Length > 0,
            NombreCompleto = UsuarioActual.Nombre(User),
            Departamento = User.FindFirst(MeaxClaims.Department)?.Value ?? "",
            Posicion = User.FindFirst(MeaxClaims.Position)?.Value ?? ""
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AtsDatosVM vm)
    {
        // La nómina de la sesión manda sobre lo que venga en el formulario
        var nominaSesion = UsuarioActual.Nomina(User);
        if (nominaSesion.Length > 0)
        {
            vm.Nomina = nominaSesion;
            vm.NominaDeSesion = true;
            ModelState.Remove(nameof(vm.Nomina));
        }

        if (!ModelState.IsValid) return View(vm);

        var enProceso = await EnProcesoAsync();
        if (enProceso is not null) return IrAlPaso(enProceso);

        var ev = new AtsEvaluacion
        {
            Nomina = vm.Nomina.Trim(),
            PcLoginId = UsuarioActual.Login(User),
            NombreCompleto = vm.NombreCompleto.Trim(),
            Departamento = vm.Departamento.Trim(),
            Posicion = vm.Posicion.Trim(),
            Edad = vm.Edad!.Value,
            Antiguedad = vm.Antiguedad.Trim()
        };
        _db.AtsEvaluaciones.Add(ev);
        await _db.SaveChangesAsync();

        return IrAlPaso(ev);
    }

    // GET: /Ats/Seccion/{id}
    [HttpGet]
    public async Task<IActionResult> Seccion(int id)
    {
        var ev = await PropiaAsync(id);
        if (ev is null) return NotFound();
        if (ev.Paso > 4 || ev.Fin is not null) return IrAlPaso(ev);

        return View(new AtsSeccionVM
        {
            EvaluacionId = ev.Id,
            Paso = ev.Paso,
            Seccion = AtsCatalogo.Seccion((AtsSeccion)ev.Paso)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Seccion(AtsSeccionVM vm)
    {
        var ev = await PropiaAsync(vm.EvaluacionId);
        if (ev is null) return NotFound();

        // Solo se acepta la sección en la que va (no se regresa ni se salta)
        if (ev.Fin is not null || vm.Paso != ev.Paso || ev.Paso > 4) return IrAlPaso(ev);

        var seccion = AtsCatalogo.Seccion((AtsSeccion)ev.Paso);
        var numeros = Enumerable.Range(1, seccion.Preguntas.Count).ToList();
        if (numeros.Any(n => !vm.Respuestas.TryGetValue(n, out var r) || r is null))
        {
            ModelState.AddModelError("", "Contesta todas las preguntas.");
            vm.Seccion = seccion;
            return View(vm);
        }

        foreach (var n in numeros)
            _db.AtsRespuestas.Add(new AtsRespuesta { EvaluacionId = ev.Id, Seccion = seccion.Seccion, Numero = n, Si = vm.Respuestas[n]!.Value });

        // Todo "No" en la sección I: ya no se contesta lo demás
        if (seccion.Seccion == AtsSeccion.I && numeros.All(n => vm.Respuestas[n] == false))
        {
            ev.Estado = AtsEstado.SinAts;
            ev.Fin = DateTime.UtcNow;
        }
        else
        {
            ev.Paso++;
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
        {
            // Doble envío de la misma sección: se ignora
        }

        return IrAlPaso(ev);
    }

    // GET: /Ats/Consentimientos/{id}
    [HttpGet]
    public async Task<IActionResult> Consentimientos(int id)
    {
        var ev = await PropiaAsync(id);
        if (ev is null) return NotFound();
        if (ev.Paso != PasoConsentimientos || ev.Fin is not null) return IrAlPaso(ev);

        return View(new AtsConsentimientosVM { EvaluacionId = ev.Id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Consentimientos(AtsConsentimientosVM vm)
    {
        var ev = await PropiaAsync(vm.EvaluacionId);
        if (ev is null) return NotFound();
        if (ev.Paso != PasoConsentimientos || ev.Fin is not null) return IrAlPaso(ev);

        if (vm.AceptaDarDetalles == true && string.IsNullOrWhiteSpace(vm.DescripcionSuceso))
            ModelState.AddModelError(nameof(vm.DescripcionSuceso), "Describe la situación.");

        if (!ModelState.IsValid) return View(vm);

        ev.AceptaDarDetalles = vm.AceptaDarDetalles;
        ev.ComentarioDarDetalles = Limpio(vm.ComentarioDarDetalles);
        ev.DescripcionSuceso = vm.AceptaDarDetalles == true ? Limpio(vm.DescripcionSuceso) : null;
        ev.AceptaComunicarRH = vm.AceptaComunicarRH;
        ev.ComentarioComunicarRH = Limpio(vm.ComentarioComunicarRH);
        ev.Estado = AtsEstado.PendienteRevision;
        ev.Fin = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Gracias));
    }

    // GET: /Ats/Gracias
    [HttpGet]
    public IActionResult Gracias() => View();

    private Task<AtsEvaluacion?> EnProcesoAsync()
    {
        var login = UsuarioActual.Login(User);
        return _db.AtsEvaluaciones
            .Where(e => e.PcLoginId == login && e.Estado == AtsEstado.EnProceso)
            .OrderByDescending(e => e.Inicio)
            .FirstOrDefaultAsync();
    }

    // Solo el dueño puede ver/contestar su evaluación
    private Task<AtsEvaluacion?> PropiaAsync(int id)
    {
        var login = UsuarioActual.Login(User);
        return _db.AtsEvaluaciones.FirstOrDefaultAsync(e => e.Id == id && e.PcLoginId == login);
    }

    private IActionResult IrAlPaso(AtsEvaluacion ev)
    {
        if (ev.Fin is not null) return RedirectToAction(nameof(Gracias));
        return ev.Paso >= PasoConsentimientos
            ? RedirectToAction(nameof(Consentimientos), new { id = ev.Id })
            : RedirectToAction(nameof(Seccion), new { id = ev.Id });
    }

    private static string? Limpio(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
