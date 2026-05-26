#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetClinic.Domain.Entities;
using PetClinic.Domain.Interfaces;
using PetClinic.Infrastructure.Persistence;

namespace PetClinic.Infrastructure.Repositories;

/// <summary>
/// Implementación EF Core del contrato IVeterinarianRepository.
/// </summary>
public sealed class VeterinarianRepository : IVeterinarianRepository
{
    private readonly PetClinicDbContext _context;

    public VeterinarianRepository(PetClinicDbContext context)
    {
        _context = context;
    }

    public async Task<Veterinarian?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Veterinarians
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Veterinarian>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Veterinarians
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Veterinarian veterinarian, CancellationToken cancellationToken = default)
    {
        await _context.Veterinarians.AddAsync(veterinarian, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(Veterinarian veterinarian)
    {
        _context.Veterinarians.Update(veterinarian);
        _context.SaveChanges();
    }

    public void Delete(Veterinarian veterinarian)
    {
        _context.Veterinarians.Remove(veterinarian);
        _context.SaveChanges();
    }
}
