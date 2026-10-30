using System.Security.Claims;

namespace CuestionarioNom035.Auth;

/// <summary>
/// La persona de la sesion, leida de los claims de MEAX One. En IIS
/// <c>Environment.UserName</c> es la cuenta del App Pool, no quien captura.
/// </summary>
public static class UsuarioActual
{
    /// <summary>
    /// Cuenta con la que entro (PcLoginId): para un IDL es su cuenta de AD, para un DL su
    /// nomina.
    /// </summary>
    public static string Login(ClaimsPrincipal user) =>
        user.FindFirstValue(MeaxClaims.PcLoginId) is { Length: > 0 } login
            ? login
            : user.Identity?.Name ?? string.Empty;

    public static string Nombre(ClaimsPrincipal user) =>
        user.FindFirstValue(MeaxClaims.DisplayName) is { Length: > 0 } nombre ? nombre : Login(user);

    /// <summary>La nomina (EmployeeId). Es la llave de la persona.</summary>
    public static string Nomina(ClaimsPrincipal user) =>
        user.FindFirstValue(MeaxClaims.EmployeeId) ?? string.Empty;

    public static string Iniciales(string nombre)
    {
        var partes = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return partes.Length switch
        {
            0 => "?",
            1 => partes[0][..Math.Min(2, partes[0].Length)].ToUpperInvariant(),
            _ => $"{partes[0][0]}{partes[1][0]}".ToUpperInvariant(),
        };
    }
}
