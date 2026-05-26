#nullable enable

using System;

namespace PetClinic.Domain.Primitives;

/// <summary>
/// Clase base para todas las excepciones del Dominio de Negocio.
/// Las excepciones que heredan de esta clase representan violaciones explícitas de reglas e invariantes del negocio,
/// diferenciándose de los errores técnicos o de infraestructura de la aplicación.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) 
        : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
