# UiContext

Namespace: FrinkyEngine.Core.UI

Wrapper API for immediate-mode UI drawing. Game code should use this type instead of raw ImGui.

```csharp
public sealed class UiContext
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [UiContext](./frinkyengine.core.ui.uicontext)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **Panel(String, UiPanelOptions)**

Begins a panel scope.

```csharp
public UiScope Panel(string id, UiPanelOptions options)
```

#### Parameters

`id` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Unique panel identifier.

`options` [UiPanelOptions](./frinkyengine.core.ui.uipaneloptions)<br>
Panel layout and behavior options.

#### Returns

[UiScope](./frinkyengine.core.ui.uiscope)<br>
A disposable scope that ends the panel.

### **Horizontal(String, Int32)**

Begins a horizontal layout with the specified column count.

```csharp
public UiScope Horizontal(string id, int columns)
```

#### Parameters

`id` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Unique layout identifier.

`columns` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Number of columns.

#### Returns

[UiScope](./frinkyengine.core.ui.uiscope)<br>
A disposable scope that ends the horizontal layout.

### **Vertical(String)**

Begins a vertical grouping scope.

```csharp
public UiScope Vertical(string id)
```

#### Parameters

`id` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Unique group identifier.

#### Returns

[UiScope](./frinkyengine.core.ui.uiscope)<br>
A disposable scope that ends the group.

### **NextCell()**

Advances to the next cell in a horizontal table layout.

```csharp
public void NextCell()
```

### **Spacer(Single, Single)**

Emits an empty spacer region.

```csharp
public void Spacer(float width, float height)
```

#### Parameters

`width` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Spacer width in pixels.

`height` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Spacer height in pixels.

### **SameLine(Single, Single)**

Places the next widget on the same line.

```csharp
public void SameLine(float offsetFromStartX, float spacing)
```

#### Parameters

`offsetFromStartX` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Optional absolute offset from line start.

`spacing` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Optional spacing override.

### **Separator()**

Draws a separator line.

```csharp
public void Separator()
```

### **Text(String, Single, UiStyle)**

Draws plain text.

```csharp
public void Text(string text, float fontPx, UiStyle style)
```

#### Parameters

`text` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Text content.

`fontPx` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Optional font size in pixels. Values less than or equal to zero use current style defaults.

`style` [UiStyle](./frinkyengine.core.ui.uistyle)<br>
Optional style overrides.

### **Button(String, String, Single, Nullable&lt;Vector2&gt;, Boolean)**

Draws a clickable button.

```csharp
public bool Button(string id, string label, float fontPx, Nullable<Vector2> size, bool disabled)
```

#### Parameters

`id` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Stable widget identifier.

`label` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Visible label.

`fontPx` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Optional font size in pixels.

`size` [Nullable&lt;Vector2&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional button size.

`disabled` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, button is disabled.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` when the button is clicked this frame.

### **Checkbox(String, String, Boolean&, Single, Boolean)**

Draws a checkbox and edits the provided value.

```csharp
public bool Checkbox(string id, string label, Boolean& value, float fontPx, bool disabled)
```

#### Parameters

`id` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Stable widget identifier.

`label` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Visible label.

`value` [Boolean&](https://docs.microsoft.com/en-us/dotnet/api/system.boolean&)<br>
Value to edit.

`fontPx` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Optional font size in pixels.

`disabled` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, checkbox is disabled.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` when the value changed this frame.

### **SliderFloat(String, String, Single&, Single, Single, Single, Boolean)**

Draws a float slider and edits the provided value.

```csharp
public bool SliderFloat(string id, string label, Single& value, float min, float max, float fontPx, bool disabled)
```

#### Parameters

`id` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Stable widget identifier.

`label` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Visible label.

`value` [Single&](https://docs.microsoft.com/en-us/dotnet/api/system.single&)<br>
Value to edit.

`min` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Minimum value.

`max` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Maximum value.

`fontPx` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Optional font size in pixels.

`disabled` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, slider is disabled.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` when the value changed this frame.

### **ProgressBar(String, Single, Nullable&lt;Vector2&gt;, String)**

Draws a progress bar.

```csharp
public void ProgressBar(string id, float value01, Nullable<Vector2> size, string overlayText)
```

#### Parameters

`id` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Stable widget identifier.

`value01` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Progress value in [0, 1].

`size` [Nullable&lt;Vector2&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Optional size override.

`overlayText` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Optional overlay text.

### **Image(UiImageHandle, Vector2, Boolean)**

Draws an image.

```csharp
public void Image(UiImageHandle image, Vector2 size, bool flipY)
```

#### Parameters

`image` [UiImageHandle](./frinkyengine.core.ui.uiimagehandle)<br>
Image handle to draw.

`size` [Vector2](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector2)<br>
Image size in pixels.

`flipY` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, flips image vertically (useful for render textures).
