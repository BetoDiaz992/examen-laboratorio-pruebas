#nullable enable

using System;
using PetClinic.Domain.Primitives;

namespace PetClinic.Domain.Entities;

/// <summary>
/// Entidad que representa al Administrador único del sistema.
/// </summary>
public sealed class Administrator : Entity
{
    /// <summary>
    /// Nombre del administrador (columna 'Name' en tabla Administrators).
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Hash de la contraseña de acceso.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Correo electrónico institucional.
    /// </summary>
    public string Email { get; private set; }

    // Constructor privado para EF Core
    private Administrator() : base(Guid.NewGuid())
    {
        Name = string.Empty;
        PasswordHash = string.Empty;
        Email = string.Empty;
    }

    public Administrator(Guid id, string name, string passwordHash, string email)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre del administrador no puede estar vacío.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("El hash de la contraseña no puede estar vacío.", nameof(passwordHash));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("El correo electrónico no puede estar vacío.", nameof(email));
        }

        Name = name;
        PasswordHash = passwordHash;
        Email = email;
    }

    /// <summary>
    /// Actualiza el correo electrónico del administrador.
    /// </summary>
    public void UpdateEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
        {
            throw new ArgumentException("El nuevo correo electrónico no puede estar vacío.", nameof(newEmail));
        }

        Email = newEmail;
    }

    /// <summary>
    /// Actualiza las credenciales de acceso del administrador.
    /// </summary>
    public void UpdateCredentials(string newName, string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("El nuevo nombre no puede estar vacío.", nameof(newName));
        }

        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new ArgumentException("El nuevo hash de contraseña no puede estar vacío.", nameof(newPasswordHash));
        }

        Name = newName;
        PasswordHash = newPasswordHash;
    }
}
