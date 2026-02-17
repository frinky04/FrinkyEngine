# Scripting

FrinkyEngine uses a composition-based entity model. An `Entity` holds a flat list of `Component` instances. Every entity always has a `TransformComponent` (cannot be removed). Gameplay is written by creating custom components.

## Component Lifecycle

```
Awake → Start → Update / LateUpdate → OnDestroy
                 OnEnable / OnDisable
                 OnTriggerEnter / OnTriggerStay / OnTriggerExit
                 OnCollisionEnter / OnCollisionStay / OnCollisionExit
                 Coroutines & Timers (ticked after Update)
```

- **Awake** — called once when the component is created
- **Start** — called once before the first Update
- **Update(float dt)** — called every frame with delta time
- **LateUpdate(float dt)** — called every frame after all Update calls
- **OnEnable** — called when the component or entity becomes active
- **OnDisable** — called when the component or entity becomes inactive
- **OnDestroy** — called when the entity is destroyed
- **OnTriggerEnter(Entity other)** — called when a trigger overlap begins
- **OnTriggerStay(Entity other)** — called each frame while a trigger overlap persists
- **OnTriggerExit(Entity other)** — called when a trigger overlap ends
- **OnCollisionEnter(CollisionInfo info)** — called when a physics collision begins
- **OnCollisionStay(CollisionInfo info)** — called each frame while a collision persists
- **OnCollisionExit(CollisionInfo info)** — called when a physics collision ends

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

## Constructor Requirements

Custom components loaded from scenes and prefabs must have a **public parameterless constructor**.

- If a component cannot be default-constructed, FrinkyEngine skips that component during load and preserves its serialized data.
- Use `Awake` and `Start` for runtime initialization logic instead of constructor parameters.

## Game Assemblies

- Game code compiles to a separate DLL referenced via `.fproject` `gameAssembly`
- Build scripts from the editor with `File -> Build Scripts` (`Ctrl+B`)
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
| `FObject` subclasses | Polymorphic `$type` + `properties` (see [FObjects](#fobjects--polymorphic-data-objects)) |
| `List<T>` where T : `FObject` | Array of polymorphic objects |

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
| `[InspectorHidden]` | Hide a public property from inspector drawing while keeping serialization |
| `[InspectorSpace(height)]` | Insert vertical spacing (default: 8px) |
| `[InspectorIndent(levels)]` | Indent the property (default: 1 level) |
| `[InspectorVisibleIf("Member", expectedValue)]` | Show only when a bool member (property/field/method) matches (stackable) |
| `[InspectorVisibleIfEnum("Member", "EnumValue")]` | Show only when an enum member (property/field/method) matches (stackable) |
| `[InspectorSearchableEnum]` | Use a searchable picker for enum properties |
| `[InspectorVector3Style(...)]` | Customize `Vector3` controls (for example colored XYZ reset buttons and reset defaults) |
| `[InspectorOnChanged("MethodName")]` | Invoke a parameterless method after inspector edits to that property |
| `[InspectorListFactory("MethodName")]` | Use a parameterless factory method when adding new `List<T>` items |
| `[InspectorDropdown("MethodName")]` | Render an `int` property as a combo box; method returns `string[]` of labels |
| `[InspectorButton("Label", ...)]` | Render a parameterless `void`/`bool` method as a clickable inspector button |
| `[InspectorMessageIf("Condition", "Text", ...)]` | Show class-level info/warning/error text when a bool member evaluates true |
| `[ComponentCategory("Path")]` | Set category in Add Component menu (slash-separated, e.g. `"Physics/Colliders"`) |
| `[ComponentDisplayName("Name")]` | Override component name in the editor |
| `[InspectorGizmo("Label", ...)]` | Draw a draggable viewport gizmo sphere for a `Vector3` property (see below) |
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

`InspectorButton` notes:

- Methods must be parameterless and return `void` or `bool`
- `Mode` supports `Always`, `EditorOnly`, `RuntimeOnly`
- `DisableWhen` can reference a bool property/field/method

`InspectorMessageIf` notes:

- Targets classes (including script components)
- Condition member can be a bool property/field/method
- `Severity` supports `Info`, `Warning`, `Error`

`InspectorGizmo` notes:

- Applies to `Vector3` properties on components or `FObject` subclasses
- The gizmo sphere is drawn in the viewport and can be clicked and dragged to reposition
- Optional named parameters: `GizmoRadius` (default 0.1), `ColorR`/`ColorG`/`ColorB` (default yellow), `SpaceProperty` (name of a sibling enum property with `World`/`Local` members — when `Local`, the position is transformed from entity-local to world space)

```csharp
public class LookAtComponent : Component
{
    public IKTargetSpace TargetSpace { get; set; } = IKTargetSpace.World;

    [InspectorGizmo("Look Target", ColorR = 50, ColorG = 255, ColorB = 50, SpaceProperty = nameof(TargetSpace))]
    public Vector3 TargetPosition { get; set; }
}
```

The inspector also reflects nested classes/structs and `List<T>` properties (with add/remove/reorder and recursive editing), so script data models can use the same workflows as built-in engine components.

`InspectorOnChanged` callbacks must be parameterless and return `void`.

## FObjects — Polymorphic Data Objects

FObjects are a general-purpose composition system for configurable, type-selectable data owned by components. Use them for things like AI behaviors, weapon configs, ability definitions, loot tables, or any case where a component needs a property whose concrete type is chosen at edit time.

### Writing an FObject

Subclass `FObject` and add public read/write properties:

```csharp
using FrinkyEngine.Core.ECS;

public abstract class AIBehavior : FObject { }

public class AggressiveBehavior : AIBehavior
{
    public override string DisplayName => "Aggressive";
    public float AggressionLevel { get; set; } = 0.8f;
    public float ChaseRange { get; set; } = 20f;
}

public class PassiveBehavior : AIBehavior
{
    public override string DisplayName => "Passive";
    public float FleeRange { get; set; } = 15f;
}
```

### Using FObjects in Components

Declare properties typed as an FObject subclass (single or list):

```csharp
public class EnemyComponent : Component
{
    // Single FObject — inspector shows a type dropdown (None, Aggressive, Passive)
    public AIBehavior? PrimaryBehavior { get; set; }

    // List of FObjects — inspector shows collapsible list with Add/Remove/Reorder
    public List<AIBehavior> Behaviors { get; set; } = new();

    public override void Update(float dt)
    {
        // Access FObject properties directly
        if (PrimaryBehavior is AggressiveBehavior aggressive)
        {
            // Chase logic using aggressive.ChaseRange
        }
    }
}
```

### Inspector Behavior

- **Single FObject property**: combo dropdown with `(None)` plus all concrete types assignable to the declared type, followed by inline property editing
- **List&lt;FObject&gt; property**: collapsible headers per entry with reorder (^/v), remove (X) buttons and an "Add" button with a type picker popup
- **Multi-entity editing**: shows "(edit individually)" for FObject properties
- **Nested FObjects**: FObject properties can themselves contain FObject or List&lt;FObject&gt; properties — the inspector recurses automatically

### Serialization

FObjects serialize to JSON with the same `$type` + `properties` pattern used by post-processing effects:

```json
{
  "$type": "MyGame.Scripts.AggressiveBehavior",
  "properties": {
    "AggressionLevel": 0.8,
    "ChaseRange": 20.0
  }
}
```

All serializable property types supported by components also work inside FObjects (float, int, bool, string, Vector2/3, Color, enums, EntityReference, AssetReference, nested FObjects).

EntityReferences inside FObjects are correctly remapped during entity duplication, prefab instantiation, and prefab application.

### Creating FObject Scripts

Use `Create -> Create Script...` in the editor menu bar. Select `FObject` (or any concrete FObject subclass) as the base class. The generated template includes a `DisplayName` override.

### Hot-Reload

FObject types from game assemblies are discovered automatically via `FObjectTypeResolver`. When a game assembly is rebuilt and reloaded, new FObject types appear in inspector dropdowns and removed types are cleaned up.

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

### Finding Entities

```csharp
// Find the first entity with a given name
var player = Entity.Scene.FindEntityByName("Player");

// Find all entities with a given name
List<Entity> enemies = Entity.Scene.FindEntitiesByName("Enemy");

// Find all entities that have a specific component
List<Entity> cameras = Entity.Scene.FindEntitiesWithComponent<CameraComponent>();
```

All searches are linear scans over the entity list.

## Accessing Other Components

From within a component:

```csharp
// Get a sibling component on the same entity
var rb = Entity.GetComponent<RigidbodyComponent>();

// Get all components of a type on this entity
List<MeshRendererComponent> renderers = Entity.GetComponents<MeshRendererComponent>();

// Access the transform
var pos = Transform.WorldPosition;
Transform.LocalPosition = new Vector3(0, 5, 0);
```

### Hierarchy Traversal

Search up or down the entity hierarchy for components:

```csharp
// First matching component in children (depth-first)
var collider = Entity.GetComponentInChildren<ColliderComponent>();

// All matching components in children
List<ColliderComponent> allColliders = Entity.GetComponentsInChildren<ColliderComponent>();

// Include inactive entities in the search
var hidden = Entity.GetComponentInChildren<MeshRendererComponent>(includeInactive: true);

// Walk up the parent chain
var controller = Entity.GetComponentInParent<CharacterControllerComponent>();
```

## Destroying Entities

```csharp
// Immediate destruction (removes entity and all children from the scene)
Entity.Destroy();

// Deferred destruction after a delay (in seconds)
Entity.Destroy(2.5f);

// End-of-frame destruction (safe to call during iteration)
Entity.Destroy(0f);
```

Deferred destroys are processed at the end of each `Scene.Update()` call. A delay of `0` destroys the entity at the end of the current frame.

## Timers

Schedule callbacks with a delay, respecting `Scene.TimeScale`:

```csharp
// One-shot: call after 2 seconds
Invoke(OnExplosion, 2f);

// Repeating: first call after 1 second, then every 0.5 seconds
InvokeRepeating(SpawnEnemy, 1f, 0.5f);

// Cancel all timers on this component
CancelInvoke();

// Cancel timers for a specific callback
CancelInvoke(SpawnEnemy);
```

Timers are automatically cancelled when the component is destroyed.

## Coroutines

Coroutines let you write multi-step sequences that execute over multiple frames:

```csharp
using System.Collections;
using FrinkyEngine.Core.Coroutines;

public class ExampleComponent : Component
{
    private Coroutine? _flashRoutine;

    public override void Start()
    {
        _flashRoutine = StartCoroutine(FlashSequence());
    }

    private IEnumerator FlashSequence()
    {
        // Wait one frame
        yield return null;

        // Wait 2 seconds (scaled time)
        yield return new WaitForSeconds(2f);

        // Wait 1 second (real time, ignores TimeScale)
        yield return new WaitForSecondsRealtime(1f);

        // Wait until a condition is true
        yield return new WaitUntil(() => Entity.Scene.Time > 10f);

        // Wait while a condition is true
        yield return new WaitWhile(() => someFlag);
    }

    public override void Update(float dt)
    {
        // Stop a specific coroutine
        if (someCondition && _flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
            _flashRoutine = null;
        }
    }

    public override void OnDestroy()
    {
        // All coroutines are automatically cancelled on destroy,
        // but you can also stop them manually:
        StopAllCoroutines();
    }
}
```

### Yield Instructions

| Yield Value | Behavior |
|-------------|----------|
| `null` | Resume next frame |
| `new WaitForSeconds(float)` | Wait for scaled time |
| `new WaitForSecondsRealtime(float)` | Wait for real (unscaled) time |
| `new WaitUntil(Func<bool>)` | Resume when condition is true |
| `new WaitWhile(Func<bool>)` | Resume when condition is false |

### Coroutine Lifecycle

- Coroutines **pause** when the component is disabled and **resume** when re-enabled
- Coroutines are **cancelled** when the component is destroyed
- Use `StopCoroutine(handle)` to stop a specific coroutine
- Use `StopAllCoroutines()` to stop all coroutines on the component

## Scene Time

Access timing information from any component via `Entity.Scene`:

```csharp
// Total elapsed scaled time since scene started
float t = Entity.Scene.Time;

// Total elapsed real time (ignores TimeScale)
float realT = Entity.Scene.UnscaledTime;

// Unscaled delta time for this frame
float rawDt = Entity.Scene.UnscaledDeltaTime;

// Total frames elapsed since scene started
long frame = Entity.Scene.FrameCount;

// Global time scale (static, affects all scenes)
Scene.TimeScale = 0.5f; // half speed
```
