# DebugDraw

Namespace: FrinkyEngine.Core.Rendering

Provides on-screen debug text rendering (similar to Unreal's Print String).
 Messages are displayed as an overlay list and automatically expire after a duration.

```csharp
public static class DebugDraw
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [DebugDraw](./frinkyengine.core.rendering.debugdraw)

## Methods

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
