using Microsoft.EntityFrameworkCore;
using NOM35.Web.Models;

namespace NOM35.Web.Data;

public class Nom35DbContext : DbContext
{
    public Nom35DbContext(DbContextOptions<Nom35DbContext> options) : base(options) { }

    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Participante> Participantes => Set<Participante>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Dominio> Dominios => Set<Dominio>();
    public DbSet<Dimension> Dimensiones => Set<Dimension>();
    public DbSet<Pregunta> Preguntas => Set<Pregunta>();
    public DbSet<Cuestionario> Cuestionarios => Set<Cuestionario>();
    public DbSet<CuestionarioSesion> CuestionarioSesiones => Set<CuestionarioSesion>();
    public DbSet<Respuesta> Respuestas => Set<Respuesta>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<Pregunta>()
          .HasIndex(p => p.Numero)
          .IsUnique();

        mb.Entity<Dominio>()
          .HasOne(d => d.Categoria)
          .WithMany(c => c.Dominios)
          .HasForeignKey(d => d.CategoriaId)
          .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Dimension>()
          .HasOne(d => d.Dominio)
          .WithMany(o => o.Dimensiones)
          .HasForeignKey(d => d.DominioId)
          .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Pregunta>()
          .HasOne(p => p.Dimension)
          .WithMany(d => d.Preguntas)
          .HasForeignKey(p => p.DimensionId)
          .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Participante>()
          .HasOne(p => p.Area)
          .WithMany()
          .HasForeignKey(p => p.AreaId)
          .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Respuesta>()
          .HasOne(r => r.Cuestionario)
          .WithMany(c => c.Respuestas)
          .HasForeignKey(r => r.CuestionarioId)
          .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Respuesta>()
          .HasOne(r => r.Pregunta)
          .WithMany()
          .HasForeignKey(r => r.PreguntaId)
          .OnDelete(DeleteBehavior.Restrict);
    }
}
