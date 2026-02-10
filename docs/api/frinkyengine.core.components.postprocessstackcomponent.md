# PostProcessStackComponent

Namespace: FrinkyEngine.Core.Components

Attach to a camera entity to define an ordered stack of post-processing effects.
 Effects are applied in list order after the scene is rendered.

```csharp
public class PostProcessStackComponent : FrinkyEngine.Core.ECS.Component
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [PostProcessStackComponent](./frinkyengine.core.components.postprocessstackcomponent)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute), [ComponentDisplayNameAttribute](./frinkyengine.core.ecs.componentdisplaynameattribute)

## Properties

### **PostProcessingEnabled**

Master toggle for all post-processing on this camera.

```csharp
public bool PostProcessingEnabled { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Effects**

Ordered list of post-processing effects to apply.

```csharp
public List<PostProcessEffect> Effects { get; set; }
```

#### Property Value

[List&lt;PostProcessEffect&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

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

### **PostProcessStackComponent()**

```csharp
public PostProcessStackComponent()
```

## Methods

### **OnDestroy()**

```csharp
public void OnDestroy()
```
