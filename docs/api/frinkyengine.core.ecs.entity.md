# Entity

Namespace: FrinkyEngine.Core.ECS

A game object composed of [Component](./frinkyengine.core.ecs.component) instances.
 Every entity always has a [TransformComponent](./frinkyengine.core.components.transformcomponent) that cannot be removed.

```csharp
public class Entity
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Entity](./frinkyengine.core.ecs.entity)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Name**

Display name of this entity.

```csharp
public string Name { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Id**

Globally unique identifier for this entity, assigned automatically on creation.

```csharp
public Guid Id { get; set; }
```

#### Property Value

[Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

### **Active**

Whether this entity participates in updates and rendering. Inactive entities are skipped.

```csharp
public bool Active { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Scene**

The [Scene](./frinkyengine.core.scene.scene) this entity currently belongs to, or `null` if not in a scene.

```csharp
public Scene Scene { get; internal set; }
```

#### Property Value

[Scene](./frinkyengine.core.scene.scene)<br>

### **Transform**

The transform component that is always present on every entity.

```csharp
public TransformComponent Transform { get; }
```

#### Property Value

[TransformComponent](./frinkyengine.core.components.transformcomponent)<br>

### **Prefab**

Optional prefab instance metadata for this entity.

```csharp
public PrefabInstanceMetadata Prefab { get; set; }
```

#### Property Value

[PrefabInstanceMetadata](./frinkyengine.core.prefabs.prefabinstancemetadata)<br>

### **Components**

All components currently attached to this entity, including the [Entity.Transform](./frinkyengine.core.ecs.entity#transform).

```csharp
public IReadOnlyList<Component> Components { get; }
```

#### Property Value

[IReadOnlyList&lt;Component&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

## Constructors

### **Entity(String)**

Creates a new entity with the specified name and a default [TransformComponent](./frinkyengine.core.components.transformcomponent).

```csharp
public Entity(string name)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Display name for the entity (defaults to "Entity").

## Methods

### **AddComponent&lt;T&gt;()**

Creates and attaches a new component of type .

```csharp
public T AddComponent<T>()
```

#### Type Parameters

`T`<br>
The component type to add. Must have a parameterless constructor.

#### Returns

T<br>
The newly created component instance.

#### Exceptions

[InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/system.invalidoperationexception)<br>
Thrown if  is [TransformComponent](./frinkyengine.core.components.transformcomponent).

### **AddComponent(Type)**

Creates and attaches a new component of the specified runtime type.

```csharp
public Component AddComponent(Type type)
```

#### Parameters

`type` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>
The component type to add. Must derive from [Component](./frinkyengine.core.ecs.component) and have a parameterless constructor.

#### Returns

[Component](./frinkyengine.core.ecs.component)<br>
The newly created component instance.

#### Exceptions

[InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/system.invalidoperationexception)<br>
Thrown if `type` is [TransformComponent](./frinkyengine.core.components.transformcomponent).

[ArgumentException](https://docs.microsoft.com/en-us/dotnet/api/system.argumentexception)<br>
Thrown if `type` does not derive from [Component](./frinkyengine.core.ecs.component).

### **GetComponent&lt;T&gt;()**

Gets the first component of type  attached to this entity.

```csharp
public T GetComponent<T>()
```

#### Type Parameters

`T`<br>
The component type to search for.

#### Returns

T<br>
The component instance, or `null` if none is found.

### **GetComponent(Type)**

Gets the first component of the specified runtime type attached to this entity.

```csharp
public Component GetComponent(Type type)
```

#### Parameters

`type` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>
The component type to search for.

#### Returns

[Component](./frinkyengine.core.ecs.component)<br>
The component instance, or `null` if none is found.

### **HasComponent&lt;T&gt;()**

Checks whether this entity has a component of type .

```csharp
public bool HasComponent<T>()
```

#### Type Parameters

`T`<br>
The component type to check for.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if a matching component exists.

### **RemoveComponent&lt;T&gt;()**

Removes the first component of type  from this entity.

```csharp
public bool RemoveComponent<T>()
```

#### Type Parameters

`T`<br>
The component type to remove.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if a component was found and removed.

#### Exceptions

[InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/system.invalidoperationexception)<br>
Thrown if  is [TransformComponent](./frinkyengine.core.components.transformcomponent).

### **RemoveComponent(Component)**

Removes a specific component instance from this entity.

```csharp
public bool RemoveComponent(Component component)
```

#### Parameters

`component` [Component](./frinkyengine.core.ecs.component)<br>
The component to remove.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the component was found and removed.

#### Exceptions

[InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/system.invalidoperationexception)<br>
Thrown if `component` is a [TransformComponent](./frinkyengine.core.components.transformcomponent).
