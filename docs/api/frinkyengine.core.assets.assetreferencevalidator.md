# AssetReferenceValidator

Namespace: FrinkyEngine.Core.Assets

Scans all entities in a scene for [AssetReference](./frinkyengine.core.assets.assetreference) properties
 and logs warnings for broken (non-empty but non-existent) references.

```csharp
public static class AssetReferenceValidator
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [AssetReferenceValidator](./frinkyengine.core.assets.assetreferencevalidator)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **ValidateScene(Scene)**

Validates all asset references in the given scene against the asset database.

```csharp
public static void ValidateScene(Scene scene)
```

#### Parameters

`scene` [Scene](./frinkyengine.core.scene.scene)<br>
