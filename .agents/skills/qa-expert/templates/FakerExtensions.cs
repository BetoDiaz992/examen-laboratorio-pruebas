#nullable enable

using System;
using Bogus;
using PetClinic.Domain.Entities;

namespace PetClinic.Tests.Utilities;

/// <summary>
/// Clase de utilidad para centralizar la generación de datos ficticios realistas usando Bogus.
/// Proporciona constructores de prueba (Test Builders) reutilizables para mantener los archivos de prueba limpios y mantenibles.
/// </summary>
public static class FakerExtensions
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Genera una instancia ficticia de la entidad Pet (Mascota) poblada con Bogus.
    /// </summary>
    public static Pet CreateFakePet()
    {
        var petFaker = new Faker<Pet>()
            .CustomInstantiator(f => Pet.Create(
                id: Guid.NewGuid(),
                ownerId: Guid.NewGuid(),
                name: f.Name.FirstName(),
                species: f.PickRandom(new[] { "Perro", "Gato", "Loro", "Conejo" }),
                breed: f.Commerce.ProductMaterial(),
                birthDate: f.Date.Past(5)
            ));

        return petFaker.Generate();
    }

    /// <summary>
    /// Genera una instancia ficticia de la entidad Veterinarian (Veterinario) poblada con Bogus.
    /// </summary>
    public static Veterinarian CreateFakeVeterinarian()
    {
        var vetFaker = new Faker<Veterinarian>()
            .CustomInstantiator(f => Veterinarian.Create(
                id: Guid.NewGuid(),
                name: f.Name.FullName(),
                specialty: f.PickRandom(new[] { "Cirugía", "Consulta General", "Dermatología", "Odontología" }),
                medicalLicense: $"LIC-{f.Random.Number(10000, 99999)}",
                email: f.Internet.Email()
            ));

        return vetFaker.Generate();
    }

    /// <summary>
    /// Genera una instancia ficticia de la entidad Appointment (Cita) poblada con Bogus.
    /// </summary>
    /// <param name="petId">ID opcional de la mascota asociada.</param>
    /// <param name="vetId">ID opcional del veterinario asociado.</param>
    public static Appointment CreateFakeAppointment(Guid? petId = null, Guid? vetId = null)
    {
        var pId = petId ?? Guid.NewGuid();
        var vId = vetId ?? Guid.NewGuid();
        
        var appFaker = new Faker<Appointment>()
            .CustomInstantiator(f => Appointment.Create(
                petId: pId,
                veterinarianId: vId,
                scheduledTime: f.Date.Future(1).Date.AddHours(f.Random.Int(8, 17)), // De 8:00 AM a 5:00 PM
                reason: f.Lorem.Sentence()
            ));

        return appFaker.Generate();
    }
}
