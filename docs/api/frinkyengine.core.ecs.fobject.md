# FObject

Namespace: FrinkyEngine.Core.ECS

Base class for polymorphic data objects owned by components.
 Subclass this to create configurable, type-selectable data (AI behaviors, weapon configs, etc.).
 Public read/write properties are auto-serialized and drawn in the inspector.

```csharp
public abstract class FObject
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FObject](./frinkyengine.core.ecs.fobject)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **DisplayName**

Human-readable name shown in the editor UI. Defaults to the type name.

```csharp
public string DisplayName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

## Constructors

### **FObject()**

```csharp
protected FObject()
```
