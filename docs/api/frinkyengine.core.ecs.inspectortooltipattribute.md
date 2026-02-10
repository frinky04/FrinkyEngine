# InspectorTooltipAttribute

Namespace: FrinkyEngine.Core.ECS

Displays a tooltip when hovering over the property label.

```csharp
public sealed class InspectorTooltipAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorTooltipAttribute](./frinkyengine.core.ecs.inspectortooltipattribute)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **Tooltip**

Tooltip text displayed on hover.

```csharp
public string Tooltip { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **TypeId**

```csharp
public object TypeId { get; }
```

#### Property Value

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

## Constructors

### **InspectorTooltipAttribute(String)**

Displays a tooltip when hovering over the property label.

```csharp
public InspectorTooltipAttribute(string tooltip)
```

#### Parameters

`tooltip` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
