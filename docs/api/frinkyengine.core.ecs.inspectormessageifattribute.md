# InspectorMessageIfAttribute

Namespace: FrinkyEngine.Core.ECS

Displays a validation/info message when a named bool condition is true.

```csharp
public sealed class InspectorMessageIfAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorMessageIfAttribute](./frinkyengine.core.ecs.inspectormessageifattribute)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **ConditionMember**

Name of a bool property/field/method on the inspected object.

```csharp
public string ConditionMember { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Message**

Message text shown in the inspector.

```csharp
public string Message { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Severity**

Visual severity style.

```csharp
public InspectorMessageSeverity Severity { get; set; }
```

#### Property Value

[InspectorMessageSeverity](./frinkyengine.core.ecs.inspectormessageseverity)<br>

### **Mode**

Controls when this message is visible.

```csharp
public InspectorUiMode Mode { get; set; }
```

#### Property Value

[InspectorUiMode](./frinkyengine.core.ecs.inspectoruimode)<br>

### **Order**

Sort key for multiple messages.

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

### **InspectorMessageIfAttribute(String, String)**

Displays a validation/info message when a named bool condition is true.

```csharp
public InspectorMessageIfAttribute(string conditionMember, string message)
```

#### Parameters

`conditionMember` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`message` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
