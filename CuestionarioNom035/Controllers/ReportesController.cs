using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOM35.Web.Data;

namespace NOM35.Web.Controllers
{
    [Route("Reportes")]
    public class ReportesController : Controller
    {
        private readonly Nom35DbContext _db;
        public ReportesController(Nom35DbContext db) => _db = db;

        // GET /Reportes/Validacion
        [HttpGet("Validacion")]
        public async Task<IActionResult> Validacion()
        {
            var total = await _db.Preguntas.CountAsync();
            ViewData["TotalPreguntas"] = total;
            return View();
        }
    }
}
