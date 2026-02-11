# EngineOverlays

Namespace: FrinkyEngine.Core.UI

Built-in engine overlays (stats overlay, developer console) that render through the Game UI pipeline.

```csharp
public static class EngineOverlays
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [EngineOverlays](./frinkyengine.core.ui.engineoverlays)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **IsConsoleVisible**

Gets whether the developer console overlay is currently visible.

```csharp
public static bool IsConsoleVisible { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Methods

### **Update(Single)**

Checks keybinds and queues overlay draw commands for the current frame.
 Call once per frame after scene update.

```csharp
public static void Update(float dt)
```

#### Parameters

`dt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Frame delta time in seconds.

### **Reset()**

Resets console state (history and input). Called when exiting editor play mode.
 Stats overlay mode intentionally persists.

```csharp
public static void Reset()
```
