# LogEntry

Namespace: FrinkyEngine.Core.Rendering

An immutable log message with metadata.

```csharp
public struct LogEntry
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [LogEntry](./frinkyengine.core.rendering.logentry)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **Message**

The log message text.

```csharp
public string Message { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Level**

Severity level of this entry.

```csharp
public LogLevel Level { get; set; }
```

#### Property Value

[LogLevel](./frinkyengine.core.rendering.loglevel)<br>

### **Timestamp**

When this entry was created.

```csharp
public DateTime Timestamp { get; set; }
```

#### Property Value

[DateTime](https://docs.microsoft.com/en-us/dotnet/api/system.datetime)<br>

### **Source**

The subsystem that produced this entry (e.g. "Engine", "Raylib").

```csharp
public string Source { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
