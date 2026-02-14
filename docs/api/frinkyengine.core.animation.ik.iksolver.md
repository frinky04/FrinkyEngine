# IKSolver

Namespace: FrinkyEngine.Core.Animation.IK

Abstract base class for IK solvers. Each solver modifies bone-local transforms in place.

```csharp
public abstract class IKSolver : FrinkyEngine.Core.ECS.FObject
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FObject](./frinkyengine.core.ecs.fobject) → [IKSolver](./frinkyengine.core.animation.ik.iksolver)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

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

### **IsConfigured**

Whether the solver has enough configuration data to run.

```csharp
public bool IsConfigured { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **DisplayName**

Human-readable name shown in the editor UI. Defaults to the type name.

```csharp
public string DisplayName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

## Constructors

### **IKSolver()**

```csharp
protected IKSolver()
```

## Methods

### **GetBoneNames()**

Returns bone names for inspector dropdowns (index 0 = "(none)").

```csharp
protected String[] GetBoneNames()
```

#### Returns

[String[]](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **CanSolve(BoneHierarchy)**

Whether the solver can run on the provided hierarchy.

```csharp
public bool CanSolve(BoneHierarchy hierarchy)
```

#### Parameters

`hierarchy` [BoneHierarchy](./frinkyengine.core.animation.ik.bonehierarchy)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Solve(ValueTuple`3[], BoneHierarchy, Matrix4x4, Matrix4x4[])**

Applies this solver to the given local-space bone transforms.

```csharp
public abstract void Solve(ValueTuple`3[] localTransforms, BoneHierarchy hierarchy, Matrix4x4 entityWorldMatrix, Matrix4x4[] worldMatrices)
```

#### Parameters

`localTransforms` [ValueTuple`3[]](https://docs.microsoft.com/en-us/dotnet/api/system.valuetuple-3)<br>
Per-bone local transforms (translation, rotation, scale).

`hierarchy` [BoneHierarchy](./frinkyengine.core.animation.ik.bonehierarchy)<br>
Bone hierarchy data.

`entityWorldMatrix` [Matrix4x4](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4)<br>
The entity's world transform matrix.

`worldMatrices` [Matrix4x4[]](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4)<br>
Pre-computed world-space matrices for all bones (FK result). Solvers may read and mutate this.
