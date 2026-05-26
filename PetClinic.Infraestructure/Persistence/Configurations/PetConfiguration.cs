#nullable enable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetClinic.Domain.Entities;

namespace PetClinic.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para mapear la entidad Pet a la tabla Pets existente.
/// Schema: Id, OwnerId, Name, Species, Breed (nullable), BirthDate
/// </summary>
public sealed class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("Pets");

        builder.HasKey(p => p.Id);

        // OwnerId es UNIQUEIDENTIFIER NOT NULL (relación conceptual, sin FK física)
        builder.Property(p => p.OwnerId)
            .HasColumnName("OwnerId")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Species)
            .HasColumnName("Species")
            .HasMaxLength(50)
            .IsRequired();

        // Breed es NVARCHAR(100) NULL — puede ser nulo para mascotas mestizas
        builder.Property(p => p.Breed)
            .HasColumnName("Breed")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(p => p.BirthDate)
            .HasColumnName("BirthDate")
            .IsRequired();
    }
}
