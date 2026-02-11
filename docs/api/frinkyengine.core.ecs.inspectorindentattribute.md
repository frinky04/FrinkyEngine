# InspectorIndentAttribute

Namespace: FrinkyEngine.Core.ECS

Indents the property in the inspector.

```csharp
public sealed class InspectorIndentAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorIndentAttribute](./frinkyengine.core.ecs.inspectorindentattribute)<br>
Attributes [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **Levels**

Number of indentation levels.

```csharp
public int Levels { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **TypeId**

```csharp
public object TypeId { get; }
```

#### Property Value

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

## Constructors

### **InspectorIndentAttribute(Int32)**

Indents the property in the inspector.

```csharp
public InspectorIndentAttribute(int levels)
```

#### Parameters

`levels` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
