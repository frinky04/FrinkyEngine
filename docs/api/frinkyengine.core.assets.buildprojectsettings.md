# BuildProjectSettings

Namespace: FrinkyEngine.Core.Assets

Settings that control game export and packaging.

```csharp
public class BuildProjectSettings
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [BuildProjectSettings](./frinkyengine.core.assets.buildprojectsettings)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **OutputName**

Name of the exported executable (without extension).

```csharp
public string OutputName { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **BuildVersion**

Version string embedded in the export.

```csharp
public string BuildVersion { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

## Constructors

### **BuildProjectSettings()**

```csharp
public BuildProjectSettings()
```
