using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CuestionarioNom035.Services;

namespace CuestionarioNom035.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ActiveDirectoryService _ad;
        private readonly IConfiguration _cfg;

        public AccountController(ActiveDirectoryService ad, IConfiguration cfg)
        {
            _ad = ad;
            _cfg = cfg;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            // 1) Validar AD
            if (!_ad.ValidateCredentials(username, password))
            {
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View();
            }

            // 2) Verificar que está en la lista permitida para Reportes
            var allowed = _cfg.GetSection("Reportes:AllowedUsers").Get<string[]>() ?? Array.Empty<string>();
            var u = (username ?? "").Trim().ToUpperInvariant();
            var canReports = allowed.Any(x => (x ?? "").Trim().ToUpperInvariant() == u);

            if (!canReports)
            {
                return RedirectToAction("Denied");
            }

            // 3) Claims
            var displayName = _ad.GetDisplayName(username, password);

            var claims = new List<Claim>
            {
                new Claim("SamAccountName", username),
                new Claim("DisplayName", string.IsNullOrWhiteSpace(displayName) ? username : displayName),
                new Claim("CanViewReports", "true")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            // 4) Regresar a donde iba (Reportes)
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Dashboard", "Reportes");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Inicio", "Cuestionario");
        }

        [HttpGet]
        public IActionResult Denied() => View();
    }
}
