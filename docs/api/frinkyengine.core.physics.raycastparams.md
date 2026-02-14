# RaycastParams

Namespace: FrinkyEngine.Core.Physics

Options for filtering raycast results.

```csharp
public struct RaycastParams
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [RaycastParams](./frinkyengine.core.physics.raycastparams)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **IncludeTriggers**

When `true`, trigger colliders are included in the test.

```csharp
public bool IncludeTriggers { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **IgnoredEntities**

Set of entities whose colliders should be skipped by the raycast.

```csharp
public HashSet<Entity> IgnoredEntities { get; set; }
```

#### Property Value

[HashSet&lt;Entity&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1)<br>

## Methods

### **IgnoreEntityTree(Entity)**

Populates [RaycastParams.IgnoredEntities](./frinkyengine.core.physics.raycastparams#ignoredentities) with the full hierarchy tree
 (root and all descendants) of the given entity. Walks up to the root
 parent, then collects the entire subtree.

```csharp
void IgnoreEntityTree(Entity entity)
```

#### Parameters

`entity` [Entity](./frinkyengine.core.ecs.entity)<br>
Any entity in the tree to ignore.
