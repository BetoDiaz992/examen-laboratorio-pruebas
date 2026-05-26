#nullable enable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetClinic.Domain.Entities;
using PetClinic.Domain.Enums;

namespace PetClinic.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para mapear la entidad Appointment a la tabla Appointments existente.
/// Schema: Id, PetId, VeterinarianId, ScheduledTime, Reason, State (NVARCHAR)
/// </summary>
public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        // Columna ScheduledTime (fecha/hora de inicio) — NO StartTime, NO EndTime
        builder.Property(a => a.ScheduledTime)
            .HasColumnName("ScheduledTime")
            .IsRequired();

        // Columna Reason (motivo) — NO Diagnosis
        builder.Property(a => a.Reason)
            .HasColumnName("Reason")
            .HasMaxLength(500)
            .IsRequired();

        // Columna State (NVARCHAR 30): SCHEDULED / COMPLETED / CANCELLED
        builder.Property(a => a.State)
            .HasColumnName("State")
            .HasMaxLength(30)
            .IsRequired()
            .HasConversion(
                v => v == AppointmentStatus.Programada ? "SCHEDULED"
                   : v == AppointmentStatus.Completada ? "COMPLETED"
                   : "CANCELLED",
                v => v == "SCHEDULED" ? AppointmentStatus.Programada
                   : v == "COMPLETED" ? AppointmentStatus.Completada
                   : AppointmentStatus.Cancelada
            );

        // Relaciones y claves foráneas explícitas
        builder.HasOne(a => a.Pet)
            .WithMany()
            .HasForeignKey(a => a.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Veterinarian)
            .WithMany()
            .HasForeignKey(a => a.VeterinarianId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
