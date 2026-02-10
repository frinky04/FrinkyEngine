# ConsoleEntryKind

Namespace: FrinkyEngine.Core.UI.ConsoleSystem

Distinguishes between command and cvar entries in the console registry.

```csharp
public enum ConsoleEntryKind
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [ConsoleEntryKind](./frinkyengine.core.ui.consolesystem.consoleentrykind)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| Command | 0 | A callable console command. |
| CVar | 1 | A query/set console variable. |
