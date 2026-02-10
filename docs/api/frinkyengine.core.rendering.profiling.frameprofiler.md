# FrameProfiler

Namespace: FrinkyEngine.Core.Rendering.Profiling

Central frame profiler that collects per-category timing via [Stopwatch](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch)-based scopes
 and stores a rolling history of [FrameSnapshot](./frinkyengine.core.rendering.profiling.framesnapshot) instances.
 Scopes are exclusive: entering a child scope pauses the parent so categories never double-count.

```csharp
public static class FrameProfiler
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FrameProfiler](./frinkyengine.core.rendering.profiling.frameprofiler)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Fields

### **HistorySize**

```csharp
public static int HistorySize;
```

## Properties

### **Enabled**

Enables or disables profiling. When disabled, [FrameProfiler.Scope(ProfileCategory)](./frinkyengine.core.rendering.profiling.frameprofiler#scopeprofilecategory) returns a zero-cost no-op.

```csharp
public static bool Enabled { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **GpuVendor**

GPU vendor string (e.g. "NVIDIA Corporation"). Queried once via OpenGL.

```csharp
public static string GpuVendor { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GpuRenderer**

GPU renderer string (e.g. "NVIDIA GeForce RTX 3080"). Queried once via OpenGL.

```csharp
public static string GpuRenderer { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **FrameCount**

Total number of frames collected.

```csharp
public static int FrameCount { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

## Methods

### **BeginFrame()**

Resets all category timers and starts the frame timer. Call at the start of each frame.

```csharp
public static void BeginFrame()
```

### **EndFrame()**

Stops the frame timer, computes the snapshot, and pushes it into the ring buffer.
 Call at the end of each frame.

```csharp
public static void EndFrame()
```

### **BeginIdle()**

Starts measuring idle time (GPU sync + frame limiter). Call immediately before EndDrawing.

```csharp
public static void BeginIdle()
```

### **EndIdle()**

Stops measuring idle time. Call immediately after EndDrawing.
 Stores the result in the parallel idle history at the most recent snapshot index.

```csharp
public static void EndIdle()
```

### **GetLatestIdleMs()**

Returns the idle time in milliseconds for the most recent frame.

```csharp
public static double GetLatestIdleMs()
```

#### Returns

[Double](https://docs.microsoft.com/en-us/dotnet/api/system.double)<br>

### **GetOrderedIdleHistory(Double[])**

Fills the buffer with idle times ordered oldest-first, matching [FrameProfiler.GetHistory()](./frinkyengine.core.rendering.profiling.frameprofiler#gethistory) ordering.
 The buffer must be at least as large as the current history count.
 Returns the number of entries written.

```csharp
public static int GetOrderedIdleHistory(Double[] buffer)
```

#### Parameters

`buffer` [Double[]](https://docs.microsoft.com/en-us/dotnet/api/system.double)<br>

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Scope(ProfileCategory)**

Returns a [ProfileScope](./frinkyengine.core.rendering.profiling.profilescope) that accumulates time into the given category.
 Entering a scope pauses any currently active scope; exiting resumes it.
 When profiling is disabled, returns a no-op scope.

```csharp
public static ProfileScope Scope(ProfileCategory category)
```

#### Parameters

`category` [ProfileCategory](./frinkyengine.core.rendering.profiling.profilecategory)<br>

#### Returns

[ProfileScope](./frinkyengine.core.rendering.profiling.profilescope)<br>

### **ScopeNamed(ProfileCategory, String)**

Returns a [ProfileScope](./frinkyengine.core.rendering.profiling.profilescope) that accumulates time into both the parent category
 and a named sub-timing entry (e.g. per post-process effect).

```csharp
public static ProfileScope ScopeNamed(ProfileCategory parent, string name)
```

#### Parameters

`parent` [ProfileCategory](./frinkyengine.core.rendering.profiling.profilecategory)<br>

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[ProfileScope](./frinkyengine.core.rendering.profiling.profilescope)<br>

### **ReportGpuStats(GpuFrameStats)**

Reports GPU-related stats for the current frame.

```csharp
public static void ReportGpuStats(GpuFrameStats stats)
```

#### Parameters

`stats` [GpuFrameStats](./frinkyengine.core.rendering.profiling.gpuframestats)<br>

### **GetHistory()**

Returns the history buffer as a span, oldest first.

```csharp
public static ReadOnlySpan<FrameSnapshot> GetHistory()
```

#### Returns

[ReadOnlySpan&lt;FrameSnapshot&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.readonlyspan-1)<br>

### **GetLatest()**

Returns the most recent frame snapshot.

```csharp
public static FrameSnapshot GetLatest()
```

#### Returns

[FrameSnapshot](./frinkyengine.core.rendering.profiling.framesnapshot)<br>
