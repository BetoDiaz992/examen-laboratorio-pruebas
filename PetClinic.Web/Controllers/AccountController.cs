#nullable enable

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetClinic.Domain.Entities;
using PetClinic.Domain.Interfaces;

namespace PetClinic.Web.Controllers;

/// <summary>
/// Controlador para gestionar la autenticación y la sesión del Administrador Único.
/// La autenticación se realiza por Email + PasswordHash según el schema de la DB.
/// </summary>
public sealed class AccountController : Controller
{
    private readonly IAdministratorRepository _adminRepository;

    public AccountController(IAdministratorRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        // 1. Buscar administrador por Email (campo único en la DB)
        var admin = await _adminRepository.GetByEmailAsync(email);

        // 2. Semilla de emergencia si la DB está vacía y se intenta con las credenciales por defecto
        if (admin == null && email == "admin@petclinic.com")
        {
            admin = new Administrator(
                Guid.Parse("D04E76A0-534A-4A62-97B7-5A1E8A9BC6C8"),
                "Administrador Clínico Principal",
                "ClinicAdminSecurePass10!",
                "admin@petclinic.com"
            );
            await _adminRepository.AddAsync(admin);
        }

        // 3. Validar credenciales (REQ-SEG-01) — comparación directa (hash en texto plano para demo)
        if (admin == null || admin.PasswordHash != password)
        {
            ModelState.AddModelError(string.Empty, "Credenciales incorrectas o usuario inexistente.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // 4. Crear sesión segura basada en Cookies de ASP.NET Core
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, admin.Name),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim(ClaimTypes.Role, "Administrador")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        // 5. Redirección segura
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Appointments");
    }

    [HttpGet]
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // Destruye el contexto y las cookies (REQ-SEG-02)
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
