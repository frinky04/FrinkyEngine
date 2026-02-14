# TwoBoneIKSolver

Namespace: FrinkyEngine.Core.Animation.IK

Two-bone (3-joint) IK solver for limb chains such as arms and legs.

```csharp
public class TwoBoneIKSolver : IKSolver
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FObject](./frinkyengine.core.ecs.fobject) → [IKSolver](./frinkyengine.core.animation.ik.iksolver) → [TwoBoneIKSolver](./frinkyengine.core.animation.ik.twoboneiksolver)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **DisplayName**

```csharp
public string DisplayName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **RootBoneIndex**

Root bone index (e.g. upper arm / thigh). Dropdown index where 0 = (none).

```csharp
public int RootBoneIndex { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **MidBoneIndex**

Mid bone index (e.g. forearm / shin). Dropdown index where 0 = (none).

```csharp
public int MidBoneIndex { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **EndBoneIndex**

End bone index (e.g. hand / foot). Dropdown index where 0 = (none).

```csharp
public int EndBoneIndex { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **TargetSpace**

Coordinate space for [TwoBoneIKSolver.TargetPosition](./frinkyengine.core.animation.ik.twoboneiksolver#targetposition).

```csharp
public IKTargetSpace TargetSpace { get; set; }
```

#### Property Value

[IKTargetSpace](./frinkyengine.core.animation.ik.iktargetspace)<br>

### **TargetPosition**

Target position the end effector should reach toward.
 Interpreted according to [TwoBoneIKSolver.TargetSpace](./frinkyengine.core.animation.ik.twoboneiksolver#targetspace).

```csharp
public Vector3 TargetPosition { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **PoleTargetSpace**

Coordinate space for [TwoBoneIKSolver.PoleTargetPosition](./frinkyengine.core.animation.ik.twoboneiksolver#poletargetposition).

```csharp
public IKTargetSpace PoleTargetSpace { get; set; }
```

#### Property Value

[IKTargetSpace](./frinkyengine.core.animation.ik.iktargetspace)<br>

### **PoleTargetPosition**

Pole target that defines the bend plane (e.g. knee/elbow direction).
 Interpreted according to [TwoBoneIKSolver.PoleTargetSpace](./frinkyengine.core.animation.ik.twoboneiksolver#poletargetspace).

```csharp
public Vector3 PoleTargetPosition { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

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

### **TwoBoneIKSolver()**

```csharp
public TwoBoneIKSolver()
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
