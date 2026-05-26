#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PetClinic.Domain.Entities;

namespace PetClinic.Domain.Interfaces;

/// <summary>
/// Contrato de persistencia para la gestión de Mascotas (Pacientes).
/// </summary>
public interface IPetRepository
{
    /// <summary>
    /// Obtiene una mascota por su identificador único.
    /// </summary>
    Task<Pet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el catálogo completo de mascotas registradas.
    /// </summary>
    Task<IEnumerable<Pet>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra una nueva mascota en la base de datos.
    /// </summary>
    Task AddAsync(Pet pet, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza los datos de una mascota preexistente.
    /// </summary>
    void Update(Pet pet);

    /// <summary>
    /// Elimina físicamente una mascota.
    /// </summary>
    void Delete(Pet pet);
}
