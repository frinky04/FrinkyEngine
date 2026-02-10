# PhysicsFrameStats

Namespace: FrinkyEngine.Core.Physics

Per-frame physics diagnostics snapshot.

```csharp
public struct PhysicsFrameStats
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [PhysicsFrameStats](./frinkyengine.core.physics.physicsframestats)<br>
Implements [IEquatable&lt;PhysicsFrameStats&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.iequatable-1)<br>
Attributes [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **Valid**

```csharp
public bool Valid { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **DynamicBodies**

```csharp
public int DynamicBodies { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **KinematicBodies**

```csharp
public int KinematicBodies { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **StaticBodies**

```csharp
public int StaticBodies { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **SubstepsThisFrame**

```csharp
public int SubstepsThisFrame { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **StepTimeMs**

```csharp
public double StepTimeMs { get; set; }
```

#### Property Value

[Double](https://docs.microsoft.com/en-us/dotnet/api/system.double)<br>

### **ActiveCharacterControllers**

```csharp
public int ActiveCharacterControllers { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

## Constructors

### **PhysicsFrameStats(Boolean, Int32, Int32, Int32, Int32, Double, Int32)**

Per-frame physics diagnostics snapshot.

```csharp
PhysicsFrameStats(bool Valid, int DynamicBodies, int KinematicBodies, int StaticBodies, int SubstepsThisFrame, double StepTimeMs, int ActiveCharacterControllers)
```

#### Parameters

`Valid` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

`DynamicBodies` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`KinematicBodies` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`StaticBodies` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`SubstepsThisFrame` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`StepTimeMs` [Double](https://docs.microsoft.com/en-us/dotnet/api/system.double)<br>

`ActiveCharacterControllers` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

## Methods

### **ToString()**

```csharp
string ToString()
```

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GetHashCode()**

```csharp
int GetHashCode()
```

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Equals(Object)**

```csharp
bool Equals(object obj)
```

#### Parameters

`obj` [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Equals(PhysicsFrameStats)**

```csharp
bool Equals(PhysicsFrameStats other)
```

#### Parameters

`other` [PhysicsFrameStats](./frinkyengine.core.physics.physicsframestats)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Deconstruct(Boolean&, Int32&, Int32&, Int32&, Int32&, Double&, Int32&)**

```csharp
void Deconstruct(Boolean& Valid, Int32& DynamicBodies, Int32& KinematicBodies, Int32& StaticBodies, Int32& SubstepsThisFrame, Double& StepTimeMs, Int32& ActiveCharacterControllers)
```

#### Parameters

`Valid` [Boolean&](https://docs.microsoft.com/en-us/dotnet/api/system.boolean&)<br>

`DynamicBodies` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>

`KinematicBodies` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>

`StaticBodies` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>

`SubstepsThisFrame` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>

`StepTimeMs` [Double&](https://docs.microsoft.com/en-us/dotnet/api/system.double&)<br>

`ActiveCharacterControllers` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>
