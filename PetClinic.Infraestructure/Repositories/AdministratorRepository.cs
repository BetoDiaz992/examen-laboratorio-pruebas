#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetClinic.Domain.Entities;
using PetClinic.Domain.Interfaces;
using PetClinic.Infrastructure.Persistence;

namespace PetClinic.Infrastructure.Repositories;

/// <summary>
/// Implementación EF Core del contrato IAdministratorRepository.
/// </summary>
public sealed class AdministratorRepository : IAdministratorRepository
{
    private readonly PetClinicDbContext _context;

    public AdministratorRepository(PetClinicDbContext context)
    {
        _context = context;
    }

    public async Task<Administrator?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Administrators
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Administrator?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Administrators
            .FirstOrDefaultAsync(a => a.Email == email, cancellationToken);
    }

    public async Task AddAsync(Administrator admin, CancellationToken cancellationToken = default)
    {
        await _context.Administrators.AddAsync(admin, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(Administrator admin)
    {
        _context.Administrators.Update(admin);
        _context.SaveChanges();
    }
}
