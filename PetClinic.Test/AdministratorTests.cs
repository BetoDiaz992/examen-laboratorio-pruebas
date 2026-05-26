#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Moq;
using PetClinic.Domain.Entities;
using PetClinic.Domain.Interfaces;

namespace PetClinic.Test;

/// <summary>
/// Pruebas unitarias para validar las reglas del Administrador único y su persistencia simulada.
/// Actualizado para coincidir con schema: Name (no Username), GetByEmailAsync (no GetByUsernameAsync).
/// </summary>
[TestClass]
public sealed class AdministratorTests
{
    private readonly Faker _faker = new();

    [TestMethod]
    public void CreateAdministrator_WithValidData_ShouldInstantiateSuccessfully()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string name = _faker.Name.FullName();
        string passwordHash = _faker.Internet.Password();
        string email = _faker.Internet.Email();

        // Act
        var admin = new Administrator(id, name, passwordHash, email);

        // Assert
        admin.Should().NotBeNull();
        admin.Id.Should().Be(id);
        admin.Name.Should().Be(name);
        admin.PasswordHash.Should().Be(passwordHash);
        admin.Email.Should().Be(email);
    }

    [TestMethod]
    [DataRow("", "passwordHash", "admin@petclinic.com")]
    [DataRow("Admin Principal", "", "admin@petclinic.com")]
    [DataRow("Admin Principal", "passwordHash", "")]
    public void CreateAdministrator_WithInvalidData_ShouldThrowArgumentException(string name, string passwordHash, string email)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Action action = () => new Administrator(id, name, passwordHash, email);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void UpdateEmail_WithValidEmail_ShouldUpdateSuccessfully()
    {
        // Arrange
        var admin = new Administrator(Guid.NewGuid(), _faker.Name.FullName(), _faker.Internet.Password(), _faker.Internet.Email());
        string newEmail = _faker.Internet.Email();

        // Act
        admin.UpdateEmail(newEmail);

        // Assert
        admin.Email.Should().Be(newEmail);
    }

    [TestMethod]
    public async Task GetByEmail_WhenAdminExists_ShouldReturnAdminRecord()
    {
        // Arrange
        var mockRepo = new Mock<IAdministratorRepository>();
        string targetEmail = "admin@petclinic.com";
        string targetPasswordHash = "ClinicAdminSecurePass10!";
        var expectedAdmin = new Administrator(Guid.NewGuid(), "Administrador Clínico Principal", targetPasswordHash, targetEmail);

        mockRepo.Setup(repo => repo.GetByEmailAsync(targetEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAdmin);

        // Act
        var result = await mockRepo.Object.GetByEmailAsync(targetEmail);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(targetEmail);
        result.PasswordHash.Should().Be(targetPasswordHash);
        mockRepo.Verify(repo => repo.GetByEmailAsync(targetEmail, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetByEmail_WhenAdminDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var mockRepo = new Mock<IAdministratorRepository>();
        string nonExistentEmail = _faker.Internet.Email();

        mockRepo.Setup(repo => repo.GetByEmailAsync(nonExistentEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Administrator?)null);

        // Act
        var result = await mockRepo.Object.GetByEmailAsync(nonExistentEmail);

        // Assert
        result.Should().BeNull();
        mockRepo.Verify(repo => repo.GetByEmailAsync(nonExistentEmail, It.IsAny<CancellationToken>()), Times.Once);
    }
}
