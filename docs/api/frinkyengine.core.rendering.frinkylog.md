# FrinkyLog

Namespace: FrinkyEngine.Core.Rendering

Central logging API for the engine. Messages are stored in memory and broadcast via [FrinkyLog.OnLog](./frinkyengine.core.rendering.frinkylog#onlog).

```csharp
public static class FrinkyLog
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FrinkyLog](./frinkyengine.core.rendering.frinkylog)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Entries**

All log entries recorded so far.

```csharp
public static IReadOnlyList<LogEntry> Entries { get; }
```

#### Property Value

[IReadOnlyList&lt;LogEntry&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

## Methods

### **Info(String)**

Logs an informational message.

```csharp
public static void Info(string message)
```

#### Parameters

`message` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The message text.

### **Warning(String)**

Logs a warning message.

```csharp
public static void Warning(string message)
```

#### Parameters

`message` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The message text.

### **Error(String)**

Logs an error message.

```csharp
public static void Error(string message)
```

#### Parameters

`message` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The message text.

### **Log(String, LogLevel)**

Logs a message with the specified level and "Engine" as the source.

```csharp
public static void Log(string message, LogLevel level)
```

#### Parameters

`message` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The message text.

`level` [LogLevel](./frinkyengine.core.rendering.loglevel)<br>
Severity level.

### **Log(String, LogLevel, String)**

Logs a message with the specified level and source.

```csharp
public static void Log(string message, LogLevel level, string source)
```

#### Parameters

`message` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The message text.

`level` [LogLevel](./frinkyengine.core.rendering.loglevel)<br>
Severity level.

`source` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The subsystem producing the message.

### **Clear()**

Removes all stored log entries.

```csharp
public static void Clear()
```

## Events

### **OnLog**

Raised whenever a new log entry is recorded.

```csharp
public static event Action<LogEntry> OnLog;
```

### **OnCleared**

Raised when all log entries are cleared via [FrinkyLog.Clear()](./frinkyengine.core.rendering.frinkylog#clear).

```csharp
public static event Action OnCleared;
```
