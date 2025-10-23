// Ruta: NOM35.Web/Extensions/HostExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NOM35.Web.Data;
using NOM35.Web.Data.Seed;

namespace NOM35.Web.Extensions;

public static class HostExtensions
{
    public static async Task EnsureMigratedAndSeededAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var db = services.GetRequiredService<Nom35DbContext>();
        var env = services.GetRequiredService<IWebHostEnvironment>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Seeder");

        await InitialSeeder.RunAsync(db, env, logger);
    }
}
