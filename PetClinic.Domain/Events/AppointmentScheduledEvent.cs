#nullable enable

using System;
using PetClinic.Domain.Primitives;

namespace PetClinic.Domain.Events;

/// <summary>
/// Evento de dominio emitido cuando una nueva cita es programada exitosamente.
/// </summary>
public sealed record AppointmentScheduledEvent(
    Guid AppointmentId,
    Guid PetId,
    Guid VeterinarianId,
    DateTime StartTime,
    DateTime EndTime,
    DateTime OccurredOn) : IDomainEvent;
