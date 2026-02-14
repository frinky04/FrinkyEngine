# RaycastHit

Namespace: FrinkyEngine.Core.Physics

Contains information about a single raycast hit against a physics collider.

```csharp
public struct RaycastHit
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [RaycastHit](./frinkyengine.core.physics.raycasthit)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **Entity**

The entity whose collider was hit.

```csharp
public Entity Entity { get; set; }
```

#### Property Value

[Entity](./frinkyengine.core.ecs.entity)<br>

### **Point**

World-space point of the ray impact.

```csharp
public Vector3 Point { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **Normal**

Surface normal at the hit location.

```csharp
public Vector3 Normal { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **Distance**

Distance from the ray origin to the hit point.

```csharp
public float Distance { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
