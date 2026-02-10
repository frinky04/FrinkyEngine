# GpuFrameStats

Namespace: FrinkyEngine.Core.Rendering.Profiling

GPU-related metrics captured once per frame.

```csharp
public struct GpuFrameStats
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [GpuFrameStats](./frinkyengine.core.rendering.profiling.gpuframestats)<br>
Attributes [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **DrawCalls**

```csharp
public int DrawCalls { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **PostProcessPasses**

```csharp
public int PostProcessPasses { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **RtMemoryBytes**

```csharp
public long RtMemoryBytes { get; }
```

#### Property Value

[Int64](https://docs.microsoft.com/en-us/dotnet/api/system.int64)<br>

## Constructors

### **GpuFrameStats(Int32, Int32, Int64)**

GPU-related metrics captured once per frame.

```csharp
GpuFrameStats(int drawCalls, int postProcessPasses, long rtMemoryBytes)
```

#### Parameters

`drawCalls` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`postProcessPasses` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`rtMemoryBytes` [Int64](https://docs.microsoft.com/en-us/dotnet/api/system.int64)<br>
