# ProgressBar

Namespace: FrinkyEngine.Core.CanvasUI.Panels

```csharp
public class ProgressBar : FrinkyEngine.Core.CanvasUI.Panel
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Panel](./frinkyengine.core.canvasui.panel) → [ProgressBar](./frinkyengine.core.canvasui.panels.progressbar)

## Fields

### **ComputedStyle**

```csharp
public ComputedStyle ComputedStyle;
```

## Properties

### **Value**

```csharp
public float Value { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **TrackColor**

```csharp
public Nullable<Color> TrackColor { get; set; }
```

#### Property Value

[Nullable&lt;Color&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **FillColor**

```csharp
public Nullable<Color> FillColor { get; set; }
```

#### Property Value

[Nullable&lt;Color&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

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

### **ProgressBar()**

```csharp
public ProgressBar()
```

## Methods

### **RenderContent(Box, ComputedStyle, Byte)**

```csharp
public void RenderContent(Box box, ComputedStyle style, byte alpha)
```

#### Parameters

`box` [Box](./frinkyengine.core.canvasui.box)<br>

`style` [ComputedStyle](./frinkyengine.core.canvasui.styles.computedstyle)<br>

`alpha` [Byte](https://docs.microsoft.com/en-us/dotnet/api/system.byte)<br>

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
