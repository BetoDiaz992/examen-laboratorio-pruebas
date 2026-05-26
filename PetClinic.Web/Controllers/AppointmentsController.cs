#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetClinic.Domain.Entities;
using PetClinic.Domain.Exceptions;
using PetClinic.Domain.Interfaces;

namespace PetClinic.Web.Controllers;

/// <summary>
/// Controlador principal para gestionar la agenda y las citas médicas del sistema.
/// </summary>
[Authorize]
public sealed class AppointmentsController : Controller
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPetRepository _petRepository;
    private readonly IVeterinarianRepository _veterinarianRepository;

    public AppointmentsController(
        IAppointmentRepository appointmentRepository,
        IPetRepository petRepository,
        IVeterinarianRepository veterinarianRepository)
    {
        _appointmentRepository = appointmentRepository;
        _petRepository = petRepository;
        _veterinarianRepository = veterinarianRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // 1. Sembrar datos por defecto si la base de datos está vacía para facilitar la demo
        await SeedDemoDataIfNeeded();

        // 2. Obtener catálogos relacionales y citas
        var appointments = await _appointmentRepository.GetAllAsync();
        var pets = await _petRepository.GetAllAsync();
        var veterinarians = await _veterinarianRepository.GetAllAsync();

        ViewBag.Pets = pets.ToList();
        ViewBag.Veterinarians = veterinarians.ToList();

        return View(appointments.ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid petId, Guid veterinarianId, DateTime scheduledTime, string reason)
    {
        try
        {
            // REQ-CIT-01: Validar existencia de dependencias
            var pet = await _petRepository.GetByIdAsync(petId);
            var veterinarian = await _veterinarianRepository.GetByIdAsync(veterinarianId);

            if (pet == null || veterinarian == null)
            {
                TempData["ErrorMessage"] = "La mascota o el veterinario seleccionados no son válidos.";
                return RedirectToAction(nameof(Index));
            }

            // REQ-CIT-02: Prevención de superposición de horarios
            bool hasOverlap = await _appointmentRepository.HasOverlappingAppointmentAsync(veterinarianId, scheduledTime, scheduledTime);
            if (hasOverlap)
            {
                TempData["ErrorMessage"] = $"Conflicto de Horario: El veterinario {veterinarian.Name} ya posee una cita activa en el bloque horario solicitado.";
                return RedirectToAction(nameof(Index));
            }

            // Crear y persistir la cita (invariante: estado inicial "Programada" por defecto REQ-CIT-03)
            var appointment = new Appointment(Guid.NewGuid(), petId, veterinarianId, scheduledTime, reason);
            await _appointmentRepository.AddAsync(appointment);

            TempData["SuccessMessage"] = "Cita programada con éxito.";
        }
        catch (InvalidAppointmentTimeException ex)
        {
            TempData["ErrorMessage"] = $"Horario Inválido: {ex.Message}";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al registrar la cita: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Reschedule(Guid id, DateTime scheduledTime, string? reason = null)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Cita no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar solapamiento excluyendo la cita actual
            bool hasOverlap = await _appointmentRepository.HasOverlappingAppointmentAsync(appointment.VeterinarianId, scheduledTime, scheduledTime, appointment.Id);
            if (hasOverlap)
            {
                TempData["ErrorMessage"] = "Conflicto de Horario: El veterinario ya posee una cita programada en ese bloque de tiempo.";
                return RedirectToAction(nameof(Index));
            }

            appointment.Reschedule(scheduledTime, reason);
            _appointmentRepository.Update(appointment);

            TempData["SuccessMessage"] = "Cita reprogramada correctamente.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al reprogramar: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Cita no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            appointment.Cancel();
            _appointmentRepository.Update(appointment);

            TempData["SuccessMessage"] = "Cita cancelada correctamente.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al cancelar: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Complete(Guid id, string notes)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Cita no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            appointment.Complete(notes);
            _appointmentRepository.Update(appointment);

            TempData["SuccessMessage"] = "Cita completada correctamente.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al completar: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // Los datos semilla de la DB (schema.sql) ya incluyen mascotas y veterinarios por defecto.
    // Este método solo actúa como respaldo si la tabla está completamente vacía.
    private async Task SeedDemoDataIfNeeded()
    {
        var existingPets = await _petRepository.GetAllAsync();
        if (!existingPets.Any())
        {
            // OwnerId = Guid aleatorio para propietario de demo
            var ownerId1 = Guid.NewGuid();
            var ownerId2 = Guid.NewGuid();
            await _petRepository.AddAsync(new Pet(Guid.NewGuid(), ownerId1, "Bruno", "Perro", "Golden Retriever", DateTime.UtcNow.AddYears(-3)));
            await _petRepository.AddAsync(new Pet(Guid.NewGuid(), ownerId1, "Luna", "Gato", "Siamés", DateTime.UtcNow.AddYears(-1)));
            await _petRepository.AddAsync(new Pet(Guid.NewGuid(), ownerId2, "Max", "Perro", "Bulldog", DateTime.UtcNow.AddYears(-2)));
            await _petRepository.AddAsync(new Pet(Guid.NewGuid(), ownerId2, "Coco", "Perro", "Poodle", DateTime.UtcNow.AddYears(-4)));
        }

        var existingVets = await _veterinarianRepository.GetAllAsync();
        if (!existingVets.Any())
        {
            await _veterinarianRepository.AddAsync(new Veterinarian(Guid.NewGuid(), "Dr. David Miller", "Consulta General", "LIC-00001", "david.miller@vetcorp.com"));
            await _veterinarianRepository.AddAsync(new Veterinarian(Guid.NewGuid(), "Dra. Sarah Chen", "Cirugía Veterinaria", "LIC-00002", "sarah.chen@vetcorp.com"));
            await _veterinarianRepository.AddAsync(new Veterinarian(Guid.NewGuid(), "Dr. Roberto Sanz", "Odontología", "LIC-00003", "roberto.sanz@vetcorp.com"));
        }
    }
}
