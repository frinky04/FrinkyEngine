# MouseEvent

Namespace: FrinkyEngine.Core.CanvasUI.Events

```csharp
public class MouseEvent
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [MouseEvent](./frinkyengine.core.canvasui.events.mouseevent)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **ScreenPos**

```csharp
public Vector2 ScreenPos { get; set; }
```

#### Property Value

[Vector2](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector2)<br>

### **LocalPos**

```csharp
public Vector2 LocalPos { get; set; }
```

#### Property Value

[Vector2](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector2)<br>

### **Button**

```csharp
public MouseButton Button { get; set; }
```

#### Property Value

MouseButton<br>

### **Target**

```csharp
public Panel Target { get; set; }
```

#### Property Value

[Panel](./frinkyengine.core.canvasui.panel)<br>

### **Handled**

```csharp
public bool Handled { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **MouseEvent()**

```csharp
public MouseEvent()
```
