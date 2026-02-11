# EntityData

Namespace: FrinkyEngine.Core.Serialization

JSON-serializable representation of an entity.

```csharp
public class EntityData
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [EntityData](./frinkyengine.core.serialization.entitydata)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Name**

Entity display name.

```csharp
public string Name { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Id**

Unique identifier.

```csharp
public Guid Id { get; set; }
```

#### Property Value

[Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

### **Active**

Whether the entity is active.

```csharp
public bool Active { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Prefab**

Optional prefab instance metadata.

```csharp
public PrefabInstanceMetadata Prefab { get; set; }
```

#### Property Value

[PrefabInstanceMetadata](./frinkyengine.core.prefabs.prefabinstancemetadata)<br>

### **Components**

Serialized components attached to this entity.

```csharp
public List<ComponentData> Components { get; set; }
```

#### Property Value

[List&lt;ComponentData&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

### **Children**

Serialized child entities.

```csharp
public List<EntityData> Children { get; set; }
```

#### Property Value

[List&lt;EntityData&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

## Constructors

### **EntityData()**

```csharp
public EntityData()
```
