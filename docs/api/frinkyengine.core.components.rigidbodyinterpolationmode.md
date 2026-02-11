# RigidbodyInterpolationMode

Namespace: FrinkyEngine.Core.Components

Controls how visual interpolation is applied to this rigidbody.

```csharp
public enum RigidbodyInterpolationMode
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [RigidbodyInterpolationMode](./frinkyengine.core.components.rigidbodyinterpolationmode)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| Inherit | 0 | Uses project-level interpolation settings. |
| None | 1 | Disables visual interpolation for this body. |
| Interpolate | 2 | Forces visual interpolation for this body. |
