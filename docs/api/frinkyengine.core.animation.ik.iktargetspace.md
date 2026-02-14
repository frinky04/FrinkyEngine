# IKTargetSpace

Namespace: FrinkyEngine.Core.Animation.IK

Coordinate space for IK targets.

```csharp
public enum IKTargetSpace
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [IKTargetSpace](./frinkyengine.core.animation.ik.iktargetspace)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| World | 0 | Target is specified in world space. |
| Local | 1 | Target is specified relative to the owning entity's transform. |
