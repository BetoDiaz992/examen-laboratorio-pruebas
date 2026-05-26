#nullable enable

namespace PetClinic.Domain.Enums;

/// <summary>
/// Representa el estado de una cita en la clínica veterinaria.
/// Los valores string coinciden exactamente con la columna State (NVARCHAR) de la tabla Appointments.
/// </summary>
public enum AppointmentStatus
{
    /// <summary>
    /// Estado predeterminado e inicial para toda cita creada con éxito. DB: 'SCHEDULED'
    /// </summary>
    Programada = 1,

    /// <summary>
    /// La cita ha concluido satisfactoriamente. DB: 'COMPLETED'
    /// </summary>
    Completada = 2,

    /// <summary>
    /// La cita fue cancelada por el administrador. DB: 'CANCELLED'
    /// </summary>
    Cancelada = 3
}
