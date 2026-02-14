# DebugDraw

Namespace: FrinkyEngine.Core.Rendering

Provides on-screen debug text rendering (similar to Unreal's Print String).
 Messages are displayed as an overlay list and automatically expire after a duration.

In the editor, messages render in the viewport overlay. In the runtime build,
 all methods are no-ops unless a backend is registered.

```csharp
public static class DebugDraw
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [DebugDraw](./frinkyengine.core.rendering.debugdraw)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **SetBackend(IDebugDrawBackend)**

Registers the debug draw backend. Called by the editor during initialization.

```csharp
public static void SetBackend(IDebugDrawBackend backend)
```

#### Parameters

`backend` [IDebugDrawBackend](./frinkyengine.core.rendering.debugdraw.idebugdrawbackend)<br>
The backend implementation, or `null` to unregister.

### **PrintString(String, Single, Nullable&lt;Vector4&gt;, String)**

Prints a debug message on screen for the specified duration.

```csharp
public static void PrintString(string message, float duration, Nullable<Vector4> color, string key)
```

#### Parameters

`message` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The text to display.

`duration` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
How long the message is visible in seconds. Default is 5 seconds.

`color` [Nullable&lt;Vector4&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
RGBA color as a [Vector4](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector4) (0-1 per channel). Defaults to green.

`key` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Optional key for replacing messages. If a message with the same key already exists,
 it is replaced instead of creating a new entry. Useful for continuously updating values.

### **Clear()**

Removes all currently displayed debug messages.

```csharp
public static void Clear()
```
