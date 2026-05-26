#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PetClinic.Domain.Entities;

namespace PetClinic.Domain.Interfaces;

/// <summary>
/// Contrato de persistencia para la gestión de Veterinarios (Médicos).
/// </summary>
public interface IVeterinarianRepository
{
    /// <summary>
    /// Obtiene un veterinario por su identificador único.
    /// </summary>
    Task<Veterinarian?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el catálogo completo de veterinarios registrados.
    /// </summary>
    Task<IEnumerable<Veterinarian>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra un nuevo veterinario en la base de datos.
    /// </summary>
    Task AddAsync(Veterinarian veterinarian, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza los datos de un veterinario preexistente.
    /// </summary>
    void Update(Veterinarian veterinarian);

    /// <summary>
    /// Elimina físicamente un veterinario.
    /// </summary>
    void Delete(Veterinarian veterinarian);
}
