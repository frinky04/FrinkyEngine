using FrinkyEngine.Core.Scene;

namespace FrinkyEngine.Core.ECS;


/// <summary>
/// A stable reference to an <see cref="Entity"/> by its <see cref="Guid"/>.
/// Survives renames, serialization round-trips, and play-mode snapshots.
/// </summary>
public readonly struct EntityReference : IEquatable<EntityReference>
{
    /// <summary>
    /// An empty reference that does not point to any entity.
    /// </summary>
    public static readonly EntityReference None = default;

    /// <summary>
    /// The GUID of the referenced entity.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Whether this reference points to a valid (non-empty) entity ID.
    /// Does not guarantee the entity still exists in the scene.
    /// </summary>
    public bool IsValid => Id != Guid.Empty;

    /// <summary>
    /// Creates a reference to an entity by its GUID.
    /// </summary>
    public EntityReference(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Creates a reference to an existing entity.
    /// </summary>
    public EntityReference(Entity entity)
    {
        Id = entity.Id;
    }

    /// <summary>
    /// Resolves this reference against a scene, returning the entity or null if not found.
    /// </summary>
    public Entity? Resolve(Scene.Scene scene)
    {
        if (!IsValid) return null;
        return scene.FindEntityById(Id);
    }

    /// <summary>
    /// Resolves this reference using the scene that the context entity belongs to.
    /// </summary>
    public Entity? Resolve(Entity context)
    {
        if (!IsValid || context.Scene == null) return null;
        return context.Scene.FindEntityById(Id);
    }

    /// <summary>
    /// Allows assigning an entity directly to an <see cref="EntityReference"/> property.
    /// </summary>
    public static implicit operator EntityReference(Entity entity) => new(entity);

    public bool Equals(EntityReference other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is EntityReference other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => IsValid ? Id.ToString() : "(None)";

    public static bool operator ==(EntityReference left, EntityReference right) => left.Equals(right);
    public static bool operator !=(EntityReference left, EntityReference right) => !left.Equals(right);
}
