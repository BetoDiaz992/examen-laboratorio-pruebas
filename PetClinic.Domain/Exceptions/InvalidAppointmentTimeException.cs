#nullable enable

using System;
using PetClinic.Domain.Primitives;

namespace PetClinic.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando la fecha y hora de la cita violan las reglas del negocio
/// (por ejemplo, fecha de fin anterior a la fecha de inicio).
/// </summary>
public sealed class InvalidAppointmentTimeException : DomainException
{
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }

    public InvalidAppointmentTimeException(DateTime startTime, DateTime endTime, string message)
        : base(message)
    {
        StartTime = startTime;
        EndTime = endTime;
    }
}
