#nullable enable

using System;
using PetClinic.Domain.Primitives;

namespace PetClinic.Domain.Entities;

/// <summary>
/// Entidad que representa a una Mascota (Paciente).
/// Alineada con tabla Pets: Id, OwnerId, Name, Species, Breed, BirthDate
/// </summary>
public sealed class Pet : Entity
{
    /// <summary>
    /// Identificador del propietario (relación conceptual, columna OwnerId).
    /// </summary>
    public Guid OwnerId { get; private set; }

    /// <summary>
    /// Nombre de la mascota.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Especie de la mascota (ej. Perro, Gato, Ave).
    /// </summary>
    public string Species { get; private set; }

    /// <summary>
    /// Raza de la mascota. Puede ser nulo si es mestizo o se desconoce.
    /// </summary>
    public string? Breed { get; private set; }

    /// <summary>
    /// Fecha de nacimiento de la mascota.
    /// </summary>
    public DateTime BirthDate { get; private set; }

    // Constructor privado para EF Core
    private Pet() : base(Guid.NewGuid())
    {
        Name = string.Empty;
        Species = string.Empty;
    }

    public Pet(Guid id, Guid ownerId, string name, string species, string? breed, DateTime birthDate)
        : base(id)
    {
        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("El identificador del propietario no puede estar vacío.", nameof(ownerId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre de la mascota no puede estar vacío.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(species))
        {
            throw new ArgumentException("La especie no puede estar vacía.", nameof(species));
        }

        if (birthDate > DateTime.UtcNow)
        {
            throw new ArgumentException("La fecha de nacimiento no puede ser en el futuro.", nameof(birthDate));
        }

        OwnerId = ownerId;
        Name = name;
        Species = species;
        Breed = breed;
        BirthDate = birthDate;
    }

    /// <summary>
    /// Actualiza la información básica de la mascota.
    /// </summary>
    public void UpdateInfo(string name, string species, string? breed, DateTime birthDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre de la mascota no puede estar vacío.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(species))
        {
            throw new ArgumentException("La especie no puede estar vacía.", nameof(species));
        }

        if (birthDate > DateTime.UtcNow)
        {
            throw new ArgumentException("La fecha de nacimiento no puede ser en el futuro.", nameof(birthDate));
        }

        Name = name;
        Species = species;
        Breed = breed;
        BirthDate = birthDate;
    }
}
