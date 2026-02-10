# AssetType

Namespace: FrinkyEngine.Core.Assets

Categorizes project assets by their file type.

```csharp
public enum AssetType
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [AssetType](./frinkyengine.core.assets.assettype)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| Unknown | 0 | Unrecognized file extension. |
| Model | 1 | A 3D model file (.obj, .gltf, .glb, etc.). |
| Scene | 2 | A scene file (.fscene). |
| Texture | 3 | An image file (.png, .jpg, etc.). |
| Audio | 4 | An audio file (.wav, .ogg, .mp3). |
| Script | 5 | A C# script file (.cs). |
| Prefab | 6 | A prefab file (.fprefab). |
