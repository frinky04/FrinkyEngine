# InspectorButtonAttribute

Namespace: FrinkyEngine.Core.ECS

Renders a method as a clickable button in the inspector.

```csharp
public sealed class InspectorButtonAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorButtonAttribute](./frinkyengine.core.ecs.inspectorbuttonattribute)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **Label**

Button label text.

```csharp
public string Label { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Section**

Optional section heading shown before the button group.

```csharp
public string Section { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Mode**

Controls when this button is visible.

```csharp
public InspectorUiMode Mode { get; set; }
```

#### Property Value

[InspectorUiMode](./frinkyengine.core.ecs.inspectoruimode)<br>

### **DisableWhen**

Optional member name returning bool that disables the button when true.

```csharp
public string DisableWhen { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Order**

Sort key for multiple buttons.

```csharp
public int Order { get; set; }
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

### **InspectorButtonAttribute(String)**

Renders a method as a clickable button in the inspector.

```csharp
public InspectorButtonAttribute(string label)
```

#### Parameters

`label` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
