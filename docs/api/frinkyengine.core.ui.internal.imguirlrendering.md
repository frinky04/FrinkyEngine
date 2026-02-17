# ImGuiRlRendering

Namespace: FrinkyEngine.Core.UI.Internal

Shared low-level ImGui rendering helpers for Raylib backends.
 Used by both the runtime [ImGuiUiBackend](./frinkyengine.core.ui.internal.imguiuibackend) and the editor RlImGui.

```csharp
public static class ImGuiRlRendering
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ImGuiRlRendering](./frinkyengine.core.ui.internal.imguirlrendering)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **RenderDrawData(ImDrawDataPtr)**

Renders an ImGui draw data list using Raylib's immediate-mode API.

```csharp
public static void RenderDrawData(ImDrawDataPtr drawData)
```

#### Parameters

`drawData` ImDrawDataPtr<br>

### **ProcessTextures(Dictionary&lt;Int32, Texture2D&gt;)**

Processes the ImGui texture queue, creating/updating/destroying textures as needed.

```csharp
public static void ProcessTextures(Dictionary<int, Texture2D> managedTextures)
```

#### Parameters

`managedTextures` [Dictionary&lt;Int32, Texture2D&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2)<br>

### **CreateTexture(Dictionary&lt;Int32, Texture2D&gt;, ImTextureDataPtr)**

Creates a GPU texture from ImGui texture data.

```csharp
public static void CreateTexture(Dictionary<int, Texture2D> managedTextures, ImTextureDataPtr texData)
```

#### Parameters

`managedTextures` [Dictionary&lt;Int32, Texture2D&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2)<br>

`texData` ImTextureDataPtr<br>

### **DetermineMaskChannel(Byte*, Int32, Int32)**

Determines which channel in a multi-byte pixel format contains the alpha mask.

```csharp
public static int DetermineMaskChannel(Byte* pixels, int pixelCount, int bytesPerPixel)
```

#### Parameters

`pixels` [Byte*](https://docs.microsoft.com/en-us/dotnet/api/system.byte*)<br>

`pixelCount` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`bytesPerPixel` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **EnableScissor(Single, Single, Single, Single)**

Enables the scissor test for a clip rectangle.

```csharp
public static void EnableScissor(float x, float y, float width, float height)
```

#### Parameters

`x` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`y` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`width` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`height` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **ProcessGamepad(ImGuiIOPtr)**

Processes gamepad input for ImGui.

```csharp
public static void ProcessGamepad(ImGuiIOPtr io)
```

#### Parameters

`io` ImGuiIOPtr<br>

### **BuildKeyMap(Dictionary&lt;KeyboardKey, ImGuiKey&gt;)**

Populates a key map dictionary with Raylib-to-ImGui key mappings.

```csharp
public static void BuildKeyMap(Dictionary<KeyboardKey, ImGuiKey> keyMap)
```

#### Parameters

`keyMap` [Dictionary&lt;KeyboardKey, ImGuiKey&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2)<br>

### **BuildCursorMap(Dictionary&lt;ImGuiMouseCursor, MouseCursor&gt;)**

Populates a cursor map dictionary with ImGui-to-Raylib cursor mappings.

```csharp
public static void BuildCursorMap(Dictionary<ImGuiMouseCursor, MouseCursor> cursorMap)
```

#### Parameters

`cursorMap` [Dictionary&lt;ImGuiMouseCursor, MouseCursor&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2)<br>
