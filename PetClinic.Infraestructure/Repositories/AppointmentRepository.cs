#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetClinic.Domain.Entities;
using PetClinic.Domain.Enums;
using PetClinic.Domain.Interfaces;
using PetClinic.Infrastructure.Persistence;

namespace PetClinic.Infrastructure.Repositories;

/// <summary>
/// Implementación EF Core del contrato IAppointmentRepository.
/// Alineada con schema: ScheduledTime, State, Reason (sin EndTime separado).
/// </summary>
public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly PetClinicDbContext _context;

    public AppointmentRepository(PetClinicDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .OrderBy(a => a.ScheduledTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByVeterinarianIdAsync(Guid veterinarianId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == veterinarianId)
            .OrderBy(a => a.ScheduledTime)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// REQ-CIT-02: Verifica si el veterinario ya tiene una cita activa (no CANCELLED) en la misma fecha/hora.
    /// Dado que la DB solo tiene ScheduledTime (sin EndTime separado), se considera solapamiento
    /// si hay una cita del mismo veterinario con exactamente la misma ScheduledTime.
    /// </summary>
    public async Task<bool> HasOverlappingAppointmentAsync(
        Guid veterinarianId, 
        DateTime startTime, 
        DateTime endTime, 
        Guid? excludeAppointmentId = null, 
        CancellationToken cancellationToken = default)
    {
        // La DB tiene índice único en (VeterinarianId, ScheduledTime) WHERE State != 'CANCELLED'
        // Verificamos si existe cita activa del veterinario en el mismo bloque
        return await _context.Appointments
            .AnyAsync(a => 
                a.VeterinarianId == veterinarianId &&
                a.State != AppointmentStatus.Cancelada &&
                (excludeAppointmentId == null || a.Id != excludeAppointmentId) &&
                a.ScheduledTime == startTime,
                cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await _context.Appointments.AddAsync(appointment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        _context.SaveChanges();
    }

    public void Delete(Appointment appointment)
    {
        _context.Appointments.Remove(appointment);
        _context.SaveChanges();
    }
}
