# CameraComponent

Namespace: FrinkyEngine.Core.Components

Provides a camera viewpoint that the renderer uses to draw the scene.
 Attach to an entity to position the camera via the entity's [TransformComponent](./frinkyengine.core.components.transformcomponent).

```csharp
public class CameraComponent : FrinkyEngine.Core.ECS.Component
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [CameraComponent](./frinkyengine.core.components.cameracomponent)<br>
Attributes [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute)

## Properties

### **FieldOfView**

Vertical field of view in degrees, used for perspective projection (defaults to 60).

```csharp
public float FieldOfView { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **NearPlane**

Distance to the near clipping plane (defaults to 0.1).

```csharp
public float NearPlane { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **FarPlane**

Distance to the far clipping plane (defaults to 1000).

```csharp
public float FarPlane { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Projection**

Whether this camera uses perspective or orthographic projection.

```csharp
public ProjectionType Projection { get; set; }
```

#### Property Value

[ProjectionType](./frinkyengine.core.components.projectiontype)<br>

### **ClearColor**

Background color used to clear the screen before rendering (defaults to dark gray).

```csharp
public Color ClearColor { get; set; }
```

#### Property Value

Color<br>

### **IsMain**

When `true`, marks this as the primary camera used for rendering.

```csharp
public bool IsMain { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

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

### **CameraComponent()**

```csharp
public CameraComponent()
```

## Methods

### **BuildCamera3D()**

Builds a Raylib  from this component's settings and the entity's transform.

```csharp
public Camera3D BuildCamera3D()
```

#### Returns

Camera3D<br>
A configured  ready for rendering.
