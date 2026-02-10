# GameAssemblyLoader

Namespace: FrinkyEngine.Core.Scripting

Loads and unloads game script assemblies using a collectible [AssemblyLoadContext](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.loader.assemblyloadcontext) for hot-reload support.
 Automatically registers loaded component types with [ComponentTypeResolver](./frinkyengine.core.serialization.componenttyperesolver).

```csharp
public class GameAssemblyLoader
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [GameAssemblyLoader](./frinkyengine.core.scripting.gameassemblyloader)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **GameAssembly**

The currently loaded game assembly, or `null` if none is loaded.

```csharp
public Assembly GameAssembly { get; }
```

#### Property Value

[Assembly](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.assembly)<br>

## Constructors

### **GameAssemblyLoader()**

```csharp
public GameAssemblyLoader()
```

## Methods

### **LoadAssembly(String)**

Loads a game assembly from the specified DLL path into an isolated load context.

```csharp
public bool LoadAssembly(string dllPath)
```

#### Parameters

`dllPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path to the game assembly DLL.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the assembly was loaded successfully.

### **Unload()**

Unloads the current game assembly and its load context, unregistering all its component types.

```csharp
public void Unload()
```

### **ReloadAssembly(String)**

Unloads the current assembly and loads a new one from the specified path (hot-reload).

```csharp
public bool ReloadAssembly(string dllPath)
```

#### Parameters

`dllPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path to the new game assembly DLL.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the new assembly was loaded successfully.
