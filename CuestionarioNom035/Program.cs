using Microsoft.EntityFrameworkCore;
using NOM35.Web.Data;
using NOM35.Web.Services;
using NOM35.Web.Extensions;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// DbContext (SQL Server)
builder.Services.AddDbContext<Nom35DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI Servicios
builder.Services.AddScoped<IFlujoService, FlujoService>();
builder.Services.AddScoped<IScoringService, ScoringService>();

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
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cuestionario}/{action=Inicio}/{id?}");

app.MapControllerRoute(
    name: "reportes",
    pattern: "Reportes/{action=Validacion}/{id?}",
    defaults: new { controller = "Reportes" });

app.Run();

