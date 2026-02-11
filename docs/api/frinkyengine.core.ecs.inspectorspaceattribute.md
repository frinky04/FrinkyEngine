# InspectorSpaceAttribute

Namespace: FrinkyEngine.Core.ECS

Inserts vertical spacing before the property.

```csharp
public sealed class InspectorSpaceAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorSpaceAttribute](./frinkyengine.core.ecs.inspectorspaceattribute)<br>
Attributes [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **Height**

Spacing height in pixels.

```csharp
public float Height { get; }
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

### **InspectorSpaceAttribute(Single)**

Inserts vertical spacing before the property.

```csharp
public InspectorSpaceAttribute(float height)
```

#### Parameters

`height` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
