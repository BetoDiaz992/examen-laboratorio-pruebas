#nullable enable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetClinic.Domain.Entities;

namespace PetClinic.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para mapear la entidad Veterinarian a la tabla Veterinarians existente.
/// Schema: Id, Name, Specialty, MedicalLicense, Email
/// </summary>
public sealed class VeterinarianConfiguration : IEntityTypeConfiguration<Veterinarian>
{
    public void Configure(EntityTypeBuilder<Veterinarian> builder)
    {
        builder.ToTable("Veterinarians");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name)
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.Specialty)
            .HasColumnName("Specialty")
            .HasMaxLength(100)
            .IsRequired();

        // Columna MedicalLicense — UNIQUE en la DB
        builder.Property(v => v.MedicalLicense)
            .HasColumnName("MedicalLicense")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(v => v.Email)
            .HasColumnName("Email")
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(v => v.MedicalLicense)
            .IsUnique();

        builder.HasIndex(v => v.Email)
            .IsUnique();
    }
}
