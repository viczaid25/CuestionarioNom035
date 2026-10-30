using System.Diagnostics;
using CuestionarioNom035.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuestionarioNom035.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Portada: elige Guía III (anónima) o Diagnóstico ATS (con sesión)
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        // Pide sesión de MEAX One (SSO) y regresa a la portada
        public IActionResult Entrar() => RedirectToAction(nameof(Index));

        public IActionResult SinPermiso()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
