# ComponentDisplayNameAttribute

Namespace: FrinkyEngine.Core.ECS

Overrides the auto-generated display name for a component in the editor UI.

```csharp
public class ComponentDisplayNameAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [ComponentDisplayNameAttribute](./frinkyengine.core.ecs.componentdisplaynameattribute)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **DisplayName**

The display name shown in the editor.

```csharp
public string DisplayName { get; }
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

### **ComponentDisplayNameAttribute(String)**

Overrides the auto-generated display name for a component in the editor UI.

```csharp
public ComponentDisplayNameAttribute(string name)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
