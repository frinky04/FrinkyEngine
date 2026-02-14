# WaitUntil

Namespace: FrinkyEngine.Core.Coroutines

Suspends a coroutine until the supplied predicate returns `true`.

```csharp
public class WaitUntil : YieldInstruction
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [YieldInstruction](./frinkyengine.core.coroutines.yieldinstruction) → [WaitUntil](./frinkyengine.core.coroutines.waituntil)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Constructors

### **WaitUntil(Func&lt;Boolean&gt;)**

Creates a new wait instruction that resumes when `predicate` returns `true`.

```csharp
public WaitUntil(Func<bool> predicate)
```

#### Parameters

`predicate` [Func&lt;Boolean&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.func-1)<br>
The condition to evaluate each frame.
