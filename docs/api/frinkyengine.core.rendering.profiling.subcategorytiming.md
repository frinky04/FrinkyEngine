# SubCategoryTiming

Namespace: FrinkyEngine.Core.Rendering.Profiling

A named timing entry within a parent [ProfileCategory](./frinkyengine.core.rendering.profiling.profilecategory), used for per-effect detail.

```csharp
public struct SubCategoryTiming
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [SubCategoryTiming](./frinkyengine.core.rendering.profiling.subcategorytiming)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **Parent**

```csharp
public ProfileCategory Parent { get; }
```

#### Property Value

[ProfileCategory](./frinkyengine.core.rendering.profiling.profilecategory)<br>

### **Name**

```csharp
public string Name { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ElapsedMs**

```csharp
public double ElapsedMs { get; }
```

#### Property Value

[Double](https://docs.microsoft.com/en-us/dotnet/api/system.double)<br>

## Constructors

### **SubCategoryTiming(ProfileCategory, String, Double)**

A named timing entry within a parent [ProfileCategory](./frinkyengine.core.rendering.profiling.profilecategory), used for per-effect detail.

```csharp
SubCategoryTiming(ProfileCategory parent, string name, double elapsedMs)
```

#### Parameters

`parent` [ProfileCategory](./frinkyengine.core.rendering.profiling.profilecategory)<br>

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`elapsedMs` [Double](https://docs.microsoft.com/en-us/dotnet/api/system.double)<br>
