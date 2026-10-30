using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using NOM35.Web.Services;
using NOM35.Web.Extensions;
using CuestionarioNom035.Auth;
using CuestionarioNom035.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// DbContext (SQL Server)
builder.Services.AddDbContext<Nom35DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI Servicios
builder.Services.AddScoped<IFlujoService, FlujoService>();
builder.Services.AddScoped<IScoringService, ScoringService>();

// Login: MEAX One (SSO) o, en desarrollo, usuario de prueba
var modoDev = DevAuth.Habilitado(builder.Environment, builder.Configuration);
if (modoDev) builder.Services.AddDevAuth();
else builder.Services.AddHubSso(builder.Configuration);

// Con sesión pero sin permiso (Reportes / Servicio Médico): página propia
builder.Services.PostConfigure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme,
    o => o.AccessDeniedPath = "/Home/SinPermiso");

// Todo pide sesión salvo lo marcado con [AllowAnonymous] (Guía III, que es anónima)
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    Permisos.AgregarPoliticas(options, builder.Configuration);
});

// Llaves de DataProtection: solo si la carpeta existe en el servidor con permiso de
// escritura para el App Pool; si no, vacío (las sesiones se renuevan solas por SSO).
if (builder.Configuration["DataProtection:KeysPath"] is { Length: > 0 } keys)
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keys))
        .SetApplicationName(HubSso.SystemCode(builder.Configuration));

var app = builder.Build();

await app.EnsureMigratedAndSeededAsync();

if (app.Configuration["App:PathBase"] is { Length: > 0 } pathBase) app.UsePathBase(pathBase);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
if (!modoDev) app.UseHubSso(app.Configuration);
app.UseAuthorization();

if (!modoDev) app.MapHubSsoLogout(app.Configuration);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
