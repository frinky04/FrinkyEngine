# ProjectSettings

Namespace: FrinkyEngine.Core.Assets

Persisted project settings stored in `project_settings.json` alongside the `.fproject` file.
 Covers metadata, runtime configuration, and build options.

```csharp
public class ProjectSettings
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ProjectSettings](./frinkyengine.core.assets.projectsettings)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Fields

### **FileName**

The settings file name on disk.

```csharp
public static string FileName;
```

## Properties

### **Project**

Metadata about the project (version, author, etc.).

```csharp
public ProjectMetadataSettings Project { get; set; }
```

#### Property Value

[ProjectMetadataSettings](./frinkyengine.core.assets.projectmetadatasettings)<br>

### **Runtime**

Runtime behavior settings (FPS, window size, Forward+ config, etc.).

```csharp
public RuntimeProjectSettings Runtime { get; set; }
```

#### Property Value

[RuntimeProjectSettings](./frinkyengine.core.assets.runtimeprojectsettings)<br>

### **Build**

Build and export settings (output name, version).

```csharp
public BuildProjectSettings Build { get; set; }
```

#### Property Value

[BuildProjectSettings](./frinkyengine.core.assets.buildprojectsettings)<br>

## Constructors

### **ProjectSettings()**

```csharp
public ProjectSettings()
```

## Methods

### **GetPath(String)**

Gets the full path to the settings file within a project directory.

```csharp
public static string GetPath(string projectDirectory)
```

#### Parameters

`projectDirectory` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Absolute path to the project directory.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The full path to the settings file.

### **GetDefault(String)**

Creates a [ProjectSettings](./frinkyengine.core.assets.projectsettings) populated with sensible defaults.

```csharp
public static ProjectSettings GetDefault(string projectName)
```

#### Parameters

`projectName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Project name used for window title and output name.

#### Returns

[ProjectSettings](./frinkyengine.core.assets.projectsettings)<br>
A new settings instance with default values.

### **LoadOrCreate(String, String)**

Loads settings from disk, or creates and saves defaults if the file doesn't exist.

```csharp
public static ProjectSettings LoadOrCreate(string projectDirectory, string projectName)
```

#### Parameters

`projectDirectory` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Absolute path to the project directory.

`projectName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Fallback project name if none is stored.

#### Returns

[ProjectSettings](./frinkyengine.core.assets.projectsettings)<br>
The loaded or newly created settings.

### **Load(String, String)**

Loads settings from the specified file path, falling back to defaults on error.

```csharp
public static ProjectSettings Load(string path, string projectName)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path to the settings JSON file.

`projectName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Fallback project name for normalization.

#### Returns

[ProjectSettings](./frinkyengine.core.assets.projectsettings)<br>
The loaded settings.

### **Save(String)**

Saves these settings to disk as JSON.

```csharp
public void Save(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Destination file path.

### **Clone()**

Creates a deep copy of these settings.

```csharp
public ProjectSettings Clone()
```

#### Returns

[ProjectSettings](./frinkyengine.core.assets.projectsettings)<br>
A new [ProjectSettings](./frinkyengine.core.assets.projectsettings) with the same values.

### **Normalize(String)**

Ensures all fields have valid values, clamping out-of-range numbers and filling empty strings.

```csharp
public void Normalize(string defaultProjectName)
```

#### Parameters

`defaultProjectName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Project name to use as a fallback for empty fields.

### **ResolveStartupScene(String)**

Resolves the startup scene path, preferring [RuntimeProjectSettings.StartupSceneOverride](./frinkyengine.core.assets.runtimeprojectsettings#startupsceneoverride) if set.

```csharp
public string ResolveStartupScene(string defaultScene)
```

#### Parameters

`defaultScene` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The default scene path from the project file.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The resolved scene path.
