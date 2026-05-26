#nullable enable

using System;
using PetClinic.Domain.Primitives;

namespace PetClinic.Domain.Entities;

/// <summary>
/// Entidad que representa a un Veterinario (Médico).
/// Alineada con tabla Veterinarians: Id, Name, Specialty, MedicalLicense, Email
/// </summary>
public sealed class Veterinarian : Entity
{
    /// <summary>
    /// Nombre completo del veterinario (ej. Dr. Pérez, Dra. Laura).
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Especialidad médica del veterinario.
    /// </summary>
    public string Specialty { get; private set; }

    /// <summary>
    /// Número de licencia médica única (columna MedicalLicense).
    /// </summary>
    public string MedicalLicense { get; private set; }

    /// <summary>
    /// Correo electrónico del veterinario.
    /// </summary>
    public string Email { get; private set; }

    // Constructor privado para EF Core
    private Veterinarian() : base(Guid.NewGuid())
    {
        Name = string.Empty;
        Specialty = string.Empty;
        MedicalLicense = string.Empty;
        Email = string.Empty;
    }

    public Veterinarian(Guid id, string name, string specialty, string medicalLicense, string email)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre del veterinario no puede estar vacío.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(specialty))
        {
            throw new ArgumentException("La especialidad del veterinario no puede estar vacía.", nameof(specialty));
        }

        if (string.IsNullOrWhiteSpace(medicalLicense))
        {
            throw new ArgumentException("La licencia médica no puede estar vacía.", nameof(medicalLicense));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("El correo electrónico del veterinario no puede estar vacío.", nameof(email));
        }

        Name = name;
        Specialty = specialty;
        MedicalLicense = medicalLicense;
        Email = email;
    }

    /// <summary>
    /// Actualiza la información del veterinario.
    /// </summary>
    public void UpdateInfo(string name, string specialty, string medicalLicense, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre del veterinario no puede estar vacío.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(specialty))
        {
            throw new ArgumentException("La especialidad del veterinario no puede estar vacía.", nameof(specialty));
        }

        if (string.IsNullOrWhiteSpace(medicalLicense))
        {
            throw new ArgumentException("La licencia médica no puede estar vacía.", nameof(medicalLicense));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("El correo electrónico del veterinario no puede estar vacío.", nameof(email));
        }

        Name = name;
        Specialty = specialty;
        MedicalLicense = medicalLicense;
        Email = email;
    }
}
