#nullable enable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetClinic.Domain.Entities;

namespace PetClinic.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para mapear la entidad Administrator a la tabla Administrators existente.
/// Schema: Id, Name, Email, PasswordHash, CreatedAt, CreatedBy, UpdatedAt
/// </summary>
public sealed class AdministratorConfiguration : IEntityTypeConfiguration<Administrator>
{
    public void Configure(EntityTypeBuilder<Administrator> builder)
    {
        builder.ToTable("Administrators");

        builder.HasKey(a => a.Id);

        // Columna 'Name' — nombre real en la DB (NO 'Username')
        builder.Property(a => a.Name)
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Email)
            .HasColumnName("Email")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(a => a.PasswordHash)
            .HasColumnName("PasswordHash")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(a => a.Email)
            .IsUnique();
    }
}
