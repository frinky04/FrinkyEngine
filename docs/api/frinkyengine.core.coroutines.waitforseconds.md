# WaitForSeconds

Namespace: FrinkyEngine.Core.Coroutines

Suspends a coroutine for the specified number of seconds using scaled time.

```csharp
public class WaitForSeconds : YieldInstruction
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [YieldInstruction](./frinkyengine.core.coroutines.yieldinstruction) → [WaitForSeconds](./frinkyengine.core.coroutines.waitforseconds)

## Constructors

### **WaitForSeconds(Single)**

Creates a new wait instruction that pauses for `seconds` of scaled time.

```csharp
public WaitForSeconds(float seconds)
```

#### Parameters

`seconds` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Duration to wait in scaled seconds. Negative values are treated as zero.
