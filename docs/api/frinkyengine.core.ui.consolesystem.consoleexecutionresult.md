# ConsoleExecutionResult

Namespace: FrinkyEngine.Core.UI.ConsoleSystem

Result of executing a developer-console command or cvar.

```csharp
public struct ConsoleExecutionResult
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [ConsoleExecutionResult](./frinkyengine.core.ui.consolesystem.consoleexecutionresult)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **Success**

Whether execution succeeded.

```csharp
public bool Success { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Lines**

Output lines to print in console history.

```csharp
public IReadOnlyList<string> Lines { get; }
```

#### Property Value

[IReadOnlyList&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

## Constructors

### **ConsoleExecutionResult(Boolean, IReadOnlyList&lt;String&gt;)**

Creates a new execution result.

```csharp
ConsoleExecutionResult(bool success, IReadOnlyList<string> lines)
```

#### Parameters

`success` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
Whether execution succeeded.

`lines` [IReadOnlyList&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>
Output lines to print in console history.
