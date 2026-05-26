#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetClinic.Domain.Entities;
using PetClinic.Domain.Primitives;

namespace PetClinic.Infrastructure.Persistence;

/// <summary>
/// Contexto de Base de Datos principal para PetClinic usando Entity Framework Core.
/// Implementa auditoría automática mediante Shadow Properties y mapeos avanzados con Fluent API.
/// </summary>
public class PetClinicDbContext : DbContext
{
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<Administrator> Administrators { get; set; } = null!;
    public DbSet<Pet> Pets { get; set; } = null!;
    public DbSet<Veterinarian> Veterinarians { get; set; } = null!;

    public PetClinicDbContext(DbContextOptions<PetClinicDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Configuración de Shadow Properties (Propiedades de Sombra) para todas las entidades del dominio
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Property<DateTime>("CreatedAt");
                modelBuilder.Entity(entityType.ClrType).Property<DateTime?>("UpdatedAt");
                modelBuilder.Entity(entityType.ClrType).Property<string>("CreatedBy")
                    .HasMaxLength(100)
                    .IsRequired();
            }
        }

        // 2. Mapeo explícito con Fluent API (Configuraciones de claves y relaciones)
        modelBuilder.Entity<Appointment>(builder =>
        {
            builder.ToTable("Appointments");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Reason).HasMaxLength(500).IsRequired();

            // Configurar relación 1:N Cita -> Mascota
            builder.HasOne<Pet>()
                .WithMany()
                .HasForeignKey(a => a.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar relación 1:N Cita -> Veterinario
            builder.HasOne<Veterinarian>()
                .WithMany()
                .HasForeignKey(a => a.VeterinarianId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Pet>(builder =>
        {
            builder.ToTable("Pets");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
            builder.Property(p => p.Species).HasMaxLength(50).IsRequired();
            builder.Property(p => p.Breed).HasMaxLength(100);
        });

        modelBuilder.Entity<Veterinarian>(builder =>
        {
            builder.ToTable("Veterinarians");
            builder.HasKey(v => v.Id);
            builder.Property(v => v.Name).HasMaxLength(100).IsRequired();
            builder.Property(v => v.Specialty).HasMaxLength(100).IsRequired();
            builder.Property(v => v.MedicalLicense).HasMaxLength(50).IsRequired();
            builder.HasIndex(v => v.MedicalLicense).IsUnique(); // Índice Único
        });

        modelBuilder.Entity<Administrator>(builder =>
        {
            builder.ToTable("Administrators");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Name).HasMaxLength(100).IsRequired();
            builder.Property(a => a.Email).HasMaxLength(150).IsRequired();
            builder.HasIndex(a => a.Email).IsUnique();
        });
    }

    /// <summary>
    /// Sobrescribe SaveChangesAsync para interceptar de manera automática el ciclo de vida de guardado 
    /// y poblar dinámicamente las Shadow Properties de auditoría.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Entity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                // Inyectar el actor administrador único por defecto
                entry.Property("CreatedBy").CurrentValue = "SystemAdmin"; 
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
