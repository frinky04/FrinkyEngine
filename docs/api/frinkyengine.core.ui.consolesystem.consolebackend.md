# ConsoleBackend

Namespace: FrinkyEngine.Core.UI.ConsoleSystem

Central backend for developer-console commands and cvars.

```csharp
public static class ConsoleBackend
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ConsoleBackend](./frinkyengine.core.ui.consolesystem.consolebackend)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **EnsureBuiltinsRegistered()**

Registers built-in commands. Safe to call repeatedly.

```csharp
public static void EnsureBuiltinsRegistered()
```

### **RegisterCommand(String, String, String, Func&lt;IReadOnlyList&lt;String&gt;, ConsoleExecutionResult&gt;)**

Registers a command handler.

```csharp
public static bool RegisterCommand(string name, string usage, string description, Func<IReadOnlyList<string>, ConsoleExecutionResult> handler)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Unique command name.

`usage` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Usage string shown in help text.

`description` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Human-readable command description.

`handler` [Func&lt;IReadOnlyList&lt;String&gt;, ConsoleExecutionResult&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.func-2)<br>
Command callback that receives parsed arguments.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if registered; `false` if the name already exists.

### **RegisterCVar(ConsoleCVar)**

Registers a cvar definition.

```csharp
public static bool RegisterCVar(ConsoleCVar cvar)
```

#### Parameters

`cvar` [ConsoleCVar](./frinkyengine.core.ui.consolesystem.consolecvar)<br>
CVar to register.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if registered; `false` if the name already exists.

### **Execute(String)**

Executes one line of console input.

```csharp
public static ConsoleExecutionResult Execute(string input)
```

#### Parameters

`input` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Raw user input.

#### Returns

[ConsoleExecutionResult](./frinkyengine.core.ui.consolesystem.consoleexecutionresult)<br>
Execution result with output lines for console history.

### **GetRegisteredEntries()**

Gets all registered command and cvar descriptors sorted by name.

```csharp
public static IReadOnlyList<ConsoleEntryDescriptor> GetRegisteredEntries()
```

#### Returns

[IReadOnlyList&lt;ConsoleEntryDescriptor&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>
A read-only list of command and cvar descriptors.

### **GetRegisteredNames()**

Gets all registered command and cvar names sorted alphabetically.

```csharp
public static IReadOnlyList<string> GetRegisteredNames()
```

#### Returns

[IReadOnlyList&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>
A read-only list of command and cvar names.
