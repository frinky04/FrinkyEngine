# WaitWhile

Namespace: FrinkyEngine.Core.Coroutines

Suspends a coroutine while the supplied predicate returns `true`.
 Resumes when the predicate returns `false`.

```csharp
public class WaitWhile : YieldInstruction
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [YieldInstruction](./frinkyengine.core.coroutines.yieldinstruction) → [WaitWhile](./frinkyengine.core.coroutines.waitwhile)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Constructors

### **WaitWhile(Func&lt;Boolean&gt;)**

Creates a new wait instruction that resumes when `predicate` returns `false`.

```csharp
public WaitWhile(Func<bool> predicate)
```

#### Parameters

`predicate` [Func&lt;Boolean&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.func-1)<br>
The condition to evaluate each frame.
