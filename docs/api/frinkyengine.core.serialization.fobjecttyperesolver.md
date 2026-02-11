# FObjectTypeResolver

Namespace: FrinkyEngine.Core.Serialization

Discovers and resolves [FObject](./frinkyengine.core.ecs.fobject) subclasses from engine and game assemblies.

```csharp
public static class FObjectTypeResolver
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FObjectTypeResolver](./frinkyengine.core.serialization.fobjecttyperesolver)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **RegisterAssembly(Assembly)**

Scans an assembly for concrete [FObject](./frinkyengine.core.ecs.fobject) subclasses and registers them.

```csharp
public static void RegisterAssembly(Assembly assembly)
```

#### Parameters

`assembly` [Assembly](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.assembly)<br>

### **UnregisterAssembly(Assembly)**

Removes all FObject types that were registered from the specified assembly.

```csharp
public static void UnregisterAssembly(Assembly assembly)
```

#### Parameters

`assembly` [Assembly](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.assembly)<br>

### **Resolve(String)**

Resolves a type name (short or fully qualified) to a [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type).

```csharp
public static Type Resolve(string typeName)
```

#### Parameters

`typeName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

### **GetAllTypes()**

Gets all distinct registered FObject types across all assemblies.

```csharp
public static IEnumerable<Type> GetAllTypes()
```

#### Returns

[IEnumerable&lt;Type&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>

### **GetTypesAssignableTo(Type)**

Gets all registered FObject types that are assignable to the specified base type.

```csharp
public static IEnumerable<Type> GetTypesAssignableTo(Type baseType)
```

#### Parameters

`baseType` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

#### Returns

[IEnumerable&lt;Type&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>

### **GetTypeName(Type)**

Gets the fully qualified type name used as a serialization key.

```csharp
public static string GetTypeName(Type type)
```

#### Parameters

`type` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GetAssemblySource(Type)**

Gets a human-readable label indicating whether a type comes from the engine or a game assembly.

```csharp
public static string GetAssemblySource(Type type)
```

#### Parameters

`type` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GetDisplayName(Type)**

Gets a human-readable display name for an FObject type.

```csharp
public static string GetDisplayName(Type type)
```

#### Parameters

`type` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
