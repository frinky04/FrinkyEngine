# RenderRuntimeCvars

Namespace: FrinkyEngine.Core.Rendering

Runtime rendering cvars controlled by the developer console.

```csharp
public static class RenderRuntimeCvars
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [RenderRuntimeCvars](./frinkyengine.core.rendering.renderruntimecvars)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **PostProcessingEnabled**

Global post-processing toggle used by standalone runtime and editor Play/Simulate rendering.

```csharp
public static bool PostProcessingEnabled { get; private set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **AmbientOverride**

Optional ambient light override. When non-null, replaces the default 0.15 ambient
 (skylights still take priority when present).

```csharp
public static Nullable<Vector3> AmbientOverride { get; private set; }
```

#### Property Value

[Nullable&lt;Vector3&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **ScreenPercentage**

Screen percentage (10-200). 100 = native resolution. Values below 100 render at lower
 resolution and upscale with nearest-neighbor filtering for a pixelated look.

```csharp
public static int ScreenPercentage { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **TargetFps**

Target FPS value (0 = uncapped). Mirrors the value passed to Raylib.SetTargetFPS.

```csharp
public static int TargetFps { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

## Methods

### **GetPostProcessingValue()**

Gets the post-processing cvar value as "1" (enabled) or "0" (disabled).

```csharp
public static string GetPostProcessingValue()
```

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The current value string.

### **TrySetPostProcessing(String)**

Attempts to parse and apply the post-processing cvar from "1" or "0".

```csharp
public static bool TrySetPostProcessing(string value)
```

#### Parameters

`value` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
User input value.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the value was accepted; otherwise `false`.

### **GetAmbientValue()**

Gets the ambient override as "r g b" or "default" if not set.

```csharp
public static string GetAmbientValue()
```

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The current value string.

### **TrySetAmbient(String)**

Attempts to parse and apply the ambient override from "r g b" (0-1 per channel).
 Pass "default" to clear the override.

```csharp
public static bool TrySetAmbient(string value)
```

#### Parameters

`value` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
User input value.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the value was accepted; otherwise `false`.

### **GetScreenPercentageValue()**

Gets the screen percentage cvar value as a string.

```csharp
public static string GetScreenPercentageValue()
```

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The current value string.

### **TrySetScreenPercentage(String)**

Attempts to parse and apply the screen percentage cvar (10-200).

```csharp
public static bool TrySetScreenPercentage(string value)
```

#### Parameters

`value` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
User input value.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the value was accepted; otherwise `false`.

### **GetScaledDimensions(Int32, Int32)**

Returns scaled dimensions based on the current screen percentage.

```csharp
public static ValueTuple<int, int> GetScaledDimensions(int displayWidth, int displayHeight)
```

#### Parameters

`displayWidth` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
The display width in pixels.

`displayHeight` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
The display height in pixels.

#### Returns

[ValueTuple&lt;Int32, Int32&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.valuetuple-2)<br>
The scaled width and height.

### **GetTargetFpsValue()**

Gets the target FPS cvar value as a string.

```csharp
public static string GetTargetFpsValue()
```

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The current value string.

### **TrySetTargetFps(String)**

Attempts to parse and apply the target FPS cvar (0-500, 0 = uncapped).

```csharp
public static bool TrySetTargetFps(string value)
```

#### Parameters

`value` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
User input value.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the value was accepted; otherwise `false`.
