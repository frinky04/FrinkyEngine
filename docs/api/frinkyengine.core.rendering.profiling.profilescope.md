# ProfileScope

Namespace: FrinkyEngine.Core.Rendering.Profiling

#### Caution

Types with embedded references are not supported in this version of your compiler.

---

Disposable scope that accumulates elapsed time into a profiler category.
 Stack-only (`ref struct`) to avoid heap allocation.
 Scopes are exclusive: entering a new scope pauses the previous one.

```csharp
public struct ProfileScope
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [ProfileScope](./frinkyengine.core.rendering.profiling.profilescope)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [IsByRefLikeAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isbyreflikeattribute), [ObsoleteAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.obsoleteattribute), [CompilerFeatureRequiredAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.compilerfeaturerequiredattribute)

## Methods

### **Dispose()**

```csharp
void Dispose()
```
