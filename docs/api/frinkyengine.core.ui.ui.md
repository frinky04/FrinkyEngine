# UI

Namespace: FrinkyEngine.Core.UI

Immediate-mode UI entry point for gameplay scripts.

```csharp
public static class UI
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [UI](./frinkyengine.core.ui.ui)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **IsAvailable**

Gets whether the UI backend is currently initialized.

```csharp
public static bool IsAvailable { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **InputCapture**

Gets input capture state from the most recently rendered UI frame.

```csharp
public static UiInputCapture InputCapture { get; }
```

#### Property Value

[UiInputCapture](./frinkyengine.core.ui.uiinputcapture)<br>

## Methods

### **Initialize()**

Initializes the UI system if it is not already initialized.

```csharp
public static void Initialize()
```

### **Shutdown()**

Shuts down the UI system and releases backend resources.

```csharp
public static void Shutdown()
```

### **Draw(Action&lt;UiContext&gt;)**

Queues UI draw commands for the current frame.

```csharp
public static void Draw(Action<UiContext> draw)
```

#### Parameters

`draw` [Action&lt;UiContext&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.action-1)<br>
Draw callback that receives a wrapper [UiContext](./frinkyengine.core.ui.uicontext).

### **BeginFrame(Single, UiFrameDesc&)**

Prepares frame metadata used to render queued UI commands.

```csharp
public static void BeginFrame(float dt, UiFrameDesc& frameDesc)
```

#### Parameters

`dt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Frame delta time in seconds.

`frameDesc` [UiFrameDesc&](./frinkyengine.core.ui.uiframedesc&)<br>
Target viewport and input metadata.

### **EndFrame()**

Renders queued UI commands and clears the frame queue.

```csharp
public static void EndFrame()
```

### **ClearFrame()**

Clears queued UI commands without rendering.

```csharp
public static void ClearFrame()
```
