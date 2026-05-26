#nullable enable

using System;
using PetClinic.Domain.Primitives;

namespace PetClinic.Domain.Events;

/// <summary>
/// Evento de dominio emitido cuando una cita es cancelada.
/// </summary>
public sealed record AppointmentCancelledEvent(
    Guid AppointmentId,
    DateTime OccurredOn) : IDomainEvent;
