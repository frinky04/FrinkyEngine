# FABRIKSolver

Namespace: FrinkyEngine.Core.Animation.IK

FABRIK (Forward And Backward Reaching Inverse Kinematics) solver for arbitrary-length bone chains.

```csharp
public class FABRIKSolver : IKSolver
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FObject](./frinkyengine.core.ecs.fobject) → [IKSolver](./frinkyengine.core.animation.ik.iksolver) → [FABRIKSolver](./frinkyengine.core.animation.ik.fabriksolver)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **DisplayName**

```csharp
public string DisplayName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **RootBoneIndex**

Root bone of the chain. Dropdown index where 0 = (none).

```csharp
public int RootBoneIndex { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **EndBoneIndex**

End effector bone. Dropdown index where 0 = (none).

```csharp
public int EndBoneIndex { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **TargetSpace**

Coordinate space for [FABRIKSolver.TargetPosition](./frinkyengine.core.animation.ik.fabriksolver#targetposition).

```csharp
public IKTargetSpace TargetSpace { get; set; }
```

#### Property Value

[IKTargetSpace](./frinkyengine.core.animation.ik.iktargetspace)<br>

### **TargetPosition**

Target position the end effector should reach toward.

```csharp
public Vector3 TargetPosition { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **MaxIterations**

Maximum number of FABRIK iterations per solve.

```csharp
public int MaxIterations { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Tolerance**

Convergence threshold in world units. Iteration stops when the end effector is within this distance of the target.

```csharp
public float Tolerance { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

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

### **FABRIKSolver()**

```csharp
public FABRIKSolver()
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
