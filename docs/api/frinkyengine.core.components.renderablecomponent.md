# RenderableComponent

Namespace: FrinkyEngine.Core.Components

Abstract base class for components that can be drawn by the [SceneRenderer](./frinkyengine.core.rendering.scenerenderer).
 Provides tint color, ray-collision testing, and world-space bounding box computation.

```csharp
public abstract class RenderableComponent : FrinkyEngine.Core.ECS.Component
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [RenderableComponent](./frinkyengine.core.components.renderablecomponent)

## Properties

### **Tint**

Color multiplier applied when drawing this renderable (defaults to white / fully opaque).

```csharp
public Color Tint { get; set; }
```

#### Property Value

Color<br>

### **Entity**

The [Entity](./frinkyengine.core.ecs.entity) this component is attached to.

```csharp
public Entity Entity { get; internal set; }
```

#### Property Value

[Entity](./frinkyengine.core.ecs.entity)<br>

### **Enabled**

Whether this component is active. Disabled components skip [Component.Update(Single)](./frinkyengine.core.ecs.component#updatesingle) and [Component.LateUpdate(Single)](./frinkyengine.core.ecs.component#lateupdatesingle).

```csharp
public bool Enabled { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **EditorOnly**

When `true`, this component is only active in the editor and is skipped during runtime play.

```csharp
public bool EditorOnly { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **HasStarted**

Indicates whether [Component.Start()](./frinkyengine.core.ecs.component#start) has already been called on this component.

```csharp
public bool HasStarted { get; internal set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **RenderableComponent()**

```csharp
protected RenderableComponent()
```

## Methods

### **Invalidate()**

Marks the internal render model as stale so it will be rebuilt before the next draw.

```csharp
public void Invalidate()
```

### **GetWorldRayCollision(Ray, Boolean)**

Casts a ray against this renderable's mesh in world space.

```csharp
public Nullable<RayCollision> GetWorldRayCollision(Ray ray, bool frontFacesOnly)
```

#### Parameters

`ray` Ray<br>
The ray to test.

`frontFacesOnly` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, ignores back-facing triangles.

#### Returns

[Nullable&lt;RayCollision&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
The closest hit, or `null` if the ray misses.

### **GetWorldRayCollision(Ray, Boolean&, Boolean)**

Casts a ray against this renderable's mesh in world space, also reporting whether mesh data was available.

```csharp
public Nullable<RayCollision> GetWorldRayCollision(Ray ray, Boolean& hasMeshData, bool frontFacesOnly)
```

#### Parameters

`ray` Ray<br>
The ray to test.

`hasMeshData` [Boolean&](https://docs.microsoft.com/en-us/dotnet/api/system.boolean&)<br>
Set to `true` if the model has mesh data to test against.

`frontFacesOnly` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, ignores back-facing triangles.

#### Returns

[Nullable&lt;RayCollision&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
The closest hit, or `null` if the ray misses.

### **GetWorldBoundingBox()**

Computes the axis-aligned bounding box of this renderable in world space.

```csharp
public Nullable<BoundingBox> GetWorldBoundingBox()
```

#### Returns

[Nullable&lt;BoundingBox&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
The world-space bounding box, or `null` if no mesh data is available.
