# UiPanelOptions

Namespace: FrinkyEngine.Core.UI

Configuration options for [UiContext.Panel(String, UiPanelOptions)](./frinkyengine.core.ui.uicontext#panelstring-uipaneloptions).

```csharp
public struct UiPanelOptions
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [UiPanelOptions](./frinkyengine.core.ui.uipaneloptions)<br>
Attributes [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **Position**

Optional panel position in pixels.

```csharp
public Nullable<Vector2> Position { get; set; }
```

#### Property Value

[Nullable&lt;Vector2&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **Size**

Optional panel size in pixels.

```csharp
public Nullable<Vector2> Size { get; set; }
```

#### Property Value

[Nullable&lt;Vector2&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **HasTitleBar**

Whether to show a title bar. Defaults to `false`.

```csharp
public bool HasTitleBar { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Movable**

Whether the panel can be moved. Defaults to `false`.

```csharp
public bool Movable { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Resizable**

Whether the panel can be resized. Defaults to `false`.

```csharp
public bool Resizable { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **NoBackground**

When `true`, panel background is not rendered.

```csharp
public bool NoBackground { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **AutoResize**

Whether panel should auto-resize to fit contents. Defaults to `false`.

```csharp
public bool AutoResize { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Scrollbar**

Whether to show a scrollbar when content overflows. Defaults to `false`.

```csharp
public bool Scrollbar { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
