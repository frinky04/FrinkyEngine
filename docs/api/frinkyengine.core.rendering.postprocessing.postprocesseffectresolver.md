# PostProcessEffectResolver

Namespace: FrinkyEngine.Core.Rendering.PostProcessing

Discovers and resolves [PostProcessEffect](./frinkyengine.core.rendering.postprocessing.postprocesseffect) subclasses from engine and game assemblies.
 Parallel to [ComponentTypeResolver](./frinkyengine.core.serialization.componenttyperesolver) but for post-processing effects.

```csharp
public static class PostProcessEffectResolver
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PostProcessEffectResolver](./frinkyengine.core.rendering.postprocessing.postprocesseffectresolver)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **RegisterAssembly(Assembly)**

Scans an assembly for concrete [PostProcessEffect](./frinkyengine.core.rendering.postprocessing.postprocesseffect) subclasses and registers them.

```csharp
public static void RegisterAssembly(Assembly assembly)
```

#### Parameters

`assembly` [Assembly](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.assembly)<br>
The assembly to scan.

### **UnregisterAssembly(Assembly)**

Removes all effect types that were registered from the specified assembly.

```csharp
public static void UnregisterAssembly(Assembly assembly)
```

#### Parameters

`assembly` [Assembly](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.assembly)<br>
The assembly to unregister.

### **Resolve(String)**

Resolves a type name (short or fully qualified) to a [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type).

```csharp
public static Type Resolve(string typeName)
```

#### Parameters

`typeName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The type name to look up.

#### Returns

[Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>
The resolved type, or `null` if not found.

### **GetAllEffectTypes()**

Gets all distinct registered effect types across all assemblies.

```csharp
public static IEnumerable<Type> GetAllEffectTypes()
```

#### Returns

[IEnumerable&lt;Type&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>
An enumerable of effect types.

### **GetTypeName(Type)**

Gets the fully qualified type name used as a serialization key.

```csharp
public static string GetTypeName(Type type)
```

#### Parameters

`type` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>
The effect type.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The full type name.

### **GetAssemblySource(Type)**

Gets a human-readable label indicating whether a type comes from the engine or a game assembly.

```csharp
public static string GetAssemblySource(Type type)
```

#### Parameters

`type` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>
The effect type.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
"Engine" for built-in types, or the game assembly name.

### **GetDisplayName(Type)**

Gets a human-readable display name for an effect type by creating an instance and reading [PostProcessEffect.DisplayName](./frinkyengine.core.rendering.postprocessing.postprocesseffect#displayname).
 Falls back to stripping "Effect" suffix and inserting spaces.

```csharp
public static string GetDisplayName(Type type)
```

#### Parameters

`type` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>
The effect type.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A display name string.
