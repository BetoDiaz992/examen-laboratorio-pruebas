#nullable enable

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Moq;
using PetClinic.Domain.Entities;
using PetClinic.Domain.Enums;
using PetClinic.Domain.Events;
using PetClinic.Domain.Exceptions;
using PetClinic.Domain.Interfaces;

namespace PetClinic.Test;

/// <summary>
/// Pruebas unitarias rigurosas para validar las reglas de negocio de la entidad Appointment.
/// Actualizado para schema: ScheduledTime+Reason+State (no StartTime/EndTime/Status/Diagnosis).
/// </summary>
[TestClass]
public sealed class AppointmentTests
{
    private readonly Faker _faker = new();

    #region Pruebas de Entidad e Invariantes de Negocio (Appointment)

    [TestMethod]
    public void CreateAppointment_WithValidData_ShouldInstantiateSuccessfullyAndSetDefaultState()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid petId = Guid.NewGuid();
        Guid veterinarianId = Guid.NewGuid();
        DateTime scheduled = DateTime.UtcNow.AddDays(1);
        string reason = "Chequeo médico de rutina";

        // Act
        var appointment = new Appointment(id, petId, veterinarianId, scheduled, reason);

        // Assert
        appointment.Should().NotBeNull();
        appointment.Id.Should().Be(id);
        appointment.PetId.Should().Be(petId);
        appointment.VeterinarianId.Should().Be(veterinarianId);
        appointment.ScheduledTime.Should().Be(scheduled);
        appointment.Reason.Should().Be(reason);

        // REQ-CIT-03: Toda cita registrada exitosamente debe tener estado inicial "Programada"
        appointment.State.Should().Be(AppointmentStatus.Programada);

        // Eventos de Dominio
        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AppointmentScheduledEvent>();

        var domainEvent = (AppointmentScheduledEvent)appointment.DomainEvents.First();
        domainEvent.AppointmentId.Should().Be(id);
        domainEvent.PetId.Should().Be(petId);
        domainEvent.VeterinarianId.Should().Be(veterinarianId);
    }

    [TestMethod]
    public void CreateAppointment_WithEmptyReason_ShouldThrowArgumentException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid petId = Guid.NewGuid();
        Guid veterinarianId = Guid.NewGuid();
        DateTime scheduled = DateTime.UtcNow.AddDays(1);

        // Act
        Action action = () => new Appointment(id, petId, veterinarianId, scheduled, "");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*motivo*");
    }

    [TestMethod]
    public void Reschedule_WithValidTime_ShouldUpdateScheduledTimeAndRaiseEvent()
    {
        // Arrange
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1), "Vacunación");
        DateTime newScheduled = DateTime.UtcNow.AddDays(2);
        appointment.ClearDomainEvents();

        // Act
        appointment.Reschedule(newScheduled, "Vacunación reprogramada");

        // Assert
        appointment.ScheduledTime.Should().Be(newScheduled);
        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AppointmentRescheduledEvent>();
    }

    [TestMethod]
    public void Reschedule_WhenCancelled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1), "Consulta");
        appointment.Cancel();

        // Act
        Action action = () => appointment.Reschedule(DateTime.UtcNow.AddDays(2));

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*cancelada*");
    }

    [TestMethod]
    public void Cancel_ActiveAppointment_ShouldSetStateToCanceladaAndRaiseEvent()
    {
        // Arrange
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1), "Desparasitación");
        appointment.ClearDomainEvents();

        // Act
        appointment.Cancel();

        // Assert
        appointment.State.Should().Be(AppointmentStatus.Cancelada);
        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AppointmentCancelledEvent>();
    }

    [TestMethod]
    public void Complete_ActiveAppointment_ShouldSetStateToCompletadaAndUpdateReason()
    {
        // Arrange
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1), "Chequeo");
        string notes = _faker.Lorem.Sentence();

        // Act
        appointment.Complete(notes);

        // Assert
        appointment.State.Should().Be(AppointmentStatus.Completada);
        appointment.Reason.Should().Be(notes);
    }

    #endregion

    #region Pruebas de Mocking y Prevención de Superposición de Horarios (REQ-CIT-02)

    [TestMethod]
    public async Task Schedule_WhenSlotIsOccupied_ShouldReturnTrueForOverlapping()
    {
        // Arrange
        var mockRepo = new Mock<IAppointmentRepository>();
        Guid veterinarianId = Guid.NewGuid();
        DateTime targetTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

        // Escenario: El repositorio simula que SÍ hay solapamiento (retorna true)
        mockRepo.Setup(repo => repo.HasOverlappingAppointmentAsync(
                veterinarianId, targetTime, targetTime, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        bool isOverlapping = await mockRepo.Object.HasOverlappingAppointmentAsync(veterinarianId, targetTime, targetTime);

        // Assert
        isOverlapping.Should().BeTrue();
        mockRepo.Verify(repo => repo.HasOverlappingAppointmentAsync(
            veterinarianId, targetTime, targetTime, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Schedule_WhenSlotIsFree_ShouldReturnFalseForOverlapping()
    {
        // Arrange
        var mockRepo = new Mock<IAppointmentRepository>();
        Guid veterinarianId = Guid.NewGuid();
        DateTime targetTime = DateTime.UtcNow.Date.AddDays(1).AddHours(15);

        // Escenario: El repositorio simula que NO hay solapamiento (retorna false)
        mockRepo.Setup(repo => repo.HasOverlappingAppointmentAsync(
                veterinarianId, targetTime, targetTime, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        bool isOverlapping = await mockRepo.Object.HasOverlappingAppointmentAsync(veterinarianId, targetTime, targetTime);

        // Assert
        isOverlapping.Should().BeFalse();
        mockRepo.Verify(repo => repo.HasOverlappingAppointmentAsync(
            veterinarianId, targetTime, targetTime, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Schedule_WhenOverlapExists_ShouldReturnTrueForPartialOverlap()
    {
        // Arrange
        var mockRepo = new Mock<IAppointmentRepository>();
        Guid veterinarianId = Guid.NewGuid();
        DateTime targetTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14).AddMinutes(30);

        // Escenario: El repositorio simula que SÍ hay solapamiento parcial (retorna true)
        mockRepo.Setup(repo => repo.HasOverlappingAppointmentAsync(
                veterinarianId, targetTime, targetTime, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        bool isOverlapping = await mockRepo.Object.HasOverlappingAppointmentAsync(veterinarianId, targetTime, targetTime);

        // Assert
        isOverlapping.Should().BeTrue();
    }

    #endregion
}
