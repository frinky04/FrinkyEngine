# Prefabs & Entity References

## Prefabs

Prefabs store reusable entity hierarchies as `.fprefab` JSON files.

### Workflow

1. **Create** — right-click an entity in the hierarchy and select "Create Prefab" (`Ctrl+Shift+M`), or use `PrefabSerializer.CreateFromEntity()`
2. **Instantiate** — drag a `.fprefab` from the asset browser into the viewport or hierarchy
3. **Override** — modify properties on a prefab instance; changes are tracked as overrides against the prefab source
4. **Update** — changes to the source `.fprefab` propagate to all instances (overridden properties are preserved)

### Prefab Operations

| Operation | Shortcut | Description |
|-----------|----------|-------------|
| Apply Prefab | Ctrl+Alt+P | Push instance overrides back to the source `.fprefab` |
| Revert Prefab | Ctrl+Alt+R | Discard instance overrides and match the source |
| Make Unique | Ctrl+Shift+U | Save a new `.fprefab` from this instance |
| Unpack | Ctrl+Alt+K | Break the prefab link, turning the instance into regular entities |

### Runtime Instantiation

Spawn prefabs from scripts using `Scene.Instantiate()`:

```csharp
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Assets;

public class SpawnerComponent : Component
{
    [AssetFilter(AssetType.Prefab)]
    public AssetReference ProjectilePrefab { get; set; }

    public override void Update(float dt)
    {
        if (Input.IsMouseButtonPressed(0))
        {
            // Spawn at a position and rotation
            var entity = Entity.Scene.Instantiate(
                ProjectilePrefab,
                Transform.WorldPosition,
                Transform.WorldRotation);

            // Or spawn with just a path
            // var entity = Entity.Scene.Instantiate("Prefabs/Bullet.fprefab");

            // Optionally parent to another transform
            // var entity = Entity.Scene.Instantiate(ProjectilePrefab, parent: someTransform);
        }
    }
}
```

All overloads accept an optional `parent` parameter. Entity references within the prefab are automatically remapped to the new instance.

### Internals

Prefabs use **stable IDs** to track entities across edits and support nested hierarchies. When a prefab is instantiated, entity IDs are remapped so each instance gets unique IDs while maintaining internal cross-references.

## Entity References

`EntityReference` is a struct for linking one entity to another. It stores the target entity's GUID and resolves at runtime.

### Usage

```csharp
using FrinkyEngine.Core.ECS;

public class MyComponent : Component
{
    public EntityReference Target { get; set; }

    public override void Update(float dt)
    {
        var entity = Target.Resolve(Scene);
        if (entity != null)
        {
            // Use the referenced entity
            var targetPos = entity.Transform.WorldPosition;
        }
    }
}
```

### Serialization

- Serialized as a GUID string in scene/prefab JSON
- Automatically resolved at runtime via `Scene.FindEntityById()`

### Remapping

Entity references are automatically remapped in these scenarios:

- **Duplication** — when duplicating entities, references between co-duplicated entities point to the new copies
- **Prefab instantiation** — references between entities within the same prefab are remapped to the new instance's entities

References to entities *outside* the duplicated/instantiated group remain unchanged.

### Inspector Assignment

Drag an entity from the hierarchy panel onto an `EntityReference` field in the inspector to assign it. The inspector shows the referenced entity's name or "None" if unassigned.
