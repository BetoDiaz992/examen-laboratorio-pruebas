#nullable enable

using System;
using PetClinic.Domain.Primitives;

namespace PetClinic.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando se intenta agendar una cita para un veterinario que ya tiene
/// un bloque de tiempo ocupado total o parcialmente.
/// </summary>
public sealed class AppointmentConflictException : DomainException
{
    public Guid VeterinarianId { get; }
    public DateTime RequestedStart { get; }
    public DateTime RequestedEnd { get; }

    public AppointmentConflictException(Guid veterinarianId, DateTime requestedStart, DateTime requestedEnd)
        : base($"El veterinario con ID {veterinarianId} ya posee una cita activa que se cruza con el bloque horario solicitado: {requestedStart:yyyy-MM-dd HH:mm:ss} - {requestedEnd:yyyy-MM-dd HH:mm:ss}.")
    {
        VeterinarianId = veterinarianId;
        RequestedStart = requestedStart;
        RequestedEnd = requestedEnd;
    }
}
