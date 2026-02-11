# ComponentRegistry

Namespace: FrinkyEngine.Core.Scene

Fast polymorphic component lookup used internally by [Scene](./frinkyengine.core.scene.scene) to maintain quick-access lists.
 Components are indexed by their concrete type and all base types up to (but not including) [Component](./frinkyengine.core.ecs.component).

```csharp
public class ComponentRegistry
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ComponentRegistry](./frinkyengine.core.scene.componentregistry)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Constructors

### **ComponentRegistry()**

```csharp
public ComponentRegistry()
```

## Methods

### **Register(Component)**

Registers a component, indexing it under its concrete type and all intermediate base types.

```csharp
public void Register(Component component)
```

#### Parameters

`component` [Component](./frinkyengine.core.ecs.component)<br>
The component to register.

### **Unregister(Component)**

Removes a component from all type-indexed lists.

```csharp
public void Unregister(Component component)
```

#### Parameters

`component` [Component](./frinkyengine.core.ecs.component)<br>
The component to unregister.

### **GetComponents&lt;T&gt;()**

Gets all registered components of type .

```csharp
public List<T> GetComponents<T>()
```

#### Type Parameters

`T`<br>
The component type to query.

#### Returns

List&lt;T&gt;<br>
A list of matching components (may be empty).

### **GetComponentsRaw(Type)**

Gets all registered components of the specified runtime type.

```csharp
public IReadOnlyList<Component> GetComponentsRaw(Type type)
```

#### Parameters

`type` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>
The component type to query.

#### Returns

[IReadOnlyList&lt;Component&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>
A read-only list of matching components (may be empty).
