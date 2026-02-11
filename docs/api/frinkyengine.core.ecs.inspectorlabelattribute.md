# InspectorLabelAttribute

Namespace: FrinkyEngine.Core.ECS

Overrides the property label shown in the inspector.

```csharp
public sealed class InspectorLabelAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorLabelAttribute](./frinkyengine.core.ecs.inspectorlabelattribute)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **Label**

Display label.

```csharp
public string Label { get; }
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

### **InspectorLabelAttribute(String)**

Overrides the property label shown in the inspector.

```csharp
public InspectorLabelAttribute(string label)
```

#### Parameters

`label` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
