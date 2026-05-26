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
/// Implementación EF Core del contrato IPetRepository.
/// </summary>
public sealed class PetRepository : IPetRepository
{
    private readonly PetClinicDbContext _context;

    public PetRepository(PetClinicDbContext context)
    {
        _context = context;
    }

    public async Task<Pet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Pets
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Pet>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Pets
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Pet pet, CancellationToken cancellationToken = default)
    {
        await _context.Pets.AddAsync(pet, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(Pet pet)
    {
        _context.Pets.Update(pet);
        _context.SaveChanges();
    }

    public void Delete(Pet pet)
    {
        _context.Pets.Remove(pet);
        _context.SaveChanges();
    }
}
