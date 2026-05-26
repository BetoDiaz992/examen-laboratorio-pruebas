#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PetClinic.Domain.Entities;

namespace PetClinic.Domain.Interfaces;

/// <summary>
/// Contrato de persistencia para la gestión de Citas de la agenda de la clínica veterinaria.
/// </summary>
public interface IAppointmentRepository
{
    /// <summary>
    /// Obtiene una cita por su identificador único.
    /// </summary>
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las citas de la clínica veterinaria.
    /// </summary>
    Task<IEnumerable<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las citas asignadas a un veterinario específico.
    /// </summary>
    Task<IEnumerable<Appointment>> GetByVeterinarianIdAsync(Guid veterinarianId, CancellationToken cancellationToken = default);

    /// <summary>
    /// REQ-CIT-02: Verifica si el veterinario seleccionado ya posee una cita activa cuyo bloque de tiempo (fecha y hora)
    /// se cruza total o parcialmente con la nueva solicitud.
    /// </summary>
    /// <param name="veterinarianId">El identificador único del veterinario.</param>
    /// <param name="startTime">Fecha y hora de inicio del bloque horario.</param>
    /// <param name="endTime">Fecha y hora de finalización del bloque horario.</param>
    /// <param name="excludeAppointmentId">Permite excluir una cita específica (útil al reprogramar).</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>True si existe solapamiento de horarios; False en caso contrario.</returns>
    Task<bool> HasOverlappingAppointmentAsync(
        Guid veterinarianId, 
        DateTime startTime, 
        DateTime endTime, 
        Guid? excludeAppointmentId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra una nueva cita en la base de datos.
    /// </summary>
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una cita existente.
    /// </summary>
    void Update(Appointment appointment);

    /// <summary>
    /// Elimina físicamente una cita.
    /// </summary>
    void Delete(Appointment appointment);
}
