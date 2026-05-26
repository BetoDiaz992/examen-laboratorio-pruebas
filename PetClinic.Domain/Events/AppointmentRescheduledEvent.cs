#nullable enable

using System;
using PetClinic.Domain.Primitives;

namespace PetClinic.Domain.Events;

/// <summary>
/// Evento de dominio emitido cuando una cita existente es reprogramada.
/// </summary>
public sealed record AppointmentRescheduledEvent(
    Guid AppointmentId,
    DateTime OldStart,
    DateTime OldEnd,
    DateTime NewStart,
    DateTime NewEnd,
    DateTime OccurredOn) : IDomainEvent;
