using Microsoft.EntityFrameworkCore;
using NOM35.Web.Models;

namespace CuestionarioNom035.Data;

public class Nom35DbContext : DbContext
{
    public Nom35DbContext(DbContextOptions<Nom35DbContext> options) : base(options) { }

    public DbSet<Participante> Participantes => Set<Participante>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Dominio> Dominios => Set<Dominio>();
    public DbSet<Dimension> Dimensiones => Set<Dimension>();
    public DbSet<Pregunta> Preguntas => Set<Pregunta>();
    public DbSet<Cuestionario> Cuestionarios => Set<Cuestionario>();
    public DbSet<CuestionarioSesion> CuestionarioSesiones => Set<CuestionarioSesion>();
    public DbSet<Respuesta> Respuestas => Set<Respuesta>();
    public DbSet<AtsEvaluacion> AtsEvaluaciones => Set<AtsEvaluacion>();
    public DbSet<AtsRespuesta> AtsRespuestas => Set<AtsRespuesta>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<Pregunta>()
          .HasIndex(p => p.Numero)
          .IsUnique();

        mb.Entity<CuestionarioSesion>()
          .HasIndex(s => s.Token)
          .IsUnique();

        mb.Entity<CuestionarioSesion>()
          .Property(s => s.Token)
          .HasDefaultValueSql("NEWID()");

        // Diagnóstico ATS
        mb.Entity<AtsEvaluacion>(e =>
        {
            e.Property(x => x.Nomina).HasMaxLength(20);
            e.Property(x => x.PcLoginId).HasMaxLength(50);
            e.Property(x => x.NombreCompleto).HasMaxLength(150);
            e.Property(x => x.Departamento).HasMaxLength(100);
            e.Property(x => x.Posicion).HasMaxLength(100);
            e.Property(x => x.Antiguedad).HasMaxLength(50);
            e.Property(x => x.ComentarioDarDetalles).HasMaxLength(500);
            e.Property(x => x.DescripcionSuceso).HasMaxLength(4000);
            e.Property(x => x.ComentarioComunicarRH).HasMaxLength(500);
            e.Property(x => x.MotivoDecision).HasMaxLength(1000);
            e.Property(x => x.RevisadoPor).HasMaxLength(50);
            e.HasIndex(x => x.PcLoginId);
            e.HasIndex(x => x.Estado);
        });

        mb.Entity<AtsRespuesta>()
          .HasOne(r => r.Evaluacion)
          .WithMany(e => e.Respuestas)
          .HasForeignKey(r => r.EvaluacionId)
          .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<AtsRespuesta>()
          .HasIndex(r => new { r.EvaluacionId, r.Seccion, r.Numero })
          .IsUnique();

        // Una sola respuesta por pregunta en cada cuestionario
        mb.Entity<Respuesta>()
          .HasIndex(r => new { r.CuestionarioId, r.NumeroPregunta })
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
