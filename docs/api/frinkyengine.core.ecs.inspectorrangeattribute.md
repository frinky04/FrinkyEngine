# InspectorRangeAttribute

Namespace: FrinkyEngine.Core.ECS

Specifies min/max bounds and optional drag speed for numeric properties.

```csharp
public sealed class InspectorRangeAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorRangeAttribute](./frinkyengine.core.ecs.inspectorrangeattribute)<br>
Attributes [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **Min**

Minimum allowed value.

```csharp
public float Min { get; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Max**

Maximum allowed value.

```csharp
public float Max { get; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Speed**

Drag speed multiplier for the inspector control.

```csharp
public float Speed { get; }
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

### **InspectorRangeAttribute(Single, Single, Single)**

Specifies min/max bounds and optional drag speed for numeric properties.

```csharp
public InspectorRangeAttribute(float min, float max, float speed)
```

#### Parameters

`min` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`max` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`speed` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
