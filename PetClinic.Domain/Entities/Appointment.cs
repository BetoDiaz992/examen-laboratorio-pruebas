#nullable enable

using System;
using PetClinic.Domain.Enums;
using PetClinic.Domain.Events;
using PetClinic.Domain.Exceptions;
using PetClinic.Domain.Primitives;

namespace PetClinic.Domain.Entities;

/// <summary>
/// Raíz de Agregado (Aggregate Root) que representa una Cita programada en la clínica veterinaria.
/// Alineada con tabla Appointments: Id, PetId, VeterinarianId, ScheduledTime, Reason, State
/// </summary>
public sealed class Appointment : Entity
{
    /// <summary>
    /// Identificador único de la mascota (Paciente) asociada.
    /// </summary>
    public Guid PetId { get; private set; }

    /// <summary>
    /// Propiedad de navegación para la Mascota.
    /// </summary>
    public Pet? Pet { get; private set; }

    /// <summary>
    /// Identificador único del veterinario (Médico) asociado.
    /// </summary>
    public Guid VeterinarianId { get; private set; }

    /// <summary>
    /// Propiedad de navegación para el Veterinario.
    /// </summary>
    public Veterinarian? Veterinarian { get; private set; }

    /// <summary>
    /// Fecha y hora de inicio de la cita (columna ScheduledTime).
    /// </summary>
    public DateTime ScheduledTime { get; private set; }

    /// <summary>
    /// Motivo o diagnóstico de la consulta (columna Reason).
    /// </summary>
    public string Reason { get; private set; }

    /// <summary>
    /// Estado actual de la cita como string: SCHEDULED, COMPLETED, CANCELLED (columna State).
    /// </summary>
    public AppointmentStatus State { get; private set; }

    // Constructor privado para EF Core
    private Appointment() : base(Guid.NewGuid())
    {
        Reason = string.Empty;
    }

    /// <summary>
    /// Instancia una nueva cita cumpliendo las invariantes de negocio.
    /// Emite un evento de dominio AppointmentScheduledEvent.
    /// </summary>
    public Appointment(Guid id, Guid petId, Guid veterinarianId, DateTime scheduledTime, string reason)
        : base(id)
    {
        if (petId == Guid.Empty)
        {
            throw new ArgumentException("El identificador de la mascota no puede estar vacío.", nameof(petId));
        }

        if (veterinarianId == Guid.Empty)
        {
            throw new ArgumentException("El identificador del veterinario no puede estar vacío.", nameof(veterinarianId));
        }

        if (scheduledTime < DateTime.UtcNow.AddMinutes(-5))
        {
            throw new InvalidAppointmentTimeException(scheduledTime, scheduledTime, "La fecha de la cita no puede ser en el pasado.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("El motivo de la cita no puede estar vacío.", nameof(reason));
        }

        PetId = petId;
        VeterinarianId = veterinarianId;
        ScheduledTime = scheduledTime;
        Reason = reason;
        State = AppointmentStatus.Programada; // REQ-CIT-03: Estado inicial SCHEDULED

        RaiseDomainEvent(new AppointmentScheduledEvent(
            AppointmentId: Id,
            PetId: PetId,
            VeterinarianId: VeterinarianId,
            StartTime: ScheduledTime,
            EndTime: ScheduledTime,
            OccurredOn: DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Reprograma la cita a un nuevo bloque de tiempo.
    /// </summary>
    public void Reschedule(DateTime newScheduledTime, string? newReason = null)
    {
        if (State == AppointmentStatus.Cancelada)
        {
            throw new InvalidOperationException("No se puede reprogramar una cita cancelada.");
        }

        if (State == AppointmentStatus.Completada)
        {
            throw new InvalidOperationException("No se puede reprogramar una cita ya completada.");
        }

        if (newScheduledTime < DateTime.UtcNow.AddMinutes(-5))
        {
            throw new InvalidAppointmentTimeException(newScheduledTime, newScheduledTime, "La nueva fecha no puede ser en el pasado.");
        }

        DateTime oldScheduled = ScheduledTime;
        ScheduledTime = newScheduledTime;

        if (!string.IsNullOrWhiteSpace(newReason))
        {
            Reason = newReason;
        }

        RaiseDomainEvent(new AppointmentRescheduledEvent(
            AppointmentId: Id,
            OldStart: oldScheduled,
            OldEnd: oldScheduled,
            NewStart: ScheduledTime,
            NewEnd: ScheduledTime,
            OccurredOn: DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Cancela la cita médica activa.
    /// </summary>
    public void Cancel()
    {
        if (State == AppointmentStatus.Cancelada)
        {
            return; // Idempotente
        }

        if (State == AppointmentStatus.Completada)
        {
            throw new InvalidOperationException("No se puede cancelar una cita que ya ha sido completada.");
        }

        State = AppointmentStatus.Cancelada;

        RaiseDomainEvent(new AppointmentCancelledEvent(
            AppointmentId: Id,
            OccurredOn: DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Completa la cita médica registrando las notas finales.
    /// </summary>
    public void Complete(string notes)
    {
        if (State == AppointmentStatus.Cancelada)
        {
            throw new InvalidOperationException("No se puede completar una cita cancelada.");
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            Reason = notes;
        }

        State = AppointmentStatus.Completada;
    }
}
