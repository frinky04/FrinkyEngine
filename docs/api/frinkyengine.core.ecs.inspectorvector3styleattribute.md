# InspectorVector3StyleAttribute

Namespace: FrinkyEngine.Core.ECS

Configures how a [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3) property is drawn.

```csharp
public sealed class InspectorVector3StyleAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorVector3StyleAttribute](./frinkyengine.core.ecs.inspectorvector3styleattribute)<br>
Attributes [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **Style**

Rendering style.

```csharp
public InspectorVector3Style Style { get; }
```

#### Property Value

[InspectorVector3Style](./frinkyengine.core.ecs.inspectorvector3style)<br>

### **ResetX**

Reset value for the X axis when using reset-style controls.

```csharp
public float ResetX { get; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **ResetY**

Reset value for the Y axis when using reset-style controls.

```csharp
public float ResetY { get; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **ResetZ**

Reset value for the Z axis when using reset-style controls.

```csharp
public float ResetZ { get; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **TypeId**

```csharp
public object TypeId { get; }
```

#### Property Value

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

## Constructors

### **InspectorVector3StyleAttribute(InspectorVector3Style, Single, Single, Single)**

Configures how a [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3) property is drawn.

```csharp
public InspectorVector3StyleAttribute(InspectorVector3Style style, float resetX, float resetY, float resetZ)
```

#### Parameters

`style` [InspectorVector3Style](./frinkyengine.core.ecs.inspectorvector3style)<br>

`resetX` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`resetY` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`resetZ` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
