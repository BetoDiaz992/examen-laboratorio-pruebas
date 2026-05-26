#nullable enable

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PetClinic.Web.Controllers;

/// <summary>
/// Controlador encargado del inicio de sesión (Login) y salida (Logout) del Administrador Único.
/// Marcado con [AllowAnonymous] para permitir el acceso de usuarios no autenticados a la pantalla de login.
/// </summary>
[AllowAnonymous]
public class AccountController : Controller
{
    [HttpGet]
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Regla Estricta: Administrador Único (Seguridad Simplificada y Robusta)
        // No creamos sistemas RBAC complejos innecesarios.
        const string AdminEmail = "admin@petclinic.com";
        const string AdminPassword = "ClinicAdminSecurePass10!"; // Contraseña de prueba fuerte

        if (model.Email.Equals(AdminEmail, StringComparison.OrdinalIgnoreCase) && model.Password == AdminPassword)
        {
            // Crear los Reclamos (Claims) de seguridad para el Administrador Único
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Administrador Clínico"),
                new Claim(ClaimTypes.Email, AdminEmail),
                new Claim(ClaimTypes.Role, "Administrator")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2) // Sesión válida por 2 horas
            };

            // Iniciar sesión emitiendo la Cookie de Seguridad de ASP.NET Core
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // Si las credenciales fallan
        ModelState.AddModelError(string.Empty, "Credenciales de administrador inválidas.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize] // Salir requiere estar logueado obligatoriamente
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}

/// <summary>
/// Modelo de vista para la captura de credenciales en el Login.
/// </summary>
public class LoginViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}
