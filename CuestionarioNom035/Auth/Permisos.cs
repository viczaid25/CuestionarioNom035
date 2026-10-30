using Microsoft.AspNetCore.Authorization;

namespace CuestionarioNom035.Auth;

/// <summary>
/// Permisos dentro del sistema. El SSO ya filtró quién entra; aquí solo se decide quién ve
/// Reportes (Guía III) y quién ve el módulo de Servicio Médico (ATS). Las listas viven en
/// appsettings (Reportes:AllowedUsers, ServicioMedico:AllowedUsers) con la cuenta PcLoginId.
/// </summary>
public static class Permisos
{
    public const string Reportes = "Reportes";
    public const string ServicioMedico = "ServicioMedico";

    public static void AgregarPoliticas(AuthorizationOptions options, IConfiguration config)
    {
        options.AddPolicy(Reportes, p => p.RequireAssertion(ctx => EnLista(config, "Reportes:AllowedUsers", ctx.User)));
        options.AddPolicy(ServicioMedico, p => p.RequireAssertion(ctx => EnLista(config, "ServicioMedico:AllowedUsers", ctx.User)));
    }

    private static bool EnLista(IConfiguration config, string seccion, System.Security.Claims.ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true) return false;
        var login = UsuarioActual.Login(user).Trim();
        var permitidos = config.GetSection(seccion).Get<string[]>() ?? Array.Empty<string>();
        return login.Length > 0 && permitidos.Any(x => string.Equals((x ?? "").Trim(), login, StringComparison.OrdinalIgnoreCase));
    }
}
