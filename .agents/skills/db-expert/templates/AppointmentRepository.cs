#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetClinic.Domain.Entities;
using PetClinic.Domain.Repositories;

namespace PetClinic.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación relacional del repositorio de Citas (IAppointmentRepository) 
/// utilizando Entity Framework Core 10.0 contra Microsoft SQL Server.
/// </summary>
public class AppointmentRepository : IAppointmentRepository
{
    private readonly PetClinicDbContext _context;

    public AppointmentRepository(PetClinicDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Consulta una cita médica por su identificador único.
    /// </summary>
    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        return await _context.Appointments
            .SingleOrDefaultAsync(a => a.Id == id);
    }

    /// <summary>
    /// Consulta si existe alguna cita registrada para un veterinario y bloque de fecha específico (evitar cruces).
    /// </summary>
    public async Task<Appointment?> GetByVetAndSlotAsync(Guid vetId, DateTime scheduledTime)
    {
        return await _context.Appointments
            .SingleOrDefaultAsync(a => a.VeterinarianId == vetId && a.ScheduledTime == scheduledTime);
    }

    /// <summary>
    /// Inserta una nueva cita médica en el contexto para su posterior persistencia física.
    /// </summary>
    public async Task AddAsync(Appointment appointment)
    {
        await _context.Appointments.AddAsync(appointment);
    }

    /// <summary>
    /// Actualiza el estado de una cita en el contexto. El ChangeTracker rastrea los cambios de forma automática.
    /// </summary>
    public Task UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        return Task.CompletedTask;
    }
}
