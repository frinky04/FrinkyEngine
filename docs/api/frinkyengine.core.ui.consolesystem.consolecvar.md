# ConsoleCVar

Namespace: FrinkyEngine.Core.UI.ConsoleSystem

Describes a console variable (cvar) that can be queried or set from the developer console.

```csharp
public sealed class ConsoleCVar
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ConsoleCVar](./frinkyengine.core.ui.consolesystem.consolecvar)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Name**

Unique cvar name.

```csharp
public string Name { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Usage**

Usage string shown in help and validation errors.

```csharp
public string Usage { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Description**

Human-readable cvar description.

```csharp
public string Description { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

## Constructors

### **ConsoleCVar(String, String, String, Func&lt;String&gt;, Func&lt;String, Boolean&gt;)**

Creates a new cvar definition.

```csharp
public ConsoleCVar(string name, string usage, string description, Func<string> getter, Func<string, bool> setter)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Unique cvar name.

`usage` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Usage string shown in help and validation errors.

`description` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Human-readable cvar description.

`getter` [Func&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.func-1)<br>
Callback that returns the current cvar value as text.

`setter` [Func&lt;String, Boolean&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.func-2)<br>
Callback that applies a user-provided value. Returns `true` on success.

## Methods

### **GetValue()**

Gets the current cvar value as text.

```csharp
public string GetValue()
```

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The current value.

### **TrySetValue(String)**

Attempts to set the cvar from user input.

```csharp
public bool TrySetValue(string value)
```

#### Parameters

`value` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Value text from the console input.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the value was accepted; otherwise `false`.
