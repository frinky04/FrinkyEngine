# WaitForSecondsRealtime

Namespace: FrinkyEngine.Core.Coroutines

Suspends a coroutine for the specified number of seconds using unscaled (real) time.

```csharp
public class WaitForSecondsRealtime : YieldInstruction
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [YieldInstruction](./frinkyengine.core.coroutines.yieldinstruction) → [WaitForSecondsRealtime](./frinkyengine.core.coroutines.waitforsecondsrealtime)

## Constructors

### **WaitForSecondsRealtime(Single)**

Creates a new wait instruction that pauses for `seconds` of real time.

```csharp
public WaitForSecondsRealtime(float seconds)
```

#### Parameters

`seconds` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Duration to wait in real (unscaled) seconds. Negative values are treated as zero.
