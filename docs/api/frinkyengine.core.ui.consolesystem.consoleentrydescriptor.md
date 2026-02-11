# ConsoleEntryDescriptor

Namespace: FrinkyEngine.Core.UI.ConsoleSystem

Immutable descriptor of one registered console command or cvar entry.

```csharp
public struct ConsoleEntryDescriptor
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [ConsoleEntryDescriptor](./frinkyengine.core.ui.consolesystem.consoleentrydescriptor)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **Kind**

Entry kind.

```csharp
public ConsoleEntryKind Kind { get; }
```

#### Property Value

[ConsoleEntryKind](./frinkyengine.core.ui.consolesystem.consoleentrykind)<br>

### **Name**

Primary entry name.

```csharp
public string Name { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Usage**

Usage/help text.

```csharp
public string Usage { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Description**

Human-readable description.

```csharp
public string Description { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

## Constructors

### **ConsoleEntryDescriptor(ConsoleEntryKind, String, String, String)**

Creates a new console entry descriptor.

```csharp
ConsoleEntryDescriptor(ConsoleEntryKind kind, string name, string usage, string description)
```

#### Parameters

`kind` [ConsoleEntryKind](./frinkyengine.core.ui.consolesystem.consoleentrykind)<br>
Entry kind.

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Primary entry name.

`usage` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Usage/help text.

`description` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Human-readable description.
