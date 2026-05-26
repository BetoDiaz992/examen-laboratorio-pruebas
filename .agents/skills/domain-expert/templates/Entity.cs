#nullable enable

using System;
using System.Collections.Generic;

namespace PetClinic.Domain.Primitives;

/// <summary>
/// Clase base para todas las entidades en el Domain-Driven Design (DDD).
/// Proporciona un identificador único y encapsula la colección de Eventos de Dominio.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Identificador único de la entidad.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Colección de lectura de los Eventos de Dominio acumulados en esta entidad.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador de la entidad no puede estar vacío.", nameof(id));
        }

        Id = id;
    }

    /// <summary>
    /// Añade un nuevo evento de dominio a la colección interna.
    /// </summary>
    /// <param name="domainEvent">El evento a registrar.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Limpia todos los eventos de dominio acumulados tras su despacho exitoso.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    #region Implementación de Igualdad

    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Entity)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }

    #endregion
}
