using Microsoft.EntityFrameworkCore;
using WariSalud.Core.Entities;

namespace WariSalud.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core para WariSalud.
/// Configura las relaciones, índices y restricciones del modelo de datos (T3.1).
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<Especialidad> Especialidades => Set<Especialidad>();
    public DbSet<HorarioMedico> HorariosMedico => Set<HorarioMedico>();
    public DbSet<ConfiguracionClinica> ConfiguracionClinica => Set<ConfiguracionClinica>();
    public DbSet<Cita> Citas => Set<Cita>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── Usuario ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Rol).IsRequired().HasMaxLength(20);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // ─── Paciente ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.NombreCompleto).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Telefono).HasMaxLength(20);
            entity.HasOne(p => p.Usuario)
                  .WithOne(u => u.Paciente)
                  .HasForeignKey<Paciente>(p => p.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Especialidad ─────────────────────────────────────────────────────
        modelBuilder.Entity<Especialidad>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.DuracionCitaMinutos).HasDefaultValue(30);
        });

        // ─── Medico ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Medico>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.NombreCompleto).IsRequired().HasMaxLength(200);
            entity.Property(m => m.NumeroColegiatura).IsRequired().HasMaxLength(50);
            entity.Property(m => m.Activo).HasDefaultValue(true);
            entity.HasOne(m => m.Usuario)
                  .WithOne(u => u.Medico)
                  .HasForeignKey<Medico>(m => m.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(m => m.Especialidad)
                  .WithMany(e => e.Medicos)
                  .HasForeignKey(m => m.EspecialidadId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── HorarioMedico ───────────────────────────────────────────────────
        modelBuilder.Entity<HorarioMedico>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.DiasLaborables).IsRequired().HasMaxLength(50);
            entity.HasOne(h => h.Medico)
                  .WithOne(m => m.HorarioMedico)
                  .HasForeignKey<HorarioMedico>(h => h.MedicoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── ConfiguracionClinica (tabla de una sola fila) ───────────────────
        modelBuilder.Entity<ConfiguracionClinica>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.DiasLaborables).IsRequired().HasMaxLength(50);
        });

        // ─── Cita ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Cita>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Estado).IsRequired().HasMaxLength(20);
            entity.Property(c => c.Motivo).HasMaxLength(500);
            entity.Property(c => c.DuracionMinutos).IsRequired();

            entity.HasOne(c => c.Paciente)
                  .WithMany(p => p.Citas)
                  .HasForeignKey(c => c.PacienteId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Medico)
                  .WithMany(m => m.Citas)
                  .HasForeignKey(c => c.MedicoId)
                  .OnDelete(DeleteBehavior.Restrict);

            // T3.4: Índice en CITA(MedicoId, FechaHora) para soporte de RNF04 (<500ms)
            entity.HasIndex(c => new { c.MedicoId, c.FechaHora })
                  .HasDatabaseName("IX_Cita_MedicoId_FechaHora");

            // Índice adicional para consultas por paciente/fecha
            entity.HasIndex(c => new { c.PacienteId, c.FechaHora })
                  .HasDatabaseName("IX_Cita_PacienteId_FechaHora");
        });

        // ─── Seed: ConfiguracionClinica por defecto ───────────────────────────
        modelBuilder.Entity<ConfiguracionClinica>().HasData(new ConfiguracionClinica
        {
            Id = 1,
            HoraApertura = new TimeOnly(8, 0),
            HoraCierre = new TimeOnly(20, 0),
            DiasLaborables = "1,2,3,4,5,6" // Lu–Sa
        });
    }
}
