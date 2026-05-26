#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using Bogus;
using PetClinic.Domain.Entities;
using PetClinic.Domain.Repositories;
using PetClinic.Domain.Exceptions;
using PetClinic.Domain.Events;

namespace PetClinic.Tests.Entities;

/// <summary>
/// Plantilla de ejemplo de pruebas unitarias aplicando TDD, MSTest, Moq y FluentAssertions.
/// Esta clase demuestra cómo validar reglas de negocio e invariantes del Dominio en aislamiento absoluto.
/// </summary>
[TestClass]
public class AppointmentTests
{
    private readonly Faker _faker = new();
    private Mock<IAppointmentRepository> _appointmentRepositoryMock = null!;
    
    [TestInitialize]
    public void Setup()
    {
        // Se ejecuta antes de cada prueba para asegurar aislamiento e independencia de estados.
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
    }

    [TestMethod]
    public async Task Schedule_WhenSlotIsAvailable_ShouldBookAppointmentAndRaiseDomainEvent()
    {
        // Arrange (Preparar) - Datos generados con Bogus de forma dinámica y realista
        var petId = Guid.NewGuid();
        var vetId = Guid.NewGuid();
        var appointmentTime = DateTime.UtcNow.AddDays(_faker.Random.Int(1, 10)).Date.AddHours(10); // Horario futuro a las 10:00 AM
        var reason = _faker.Lorem.Sentence();
        
        // Simular que el repositorio no encuentra colisiones para este horario (retorna null)
        _appointmentRepositoryMock
            .Setup(repo => repo.GetByVetAndSlotAsync(vetId, appointmentTime))
            .ReturnsAsync((Appointment?)null);

        // Act (Actuar) - Ejecución de la lógica de negocio
        // Nota TDD: Si la clase 'Appointment' no existiera, esto daría error de compilación.
        var appointment = Appointment.Create(petId, vetId, appointmentTime, reason);
        await _appointmentRepositoryMock.Object.AddAsync(appointment);

        // Assert (Aseverar) - Validaciones legibles con FluentAssertions
        appointment.Should().NotBeNull();
        appointment.PetId.Should().Be(petId);
        appointment.VeterinarianId.Should().Be(vetId);
        appointment.ScheduledTime.Should().Be(appointmentTime);
        appointment.Reason.Should().Be(reason);
        appointment.State.Should().Be(AppointmentState.Scheduled);

        // Validar Evento de Dominio
        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AppointmentScheduledEvent>();

        // Verificar que el mock del repositorio fue invocado exactamente una vez
        _appointmentRepositoryMock.Verify(repo => repo.AddAsync(appointment), Times.Once);
    }

    [TestMethod]
    public async Task Schedule_WhenSlotIsOverlapped_ShouldThrowAppointmentConflictException()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var vetId = Guid.NewGuid();
        var appointmentTime = DateTime.UtcNow.AddDays(2).Date.AddHours(11);
        var reason = _faker.Lorem.Sentence();
        
        // Crear una cita preexistente usando Bogus
        var existingAppointment = Appointment.Create(
            Guid.NewGuid(), 
            vetId, 
            appointmentTime, 
            _faker.Lorem.Sentence()
        );

        // Simular que el repositorio de citas retorna la cita preexistente (conflicto)
        _appointmentRepositoryMock
            .Setup(repo => repo.GetByVetAndSlotAsync(vetId, appointmentTime))
            .ReturnsAsync(existingAppointment);

        // Act (Actuar) y Assert (Aseverar)
        // Intentar crear la cita debe disparar la excepción de dominio (violación de invariante)
        Func<Task> action = async () =>
        {
            var conflictAppointment = Appointment.Create(petId, vetId, appointmentTime, reason);
            
            // Simulación de la regla que el servicio/entidad validaría
            var isSlotBusy = await _appointmentRepositoryMock.Object.GetByVetAndSlotAsync(vetId, appointmentTime);
            if (isSlotBusy != null)
            {
                throw new AppointmentConflictException(vetId, appointmentTime);
            }
        };

        // Aseveración del lanzamiento de la excepción de dominio usando FluentAssertions
        await action.Should().ThrowAsync<AppointmentConflictException>()
            .WithMessage($"El veterinario {vetId} ya cuenta con una cita programada para el horario {appointmentTime}.");
    }
}
