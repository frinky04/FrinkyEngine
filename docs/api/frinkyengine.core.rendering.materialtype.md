# MaterialType

Namespace: FrinkyEngine.Core.Rendering

Determines how a material surface is rendered.

```csharp
public enum MaterialType
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [MaterialType](./frinkyengine.core.rendering.materialtype)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| SolidColor | 0 | Renders using a flat tint color with no texture. |
| Textured | 1 | Renders using a texture image mapped onto the surface. |
| TriplanarTexture | 2 | Renders using triplanar mapping of a texture image (UVs are not required). |
