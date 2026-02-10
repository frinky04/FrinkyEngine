# Scripting

FrinkyEngine uses a composition-based entity model. An `Entity` holds a flat list of `Component` instances. Every entity always has a `TransformComponent` (cannot be removed). Gameplay is written by creating custom components.

## Component Lifecycle

```
Awake → Start → Update / LateUpdate → OnDestroy
                 OnEnable / OnDisable
```

- **Awake** — called once when the component is created
- **Start** — called once before the first Update
- **Update(float dt)** — called every frame with delta time
- **LateUpdate(float dt)** — called every frame after all Update calls
- **OnEnable** — called when the component or entity becomes active
- **OnDisable** — called when the component or entity becomes inactive
- **OnDestroy** — called when the entity is destroyed

## Writing a Custom Component

```csharp
using FrinkyEngine.Core.ECS;

public class RotatorComponent : Component
{
    public float Speed { get; set; } = 90f;

    public override void Update(float dt)
    {
        Transform.LocalRotation *= Quaternion.CreateFromAxisAngle(
            Vector3.UnitY, float.DegreesToRadians(Speed * dt));
    }
}
```

Public read/write properties appear automatically in the inspector and are serialized to scene/prefab JSON.

## Game Assemblies

- Game code compiles to a separate DLL referenced via `.fproject` `gameAssembly`
- Build scripts from the editor with `Scripts -> Build Scripts` (`Ctrl+B`)
- The engine discovers custom components via `ComponentTypeResolver` reflection
- **Hot-reload**: assemblies load through collectible `AssemblyLoadContext` — rebuild and reload without restarting the editor

## Auto-Serialization

Public read/write properties on components are automatically serialized to JSON. Supported types:

| Type | Notes |
|------|-------|
| `float`, `int`, `bool`, `string` | Primitive types |
| `Vector2`, `Vector3`, `Quaternion` | System.Numerics types |
| `Color` | Raylib color |
| Enums | Serialized by name |
| `EntityReference` | Serialized as GUID string (see [Prefabs](prefabs.md)) |
| `AssetReference` | Asset path reference |

## Inspector Attributes

Customize how properties appear in the editor inspector:

| Attribute | Description |
|-----------|-------------|
| `[InspectorLabel("Name")]` | Override the displayed property label |
| `[InspectorRange(min, max, speed)]` | Clamp numeric values and set drag speed (default speed: 0.1) |
| `[InspectorSection("Title")]` | Group properties under a labeled section |
| `[InspectorHeader("Title")]` | Insert a header label before the property |
| `[InspectorTooltip("Text")]` | Show a tooltip on hover |
| `[InspectorReadOnly]` | Display value without allowing edits |
| `[InspectorSpace(height)]` | Insert vertical spacing (default: 8px) |
| `[InspectorIndent(levels)]` | Indent the property (default: 1 level) |
| `[InspectorVisibleIf("Property", expectedValue)]` | Show only when a bool property matches (stackable) |
| `[InspectorVisibleIfEnum("Property", "MemberName")]` | Show only when an enum property matches (stackable) |
| `[InspectorSearchableEnum]` | Use a searchable picker for enum properties |
| `[ComponentCategory("Path")]` | Set category in Add Component menu (slash-separated, e.g. `"Physics/Colliders"`) |
| `[ComponentDisplayName("Name")]` | Override component name in the editor |
| `[AssetFilter(AssetType)]` | Restrict `AssetReference` to a specific asset type |

Example:

```csharp
public class WeaponComponent : Component
{
    [InspectorSection("Stats")]
    [InspectorRange(0, 100, 0.5f)]
    [InspectorTooltip("Damage dealt per hit")]
    public float Damage { get; set; } = 10f;

    [InspectorLabel("Fire Rate (rounds/sec)")]
    public float FireRate { get; set; } = 5f;

    public bool UseAltFire { get; set; }

    [InspectorVisibleIf("UseAltFire")]
    [InspectorIndent]
    public float AltFireDamage { get; set; } = 25f;
}
```

## Input API

Access input state from any component:

- `Input.IsKeyDown(key)` / `Input.IsKeyPressed(key)` / `Input.IsKeyReleased(key)`
- `Input.IsMouseButtonDown(button)` / `Input.IsMouseButtonPressed(button)`
- `Input.MouseDelta` — mouse movement delta
- `Input.MousePosition` — current mouse position

## Scene Queries

- `Scene.GetComponents<T>()` — find all components of a type in the scene
- `Scene.MainCamera` — the active camera entity
- `Scene.TimeScale` — global time scale multiplier (set to 0 to pause gameplay)

## Accessing Other Components

From within a component:

```csharp
// Get a sibling component on the same entity
var rb = Entity.GetComponent<RigidbodyComponent>();

// Access the transform
var pos = Transform.WorldPosition;
Transform.LocalPosition = new Vector3(0, 5, 0);
```
