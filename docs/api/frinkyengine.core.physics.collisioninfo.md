# CollisionInfo

Namespace: FrinkyEngine.Core.Physics

Contains information about a collision between two physics bodies.

```csharp
public struct CollisionInfo
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [CollisionInfo](./frinkyengine.core.physics.collisioninfo)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **Other**

The other entity involved in the collision.

```csharp
public Entity Other { get; set; }
```

#### Property Value

[Entity](./frinkyengine.core.ecs.entity)<br>

### **ContactPoint**

World-space contact point.

```csharp
public Vector3 ContactPoint { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **Normal**

Contact normal pointing from the other entity toward this entity.

```csharp
public Vector3 Normal { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **PenetrationDepth**

Penetration depth of the collision contact.

```csharp
public float PenetrationDepth { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
