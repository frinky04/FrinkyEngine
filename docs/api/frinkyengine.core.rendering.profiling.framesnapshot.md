# FrameSnapshot

Namespace: FrinkyEngine.Core.Rendering.Profiling

Immutable snapshot of one frame's profiling data.

```csharp
public struct FrameSnapshot
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [FrameSnapshot](./frinkyengine.core.rendering.profiling.framesnapshot)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **TotalFrameMs**

```csharp
public double TotalFrameMs { get; }
```

#### Property Value

[Double](https://docs.microsoft.com/en-us/dotnet/api/system.double)<br>

### **OtherMs**

```csharp
public double OtherMs { get; }
```

#### Property Value

[Double](https://docs.microsoft.com/en-us/dotnet/api/system.double)<br>

### **SubTimings**

```csharp
public SubCategoryTiming[] SubTimings { get; }
```

#### Property Value

[SubCategoryTiming[]](./frinkyengine.core.rendering.profiling.subcategorytiming)<br>

### **GpuStats**

```csharp
public GpuFrameStats GpuStats { get; }
```

#### Property Value

[GpuFrameStats](./frinkyengine.core.rendering.profiling.gpuframestats)<br>

## Methods

### **GetCategoryMs(ProfileCategory)**

```csharp
double GetCategoryMs(ProfileCategory cat)
```

#### Parameters

`cat` [ProfileCategory](./frinkyengine.core.rendering.profiling.profilecategory)<br>

#### Returns

[Double](https://docs.microsoft.com/en-us/dotnet/api/system.double)<br>
