#nullable enable

using System;

namespace PetClinic.Domain.Primitives;

/// <summary>
/// Interfaz base para todos los Eventos de Dominio en el sistema.
/// Sirve como marcador y garantiza que cada evento capture el instante exacto en que ocurrió.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Fecha y hora exacta (preferiblemente UTC) en que ocurrió el evento de negocio.
    /// </summary>
    DateTime OccurredOn { get; }
}
