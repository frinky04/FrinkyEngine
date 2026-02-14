# IKMath

Namespace: FrinkyEngine.Core.Animation.IK

Pure math utilities for inverse kinematics — no ECS or Raylib model dependencies.

```csharp
public static class IKMath
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [IKMath](./frinkyengine.core.animation.ik.ikmath)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **ComputeTwoBoneMidPosition(Vector3, Single, Single, Vector3, Vector3)**

Computes the desired mid-joint (elbow/knee) world position for a two-bone IK chain.
 Uses law of cosines to determine the triangle, and the pole target to orient the bend plane.
 Returns null if the chain is degenerate (zero-length bones or zero distance to target).

```csharp
public static Nullable<Vector3> ComputeTwoBoneMidPosition(Vector3 rootPos, float upperLen, float lowerLen, Vector3 targetPos, Vector3 poleTarget)
```

#### Parameters

`rootPos` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

`upperLen` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`lowerLen` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`targetPos` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

`poleTarget` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

#### Returns

[Nullable&lt;Vector3&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **ForwardKinematics(Int32[], ValueTuple`3[], Matrix4x4, Matrix4x4[])**

Computes world-space matrices for all bones via forward kinematics.

```csharp
public static void ForwardKinematics(Int32[] parentIndices, ValueTuple`3[] localTransforms, Matrix4x4 rootMatrix, Matrix4x4[] worldMatrices)
```

#### Parameters

`parentIndices` [Int32[]](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`localTransforms` [ValueTuple`3[]](https://docs.microsoft.com/en-us/dotnet/api/system.valuetuple-3)<br>

`rootMatrix` [Matrix4x4](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4)<br>

`worldMatrices` [Matrix4x4[]](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4)<br>

### **ExtractRotation(Matrix4x4)**

Extracts the rotation as a quaternion from a 4x4 matrix.
 Assumes the matrix is a valid TRS matrix.

```csharp
public static Quaternion ExtractRotation(Matrix4x4 m)
```

#### Parameters

`m` [Matrix4x4](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4)<br>

#### Returns

[Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>

### **WorldToLocalRotation(Quaternion, Matrix4x4)**

Converts a world-space rotation to local space given the parent's world matrix.

```csharp
public static Quaternion WorldToLocalRotation(Quaternion worldRot, Matrix4x4 parentWorldMatrix)
```

#### Parameters

`worldRot` [Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>

`parentWorldMatrix` [Matrix4x4](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4)<br>

#### Returns

[Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>

### **RotationBetween(Vector3, Vector3)**

Computes the shortest rotation quaternion that rotates vector `from` to `to`.

```csharp
public static Quaternion RotationBetween(Vector3 from, Vector3 to)
```

#### Parameters

`from` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

`to` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

#### Returns

[Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>

### **ApplyWorldRotationDelta(ValueTuple`3[], Matrix4x4[], BoneHierarchy, Int32, Quaternion)**

Applies a world-space rotation delta to a bone, converting back to local space.
 Updates the local transform in place.

```csharp
public static void ApplyWorldRotationDelta(ValueTuple`3[] localTransforms, Matrix4x4[] worldMatrices, BoneHierarchy hierarchy, int boneIndex, Quaternion worldDelta)
```

#### Parameters

`localTransforms` [ValueTuple`3[]](https://docs.microsoft.com/en-us/dotnet/api/system.valuetuple-3)<br>

`worldMatrices` [Matrix4x4[]](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4)<br>

`hierarchy` [BoneHierarchy](./frinkyengine.core.animation.ik.bonehierarchy)<br>

`boneIndex` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`worldDelta` [Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>
