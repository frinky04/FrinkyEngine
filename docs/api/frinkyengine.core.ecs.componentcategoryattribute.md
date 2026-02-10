# ComponentCategoryAttribute

Namespace: FrinkyEngine.Core.ECS

Declares the category path for a component in the Add Component menu.
 Supports slash-separated nesting (e.g. "Physics/Colliders").

```csharp
public class ComponentCategoryAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **Category**

The category path.

```csharp
public string Category { get; }
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

### **ComponentCategoryAttribute(String)**

Declares the category path for a component in the Add Component menu.
 Supports slash-separated nesting (e.g. "Physics/Colliders").

```csharp
public ComponentCategoryAttribute(string category)
```

#### Parameters

`category` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
