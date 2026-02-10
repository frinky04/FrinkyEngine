# LightComponent

Namespace: FrinkyEngine.Core.Components

Adds a light source to the scene. The light's position and direction come from the entity's [TransformComponent](./frinkyengine.core.components.transformcomponent).

```csharp
public class LightComponent : FrinkyEngine.Core.ECS.Component
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [LightComponent](./frinkyengine.core.components.lightcomponent)<br>
Attributes [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute)

## Properties

### **LightType**

The type of light (directional, point, or skylight).

```csharp
public LightType LightType { get; set; }
```

#### Property Value

[LightType](./frinkyengine.core.components.lighttype)<br>

### **LightColor**

The color of the emitted light (defaults to white).

```csharp
public Color LightColor { get; set; }
```

#### Property Value

Color<br>

### **Intensity**

Brightness multiplier applied to the light color (defaults to 1).

```csharp
public float Intensity { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Range**

Maximum distance for point light attenuation, in world units (defaults to 10).

```csharp
public float Range { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

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

### **LightComponent()**

```csharp
public LightComponent()
```
