#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using PetClinic.Domain.Entities;

namespace PetClinic.Domain.Interfaces;

/// <summary>
/// Contrato de persistencia para la gestión del Administrador único.
/// </summary>
public interface IAdministratorRepository
{
    /// <summary>
    /// Obtiene al administrador por su identificador único.
    /// </summary>
    Task<Administrator?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene al administrador por su correo electrónico (campo único en la DB para la barrera de Login).
    /// </summary>
    Task<Administrator?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra el administrador en el sistema.
    /// </summary>
    Task AddAsync(Administrator admin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Modifica los datos del administrador.
    /// </summary>
    void Update(Administrator admin);
}
