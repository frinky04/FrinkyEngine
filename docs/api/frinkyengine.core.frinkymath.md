# FrinkyMath

Namespace: FrinkyEngine.Core

Common math constants and conversion utilities used throughout the engine.

```csharp
public static class FrinkyMath
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FrinkyMath](./frinkyengine.core.frinkymath)

## Fields

### **Deg2Rad**

Multiplier to convert degrees to radians.

```csharp
public static float Deg2Rad;
```

### **Rad2Deg**

Multiplier to convert radians to degrees.

```csharp
public static float Rad2Deg;
```

## Methods

### **QuaternionToEuler(Quaternion)**

Converts a quaternion rotation to Euler angles in degrees.

```csharp
public static Vector3 QuaternionToEuler(Quaternion q)
```

#### Parameters

`q` [Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>
The quaternion to convert.

#### Returns

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
A [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3) containing (roll, pitch, yaw) in degrees.

### **EulerToQuaternion(Vector3)**

Converts Euler angles in degrees to a quaternion rotation.

```csharp
public static Quaternion EulerToQuaternion(Vector3 eulerDegrees)
```

#### Parameters

`eulerDegrees` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
A [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3) containing (pitch, yaw, roll) in degrees.

#### Returns

[Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>
The equivalent quaternion rotation.

### **Matrix4x4ToFloatArray(Matrix4x4)**

Converts a [Matrix4x4](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4) to a flat 16-element float array in row-major order.

```csharp
public static Single[] Matrix4x4ToFloatArray(Matrix4x4 m)
```

#### Parameters

`m` [Matrix4x4](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4)<br>
The matrix to convert.

#### Returns

[Single[]](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
A 16-element array containing the matrix values.
