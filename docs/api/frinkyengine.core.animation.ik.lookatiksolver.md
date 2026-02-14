# LookAtIKSolver

Namespace: FrinkyEngine.Core.Animation.IK

Rotates a single bone so that a chosen local axis points toward a target position.
 Useful for head tracking, turrets, eye gaze, etc.

```csharp
public class LookAtIKSolver : IKSolver
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FObject](./frinkyengine.core.ecs.fobject) → [IKSolver](./frinkyengine.core.animation.ik.iksolver) → [LookAtIKSolver](./frinkyengine.core.animation.ik.lookatiksolver)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **DisplayName**

```csharp
public string DisplayName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **BoneIndex**

The bone to rotate. Dropdown index where 0 = (none).

```csharp
public int BoneIndex { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **TargetSpace**

Coordinate space for [LookAtIKSolver.TargetPosition](./frinkyengine.core.animation.ik.lookatiksolver#targetposition).

```csharp
public IKTargetSpace TargetSpace { get; set; }
```

#### Property Value

[IKTargetSpace](./frinkyengine.core.animation.ik.iktargetspace)<br>

### **TargetPosition**

Target position to look at.

```csharp
public Vector3 TargetPosition { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **AimAxis**

Which local bone axis should point toward the target.

```csharp
public LocalAxis AimAxis { get; set; }
```

#### Property Value

[LocalAxis](./frinkyengine.core.animation.ik.localaxis)<br>

### **UpAxis**

Which local bone axis should align toward world up.

```csharp
public LocalAxis UpAxis { get; set; }
```

#### Property Value

[LocalAxis](./frinkyengine.core.animation.ik.localaxis)<br>

### **IsConfigured**

```csharp
public bool IsConfigured { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Enabled**

Whether this solver is active.

```csharp
public bool Enabled { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Weight**

Blend weight for this solver (0 = no effect, 1 = full IK).

```csharp
public float Weight { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

## Constructors

### **LookAtIKSolver()**

```csharp
public LookAtIKSolver()
```

## Methods

### **CanSolve(BoneHierarchy)**

```csharp
public bool CanSolve(BoneHierarchy hierarchy)
```

#### Parameters

`hierarchy` [BoneHierarchy](./frinkyengine.core.animation.ik.bonehierarchy)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Solve(ValueTuple`3[], BoneHierarchy, Matrix4x4, Matrix4x4[])**

```csharp
public void Solve(ValueTuple`3[] localTransforms, BoneHierarchy hierarchy, Matrix4x4 entityWorldMatrix, Matrix4x4[] worldMatrices)
```

#### Parameters

`localTransforms` [ValueTuple`3[]](https://docs.microsoft.com/en-us/dotnet/api/system.valuetuple-3)<br>

`hierarchy` [BoneHierarchy](./frinkyengine.core.animation.ik.bonehierarchy)<br>

`entityWorldMatrix` [Matrix4x4](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4)<br>

`worldMatrices` [Matrix4x4[]](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4)<br>
