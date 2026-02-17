# RootPanel

Namespace: FrinkyEngine.Core.CanvasUI

```csharp
public class RootPanel : Panel
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Panel](./frinkyengine.core.canvasui.panel) → [RootPanel](./frinkyengine.core.canvasui.rootpanel)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Fields

### **ComputedStyle**

```csharp
public ComputedStyle ComputedStyle;
```

## Properties

### **MousePosition**

```csharp
public Vector2 MousePosition { get; }
```

#### Property Value

[Vector2](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector2)<br>

### **Id**

```csharp
public int Id { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Classes**

```csharp
public IReadOnlyCollection<string> Classes { get; }
```

#### Property Value

[IReadOnlyCollection&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlycollection-1)<br>

### **Parent**

```csharp
public Panel Parent { get; internal set; }
```

#### Property Value

[Panel](./frinkyengine.core.canvasui.panel)<br>

### **Children**

```csharp
public IReadOnlyList<Panel> Children { get; }
```

#### Property Value

[IReadOnlyList&lt;Panel&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### **BindingContext**

Effective binding context for this panel. Inherited from the parent
 unless overridden via [Panel.SetBindingContext(Object)](./frinkyengine.core.canvasui.panel#setbindingcontextobject).

```csharp
public object BindingContext { get; }
```

#### Property Value

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

### **Style**

```csharp
public StyleSheet Style { get; }
```

#### Property Value

[StyleSheet](./frinkyengine.core.canvasui.styles.stylesheet)<br>

### **PseudoClasses**

```csharp
public PseudoClassFlags PseudoClasses { get; set; }
```

#### Property Value

[PseudoClassFlags](./frinkyengine.core.canvasui.pseudoclassflags)<br>

### **Box**

```csharp
public Box Box { get; internal set; }
```

#### Property Value

[Box](./frinkyengine.core.canvasui.box)<br>

### **AcceptsFocus**

```csharp
public bool AcceptsFocus { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ScrollOffsetY**

```csharp
public float ScrollOffsetY { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

## Constructors

### **RootPanel()**

```csharp
public RootPanel()
```

## Methods

### **LoadStyleSheet(String)**

```csharp
public void LoadStyleSheet(string css)
```

#### Parameters

`css` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **LoadStyleSheetFromAsset(String)**

```csharp
public bool LoadStyleSheetFromAsset(string assetPath)
```

#### Parameters

`assetPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ClearStyleSheets()**

```csharp
public void ClearStyleSheets()
```

### **Update(Single, Int32, Int32, Nullable&lt;Vector2&gt;)**

```csharp
public void Update(float dt, int screenWidth, int screenHeight, Nullable<Vector2> mouseOverride)
```

#### Parameters

`dt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`screenWidth` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`screenHeight` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`mouseOverride` [Nullable&lt;Vector2&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **ResetInput()**

```csharp
public void ResetInput()
```

### **EnableHotReload(Boolean)**

```csharp
public void EnableHotReload(bool enabled)
```

#### Parameters

`enabled` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **LoadMarkup(String, Object, Boolean)**

```csharp
public Panel LoadMarkup(string markup, object bindingContext, bool clearRoot)
```

#### Parameters

`markup` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`bindingContext` [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

`clearRoot` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[Panel](./frinkyengine.core.canvasui.panel)<br>

### **LoadMarkupFromAsset(String, Object, Boolean)**

```csharp
public Panel LoadMarkupFromAsset(string assetPath, object bindingContext, bool clearRoot)
```

#### Parameters

`assetPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`bindingContext` [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

`clearRoot` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[Panel](./frinkyengine.core.canvasui.panel)<br>

### **Shutdown()**

```csharp
public void Shutdown()
```

## Events

### **OnClick**

```csharp
public event Action<MouseEvent> OnClick;
```

### **OnMouseOver**

```csharp
public event Action<MouseEvent> OnMouseOver;
```

### **OnMouseOut**

```csharp
public event Action<MouseEvent> OnMouseOut;
```

### **OnMouseDown**

```csharp
public event Action<MouseEvent> OnMouseDown;
```

### **OnMouseUp**

```csharp
public event Action<MouseEvent> OnMouseUp;
```

### **OnFocus**

```csharp
public event Action<FocusEvent> OnFocus;
```

### **OnBlur**

```csharp
public event Action<FocusEvent> OnBlur;
```

### **OnKeyDown**

```csharp
public event Action<KeyboardEvent> OnKeyDown;
```

### **OnKeyPress**

```csharp
public event Action<KeyboardEvent> OnKeyPress;
```

### **OnMouseWheel**

```csharp
public event Action<MouseWheelEvent> OnMouseWheel;
```
