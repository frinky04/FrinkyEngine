# Box

Namespace: FrinkyEngine.Core.CanvasUI

```csharp
public struct Box
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Box](./frinkyengine.core.canvasui.box)

## Fields

### **X**

```csharp
public float X;
```

### **Y**

```csharp
public float Y;
```

### **Width**

```csharp
public float Width;
```

### **Height**

```csharp
public float Height;
```

## Properties

### **Right**

```csharp
public float Right { get; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Bottom**

```csharp
public float Bottom { get; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

## Constructors

### **Box(Single, Single, Single, Single)**

```csharp
Box(float x, float y, float width, float height)
```

#### Parameters

`x` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`y` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`width` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`height` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

## Methods

### **Contains(Single, Single)**

```csharp
bool Contains(float px, float py)
```

#### Parameters

`px` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`py` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Intersect(Box, Box)**

```csharp
Box Intersect(Box a, Box b)
```

#### Parameters

`a` [Box](./frinkyengine.core.canvasui.box)<br>

`b` [Box](./frinkyengine.core.canvasui.box)<br>

#### Returns

[Box](./frinkyengine.core.canvasui.box)<br>
