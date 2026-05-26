#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetClinic.Domain.Entities;
using PetClinic.Domain.Primitives;

namespace PetClinic.Infrastructure.Persistence;

/// <summary>
/// Contexto de Entity Framework Core para la base de datos de PetClinic.
/// </summary>
public sealed class PetClinicDbContext : DbContext
{
    public DbSet<Administrator> Administrators => Set<Administrator>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Veterinarian> Veterinarians => Set<Veterinarian>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    public PetClinicDbContext(DbContextOptions<PetClinicDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas las configuraciones Fluent API del ensamblado actual
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetClinicDbContext).Assembly);

        // Configuración masiva de Shadow Properties de auditoría para todas las entidades
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Property<DateTime>("CreatedAt")
                    .IsRequired();

                modelBuilder.Entity(entityType.ClrType).Property<DateTime?>("UpdatedAt")
                    .IsRequired(false);

                modelBuilder.Entity(entityType.ClrType).Property<string>("CreatedBy")
                    .HasMaxLength(100)
                    .IsRequired();
            }
        }
    }

    /// <summary>
    /// Intercepta y actualiza automáticamente los metadatos de auditoría al guardar de forma asíncrona.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditProperties();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Intercepta y actualiza automáticamente los metadatos de auditoría al guardar de forma síncrona.
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditProperties();
        return base.SaveChanges();
    }

    private void UpdateAuditProperties()
    {
        var entries = ChangeTracker.Entries<Entity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                entry.Property("CreatedBy").CurrentValue = "SystemAdmin"; // Inyección por Administrador Único (REQ-SEG-03)
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
