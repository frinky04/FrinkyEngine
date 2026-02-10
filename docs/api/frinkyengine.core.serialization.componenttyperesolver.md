# ComponentTypeResolver

Namespace: FrinkyEngine.Core.Serialization

Maps component type names to [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type) objects for scene deserialization.
 Supports registering external assemblies (e.g. game scripts) for custom component types.

```csharp
public static class ComponentTypeResolver
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ComponentTypeResolver](./frinkyengine.core.serialization.componenttyperesolver)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **RegisterAssembly(Assembly)**

Scans an assembly for concrete [Component](./frinkyengine.core.ecs.component) subclasses and registers them by both short name and full name.

```csharp
public static void RegisterAssembly(Assembly assembly)
```

#### Parameters

`assembly` [Assembly](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.assembly)<br>
The assembly to scan.

### **UnregisterAssembly(Assembly)**

Removes all component types that were registered from the specified assembly.

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

### **GetAllComponentTypes()**

Gets all distinct registered component types across all assemblies.

```csharp
public static IEnumerable<Type> GetAllComponentTypes()
```

#### Returns

[IEnumerable&lt;Type&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>
An enumerable of component types.

### **GetTypeName(Type)**

Gets the fully qualified type name used as a serialization key.

```csharp
public static string GetTypeName(Type type)
```

#### Parameters

`type` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>
The component type.

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
The component type.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
"Engine" for built-in types, or the game assembly name.

### **GetCategory(Type)**

Gets the category declared via [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute), or `null` if none.
 Categories support slash-separated nesting (e.g. "Physics/Colliders").

```csharp
public static string GetCategory(Type type)
```

#### Parameters

`type` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GetDisplayName(Type)**

Gets a human-readable display name for a component type.
 Returns the [ComponentDisplayNameAttribute](./frinkyengine.core.ecs.componentdisplaynameattribute) value if present,
 otherwise strips "Component" suffix and inserts spaces between PascalCase words.

```csharp
public static string GetDisplayName(Type type)
```

#### Parameters

`type` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
