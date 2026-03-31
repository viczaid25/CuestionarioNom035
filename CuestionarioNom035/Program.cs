using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NOM35.Web.Services;
using NOM35.Web.Extensions;
using CuestionarioNom035.Data;
using CuestionarioNom035.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// DbContext (SQL Server)
builder.Services.AddDbContext<Nom35DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI Servicios
builder.Services.AddScoped<IFlujoService, FlujoService>();
builder.Services.AddScoped<IScoringService, ScoringService>();

// ✅ AD (LDAP)
builder.Services.AddSingleton(new ActiveDirectoryService("ad.meax.mx"));

// ✅ Auth Cookies (solo para Reportes)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "NOM35.Auth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Denied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(10);
    });

// ✅ Policy: Reportes
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewReports", p => p.RequireClaim("CanViewReports", "true"));
});

var app = builder.Build();

await app.EnsureMigratedAndSeededAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();   // ✅ IMPORTANTE
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cuestionario}/{action=Inicio}/{id?}");

app.MapControllerRoute(
    name: "reportes",
    pattern: "Reportes/{action=Validacion}/{id?}",
    defaults: new { controller = "Reportes" });

app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=Login}/{id?}",
    defaults: new { controller = "Account" });

app.Run();
