# ProjectFile

Namespace: FrinkyEngine.Core.Assets

Represents a `.fproject` file that defines a FrinkyEngine game project's configuration.

```csharp
public class ProjectFile
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ProjectFile](./frinkyengine.core.assets.projectfile)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **ProjectName**

Display name of the project.

```csharp
public string ProjectName { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **DefaultScene**

Asset-relative path to the scene loaded on startup.

```csharp
public string DefaultScene { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **AssetsPath**

Relative path to the assets root directory.

```csharp
public string AssetsPath { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GameAssembly**

Relative path to the compiled game assembly DLL, or empty if none.

```csharp
public string GameAssembly { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GameProject**

Relative path to the game's `.csproj` file, used for building before play/export.

```csharp
public string GameProject { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

## Constructors

### **ProjectFile()**

```csharp
public ProjectFile()
```

## Methods

### **Load(String)**

Loads a project file from disk.

```csharp
public static ProjectFile Load(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path to the `.fproject` file.

#### Returns

[ProjectFile](./frinkyengine.core.assets.projectfile)<br>
The deserialized project file.

### **Save(String)**

Saves this project file to disk as JSON.

```csharp
public void Save(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Destination file path.

### **GetAbsoluteAssetsPath(String)**

Resolves the absolute path to the assets directory from a project root.

```csharp
public string GetAbsoluteAssetsPath(string projectDir)
```

#### Parameters

`projectDir` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Absolute path to the project directory.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The absolute path to the assets folder.

### **GetAbsoluteScenePath(String)**

Resolves the absolute path to the default scene file from a project root.

```csharp
public string GetAbsoluteScenePath(string projectDir)
```

#### Parameters

`projectDir` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Absolute path to the project directory.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The absolute path to the default scene file.
