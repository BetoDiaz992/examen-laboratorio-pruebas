#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetClinic.Domain.Repositories;

namespace PetClinic.Web.Controllers;

/// <summary>
/// Controlador encargado del agendamiento y visualización de Citas Médicas.
/// Cumple con las reglas estrictas de:
/// 1. Requerir autenticación ([Authorize]).
/// 2. No usar DbContext directo (inyecta IAppointmentRepository del Dominio).
/// 3. Ofrecer visualización dual: Lista y Calendario Semanal.
/// </summary>
[Authorize]
public class AppointmentsController : Controller
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentsController(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
    }

    /// <summary>
    /// Acción principal. Soporta el paso de un parámetro 'viewMode' para alternar vistas de forma interactiva.
    /// </summary>
    /// <param name="viewMode">"list" (Listado tabular) o "calendar" (Cuadrícula semanal).</param>
    [HttpGet]
    public async Task<IActionResult> Index(string viewMode = "list")
    {
        ViewData["ActiveTab"] = "Citas";
        ViewData["ViewMode"] = viewMode; // Controla la renderización en el archivo .cshtml

        // Recuperar las citas mediante el contrato puro de repositorio del Dominio
        var appointments = await _appointmentRepository.GetAllAsync();

        // Enviar el listado a la vista correspondiente
        return View(appointments);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["ActiveTab"] = "Citas";
        // Retorna el formulario de creación estilizado
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAppointmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Validar cruces de horarios usando el contrato del repositorio del dominio
            var existingAppointment = await _appointmentRepository.GetByVetAndSlotAsync(model.VeterinarianId, model.ScheduledTime);
            if (existingAppointment != null)
            {
                ModelState.AddModelError("ScheduledTime", "El veterinario seleccionado ya cuenta con una cita programada en ese bloque.");
                return View(model);
            }

            // Invocar el constructor de fábrica de la Entidad de Dominio
            var appointment = PetClinic.Domain.Entities.Appointment.Create(
                model.PetId,
                model.VeterinarianId,
                model.ScheduledTime,
                model.Reason
            );

            // Persistir mediante el repositorio
            await _appointmentRepository.AddAsync(appointment);

            // Redirigir al listado principal de citas
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al programar la cita: {ex.Message}");
            return View(model);
        }
    }
}

/// <summary>
/// Modelo de vista para la captura de datos al agendar una cita.
/// </summary>
public class CreateAppointmentViewModel
{
    public Guid PetId { get; set; }
    public Guid VeterinarianId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public string Reason { get; set; } = string.Empty;
}
