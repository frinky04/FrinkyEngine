# EntityReference

Namespace: FrinkyEngine.Core.ECS

A stable reference to an [Entity](./frinkyengine.core.ecs.entity) by its [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid).
 Survives renames, serialization round-trips, and play-mode snapshots.

```csharp
public struct EntityReference
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [EntityReference](./frinkyengine.core.ecs.entityreference)<br>
Implements [IEquatable&lt;EntityReference&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.iequatable-1)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Fields

### **None**

An empty reference that does not point to any entity.

```csharp
public static EntityReference None;
```

## Properties

### **Id**

The GUID of the referenced entity.

```csharp
public Guid Id { get; }
```

#### Property Value

[Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

### **IsValid**

Whether this reference points to a valid (non-empty) entity ID.
 Does not guarantee the entity still exists in the scene.

```csharp
public bool IsValid { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **EntityReference(Guid)**

Creates a reference to an entity by its GUID.

```csharp
EntityReference(Guid id)
```

#### Parameters

`id` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

### **EntityReference(Entity)**

Creates a reference to an existing entity.

```csharp
EntityReference(Entity entity)
```

#### Parameters

`entity` [Entity](./frinkyengine.core.ecs.entity)<br>

## Methods

### **Resolve(Scene)**

Resolves this reference against a scene, returning the entity or null if not found.

```csharp
Entity Resolve(Scene scene)
```

#### Parameters

`scene` [Scene](./frinkyengine.core.scene.scene)<br>

#### Returns

[Entity](./frinkyengine.core.ecs.entity)<br>

### **Resolve(Entity)**

Resolves this reference using the scene that the context entity belongs to.

```csharp
Entity Resolve(Entity context)
```

#### Parameters

`context` [Entity](./frinkyengine.core.ecs.entity)<br>

#### Returns

[Entity](./frinkyengine.core.ecs.entity)<br>

### **Equals(EntityReference)**

```csharp
bool Equals(EntityReference other)
```

#### Parameters

`other` [EntityReference](./frinkyengine.core.ecs.entityreference)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Equals(Object)**

```csharp
bool Equals(object obj)
```

#### Parameters

`obj` [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **GetHashCode()**

```csharp
int GetHashCode()
```

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **ToString()**

```csharp
string ToString()
```

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
