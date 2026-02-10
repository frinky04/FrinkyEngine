# FogMode

Namespace: FrinkyEngine.Core.Rendering.PostProcessing.Effects

Fog falloff modes.

```csharp
public enum FogMode
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [FogMode](./frinkyengine.core.rendering.postprocessing.effects.fogmode)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| Linear | 0 | Linear fog between start and end distances. |
| Exponential | 1 | Exponential fog falloff. |
| ExponentialSquared | 2 | Exponential-squared fog falloff. |
