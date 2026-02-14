# Physics

Namespace: FrinkyEngine.Core.Physics

Static entry point for physics queries such as raycasting, shape casts, and overlap tests.

```csharp
public static class Physics
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Physics](./frinkyengine.core.physics.physics)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **Raycast(Vector3, Vector3, Single, RaycastHit&, Nullable&lt;RaycastParams&gt;)**

Casts a ray and returns the closest hit, if any.

```csharp
public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, RaycastHit& hit, Nullable<RaycastParams> raycastParams)
```

#### Parameters

`origin` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space origin of the ray.

`direction` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Direction of the ray (does not need to be normalized).

`maxDistance` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Maximum distance the ray can travel.

`hit` [RaycastHit&](./frinkyengine.core.physics.raycasthit&)<br>
Information about the closest hit, if the method returns `true`.

`raycastParams` [Nullable&lt;RaycastParams&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional filtering options for the raycast.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the ray hit something within `maxDistance`.

### **Raycast(Vector3, Vector3, RaycastHit&, Nullable&lt;RaycastParams&gt;)**

Casts a ray between two points and returns the closest hit, if any.

```csharp
public static bool Raycast(Vector3 from, Vector3 to, RaycastHit& hit, Nullable<RaycastParams> raycastParams)
```

#### Parameters

`from` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space start point.

`to` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space end point.

`hit` [RaycastHit&](./frinkyengine.core.physics.raycasthit&)<br>
Information about the closest hit, if the method returns `true`.

`raycastParams` [Nullable&lt;RaycastParams&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional filtering options for the raycast.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the ray hit something between `from` and `to`.

### **RaycastAll(Vector3, Vector3, Single, Nullable&lt;RaycastParams&gt;)**

Casts a ray and returns all hits along it.

```csharp
public static List<RaycastHit> RaycastAll(Vector3 origin, Vector3 direction, float maxDistance, Nullable<RaycastParams> raycastParams)
```

#### Parameters

`origin` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space origin of the ray.

`direction` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Direction of the ray (does not need to be normalized).

`maxDistance` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Maximum distance the ray can travel.

`raycastParams` [Nullable&lt;RaycastParams&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional filtering options for the raycast.

#### Returns

[List&lt;RaycastHit&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>
A list of all hits along the ray, unordered.

### **RaycastAll(Vector3, Vector3, Nullable&lt;RaycastParams&gt;)**

Casts a ray between two points and returns all hits along it.

```csharp
public static List<RaycastHit> RaycastAll(Vector3 from, Vector3 to, Nullable<RaycastParams> raycastParams)
```

#### Parameters

`from` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space start point.

`to` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space end point.

`raycastParams` [Nullable&lt;RaycastParams&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional filtering options for the raycast.

#### Returns

[List&lt;RaycastHit&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>
A list of all hits between `from` and `to`, unordered.

### **SphereCast(Vector3, Single, Vector3, Single, ShapeCastHit&, Nullable&lt;RaycastParams&gt;)**

Sweeps a sphere along a direction and returns the closest hit, if any.

```csharp
public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, float maxDistance, ShapeCastHit& hit, Nullable<RaycastParams> raycastParams)
```

#### Parameters

`origin` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space center of the sphere at the start of the sweep.

`radius` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Radius of the sphere.

`direction` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Direction of the sweep (does not need to be normalized).

`maxDistance` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Maximum distance the sphere can travel.

`hit` [ShapeCastHit&](./frinkyengine.core.physics.shapecasthit&)<br>
Information about the closest hit, if the method returns `true`.

`raycastParams` [Nullable&lt;RaycastParams&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional filtering options.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the sphere hit something within `maxDistance`.

### **SphereCastAll(Vector3, Single, Vector3, Single, Nullable&lt;RaycastParams&gt;)**

Sweeps a sphere along a direction and returns all hits.

```csharp
public static List<ShapeCastHit> SphereCastAll(Vector3 origin, float radius, Vector3 direction, float maxDistance, Nullable<RaycastParams> raycastParams)
```

#### Parameters

`origin` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space center of the sphere at the start of the sweep.

`radius` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Radius of the sphere.

`direction` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Direction of the sweep (does not need to be normalized).

`maxDistance` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Maximum distance the sphere can travel.

`raycastParams` [Nullable&lt;RaycastParams&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional filtering options.

#### Returns

[List&lt;ShapeCastHit&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>
A list of all hits along the sweep, unordered.

### **BoxCast(Vector3, Vector3, Quaternion, Vector3, Single, ShapeCastHit&, Nullable&lt;RaycastParams&gt;)**

Sweeps a box along a direction and returns the closest hit, if any.

```csharp
public static bool BoxCast(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float maxDistance, ShapeCastHit& hit, Nullable<RaycastParams> raycastParams)
```

#### Parameters

`origin` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space center of the box at the start of the sweep.

`halfExtents` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Half-size of the box on each axis.

`orientation` [Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>
Rotation of the box.

`direction` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Direction of the sweep (does not need to be normalized).

`maxDistance` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Maximum distance the box can travel.

`hit` [ShapeCastHit&](./frinkyengine.core.physics.shapecasthit&)<br>
Information about the closest hit, if the method returns `true`.

`raycastParams` [Nullable&lt;RaycastParams&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional filtering options.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the box hit something within `maxDistance`.

### **BoxCastAll(Vector3, Vector3, Quaternion, Vector3, Single, Nullable&lt;RaycastParams&gt;)**

Sweeps a box along a direction and returns all hits.

```csharp
public static List<ShapeCastHit> BoxCastAll(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float maxDistance, Nullable<RaycastParams> raycastParams)
```

#### Parameters

`origin` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space center of the box at the start of the sweep.

`halfExtents` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Half-size of the box on each axis.

`orientation` [Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>
Rotation of the box.

`direction` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Direction of the sweep (does not need to be normalized).

`maxDistance` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Maximum distance the box can travel.

`raycastParams` [Nullable&lt;RaycastParams&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional filtering options.

#### Returns

[List&lt;ShapeCastHit&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>
A list of all hits along the sweep, unordered.

### **CapsuleCast(Vector3, Single, Single, Quaternion, Vector3, Single, ShapeCastHit&, Nullable&lt;RaycastParams&gt;)**

Sweeps a capsule along a direction and returns the closest hit, if any.

```csharp
public static bool CapsuleCast(Vector3 origin, float radius, float length, Quaternion orientation, Vector3 direction, float maxDistance, ShapeCastHit& hit, Nullable<RaycastParams> raycastParams)
```

#### Parameters

`origin` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space center of the capsule at the start of the sweep.

`radius` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Radius of the capsule.

`length` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Length of the capsule's cylindrical segment (total height = length + 2 * radius).

`orientation` [Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>
Rotation of the capsule.

`direction` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Direction of the sweep (does not need to be normalized).

`maxDistance` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Maximum distance the capsule can travel.

`hit` [ShapeCastHit&](./frinkyengine.core.physics.shapecasthit&)<br>
Information about the closest hit, if the method returns `true`.

`raycastParams` [Nullable&lt;RaycastParams&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional filtering options.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the capsule hit something within `maxDistance`.

### **CapsuleCastAll(Vector3, Single, Single, Quaternion, Vector3, Single, Nullable&lt;RaycastParams&gt;)**

Sweeps a capsule along a direction and returns all hits.

```csharp
public static List<ShapeCastHit> CapsuleCastAll(Vector3 origin, float radius, float length, Quaternion orientation, Vector3 direction, float maxDistance, Nullable<RaycastParams> raycastParams)
```

#### Parameters

`origin` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space center of the capsule at the start of the sweep.

`radius` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Radius of the capsule.

`length` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Length of the capsule's cylindrical segment (total height = length + 2 * radius).

`orientation` [Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>
Rotation of the capsule.

`direction` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Direction of the sweep (does not need to be normalized).

`maxDistance` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Maximum distance the capsule can travel.

`raycastParams` [Nullable&lt;RaycastParams&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional filtering options.

#### Returns

[List&lt;ShapeCastHit&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>
A list of all hits along the sweep, unordered.

### **OverlapSphere(Vector3, Single, Nullable&lt;RaycastParams&gt;)**

Finds all entities whose colliders overlap a sphere at the given position.

```csharp
public static List<Entity> OverlapSphere(Vector3 center, float radius, Nullable<RaycastParams> raycastParams)
```

#### Parameters

`center` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space center of the overlap sphere.

`radius` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Radius of the sphere.

`raycastParams` [Nullable&lt;RaycastParams&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional filtering options.

#### Returns

[List&lt;Entity&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>
A list of entities whose colliders overlap the sphere.

### **OverlapBox(Vector3, Vector3, Quaternion, Nullable&lt;RaycastParams&gt;)**

Finds all entities whose colliders overlap a box at the given position and orientation.

```csharp
public static List<Entity> OverlapBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, Nullable<RaycastParams> raycastParams)
```

#### Parameters

`center` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space center of the overlap box.

`halfExtents` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Half-size of the box on each axis.

`orientation` [Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>
Rotation of the box.

`raycastParams` [Nullable&lt;RaycastParams&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional filtering options.

#### Returns

[List&lt;Entity&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>
A list of entities whose colliders overlap the box.

### **OverlapCapsule(Vector3, Single, Single, Quaternion, Nullable&lt;RaycastParams&gt;)**

Finds all entities whose colliders overlap a capsule at the given position and orientation.

```csharp
public static List<Entity> OverlapCapsule(Vector3 center, float radius, float length, Quaternion orientation, Nullable<RaycastParams> raycastParams)
```

#### Parameters

`center` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
World-space center of the overlap capsule.

`radius` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Radius of the capsule.

`length` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Length of the capsule's cylindrical segment.

`orientation` [Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>
Rotation of the capsule.

`raycastParams` [Nullable&lt;RaycastParams&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional filtering options.

#### Returns

[List&lt;Entity&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>
A list of entities whose colliders overlap the capsule.
