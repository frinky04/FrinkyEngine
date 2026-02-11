# AudioRolloffMode

Namespace: FrinkyEngine.Core.Audio

Determines how 3D audio volume decays over distance.

```csharp
public enum AudioRolloffMode
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [AudioRolloffMode](./frinkyengine.core.audio.audiorolloffmode)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| Linear | 0 | Linear distance falloff from min to max distance. |
| Logarithmic | 1 | Logarithmic-style distance falloff. |
| CustomCurve | 2 | Reserved for curve-driven rolloff. |
