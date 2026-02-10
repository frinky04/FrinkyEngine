# UiFrameDesc

Namespace: FrinkyEngine.Core.UI

Describes the target viewport and input behavior for a UI frame.

```csharp
public struct UiFrameDesc
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [UiFrameDesc](./frinkyengine.core.ui.uiframedesc)<br>
Implements [IEquatable&lt;UiFrameDesc&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.iequatable-1)<br>
Attributes [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **Width**

Viewport width in pixels.

```csharp
public int Width { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Height**

Viewport height in pixels.

```csharp
public int Height { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **IsFocused**

When `true`, keyboard focus is considered active.

```csharp
public bool IsFocused { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **IsHovered**

When `true`, mouse input is considered active for the UI viewport.

```csharp
public bool IsHovered { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **UseMousePositionOverride**

When `true`, [UiFrameDesc.MousePosition](./frinkyengine.core.ui.uiframedesc#mouseposition) is used instead of screen mouse coordinates.

```csharp
public bool UseMousePositionOverride { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **MousePosition**

Mouse position in viewport-local coordinates when override is enabled.

```csharp
public Vector2 MousePosition { get; set; }
```

#### Property Value

[Vector2](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector2)<br>

### **UseMouseWheelOverride**

When `true`, [UiFrameDesc.MouseWheel](./frinkyengine.core.ui.uiframedesc#mousewheel) is used instead of runtime mouse wheel values.

```csharp
public bool UseMouseWheelOverride { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **MouseWheel**

Mouse wheel delta to use when override is enabled.

```csharp
public Vector2 MouseWheel { get; set; }
```

#### Property Value

[Vector2](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector2)<br>

### **AllowCursorChanges**

When `true`, UI may change the OS cursor shape/visibility.

```csharp
public bool AllowCursorChanges { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **AllowSetMousePos**

When `true`, UI may reposition the OS cursor if requested.

```csharp
public bool AllowSetMousePos { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **AllowKeyboardInput**

When `true`, keyboard and text events are forwarded to UI.

```csharp
public bool AllowKeyboardInput { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ClampedWidth**

Gets the width clamped to a minimum of 1.

```csharp
public int ClampedWidth { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **ClampedHeight**

Gets the height clamped to a minimum of 1.

```csharp
public int ClampedHeight { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

## Constructors

### **UiFrameDesc(Int32, Int32, Boolean, Boolean, Boolean, Vector2, Boolean, Vector2, Boolean, Boolean, Boolean)**

Describes the target viewport and input behavior for a UI frame.

```csharp
UiFrameDesc(int Width, int Height, bool IsFocused, bool IsHovered, bool UseMousePositionOverride, Vector2 MousePosition, bool UseMouseWheelOverride, Vector2 MouseWheel, bool AllowCursorChanges, bool AllowSetMousePos, bool AllowKeyboardInput)
```

#### Parameters

`Width` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Viewport width in pixels.

`Height` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Viewport height in pixels.

`IsFocused` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, keyboard focus is considered active.

`IsHovered` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, mouse input is considered active for the UI viewport.

`UseMousePositionOverride` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, [UiFrameDesc.MousePosition](./frinkyengine.core.ui.uiframedesc#mouseposition) is used instead of screen mouse coordinates.

`MousePosition` [Vector2](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector2)<br>
Mouse position in viewport-local coordinates when override is enabled.

`UseMouseWheelOverride` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, [UiFrameDesc.MouseWheel](./frinkyengine.core.ui.uiframedesc#mousewheel) is used instead of runtime mouse wheel values.

`MouseWheel` [Vector2](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector2)<br>
Mouse wheel delta to use when override is enabled.

`AllowCursorChanges` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, UI may change the OS cursor shape/visibility.

`AllowSetMousePos` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, UI may reposition the OS cursor if requested.

`AllowKeyboardInput` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, keyboard and text events are forwarded to UI.

## Methods

### **ToString()**

```csharp
string ToString()
```

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GetHashCode()**

```csharp
int GetHashCode()
```

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Equals(Object)**

```csharp
bool Equals(object obj)
```

#### Parameters

`obj` [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Equals(UiFrameDesc)**

```csharp
bool Equals(UiFrameDesc other)
```

#### Parameters

`other` [UiFrameDesc](./frinkyengine.core.ui.uiframedesc)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Deconstruct(Int32&, Int32&, Boolean&, Boolean&, Boolean&, Vector2&, Boolean&, Vector2&, Boolean&, Boolean&, Boolean&)**

```csharp
void Deconstruct(Int32& Width, Int32& Height, Boolean& IsFocused, Boolean& IsHovered, Boolean& UseMousePositionOverride, Vector2& MousePosition, Boolean& UseMouseWheelOverride, Vector2& MouseWheel, Boolean& AllowCursorChanges, Boolean& AllowSetMousePos, Boolean& AllowKeyboardInput)
```

#### Parameters

`Width` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>

`Height` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>

`IsFocused` [Boolean&](https://docs.microsoft.com/en-us/dotnet/api/system.boolean&)<br>

`IsHovered` [Boolean&](https://docs.microsoft.com/en-us/dotnet/api/system.boolean&)<br>

`UseMousePositionOverride` [Boolean&](https://docs.microsoft.com/en-us/dotnet/api/system.boolean&)<br>

`MousePosition` [Vector2&](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector2&)<br>

`UseMouseWheelOverride` [Boolean&](https://docs.microsoft.com/en-us/dotnet/api/system.boolean&)<br>

`MouseWheel` [Vector2&](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector2&)<br>

`AllowCursorChanges` [Boolean&](https://docs.microsoft.com/en-us/dotnet/api/system.boolean&)<br>

`AllowSetMousePos` [Boolean&](https://docs.microsoft.com/en-us/dotnet/api/system.boolean&)<br>

`AllowKeyboardInput` [Boolean&](https://docs.microsoft.com/en-us/dotnet/api/system.boolean&)<br>
