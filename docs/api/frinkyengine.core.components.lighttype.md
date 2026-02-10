# LightType

Namespace: FrinkyEngine.Core.Components

Determines the behavior of a light source in the scene.

```csharp
public enum LightType
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [LightType](./frinkyengine.core.components.lighttype)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| Directional | 0 | Casts parallel light rays in the entity's forward direction (like sunlight). |
| Point | 1 | Emits light in all directions from the entity's position, attenuated by [LightComponent.Range](./frinkyengine.core.components.lightcomponent#range). |
| Skylight | 2 | Provides uniform ambient illumination across the entire scene. |
