# Component

Namespace: FrinkyEngine.Core.ECS

Base class for all components that can be attached to an [Component.Entity](./frinkyengine.core.ecs.component#entity).

```csharp
public abstract class Component
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

**Remarks:**

Components follow a Unity-style lifecycle: [Component.Awake()](./frinkyengine.core.ecs.component#awake) is called when the component is added,
 [Component.Start()](./frinkyengine.core.ecs.component#start) runs before the first update (only if [Component.Enabled](./frinkyengine.core.ecs.component#enabled)),
 then [Component.Update(Single)](./frinkyengine.core.ecs.component#updatesingle) and [Component.LateUpdate(Single)](./frinkyengine.core.ecs.component#lateupdatesingle) run each frame.
 [Component.OnDestroy()](./frinkyengine.core.ecs.component#ondestroy) is called when the component is removed or the entity is destroyed.

## Properties

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

### **Component()**

```csharp
protected Component()
```

## Methods

### **Awake()**

Called once when the component is first added to an entity, before [Component.Start()](./frinkyengine.core.ecs.component#start).

```csharp
public void Awake()
```

### **Start()**

Called once before the first [Component.Update(Single)](./frinkyengine.core.ecs.component#updatesingle), only if the component is [Component.Enabled](./frinkyengine.core.ecs.component#enabled).

```csharp
public void Start()
```

### **Update(Single)**

Called every frame while the component is enabled.

```csharp
public void Update(float dt)
```

#### Parameters

`dt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Time elapsed since the previous frame, in seconds.

### **LateUpdate(Single)**

Called every frame after all [Component.Update(Single)](./frinkyengine.core.ecs.component#updatesingle) calls have completed.

```csharp
public void LateUpdate(float dt)
```

#### Parameters

`dt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Time elapsed since the previous frame, in seconds.

### **OnDestroy()**

Called when the component is removed from its entity or the entity is destroyed.

```csharp
public void OnDestroy()
```

### **OnEnable()**

Called when the component transitions from disabled to enabled.

```csharp
public void OnEnable()
```

### **OnDisable()**

Called when the component transitions from enabled to disabled.

```csharp
public void OnDisable()
```
