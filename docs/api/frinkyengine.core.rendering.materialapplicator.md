# MaterialApplicator

Namespace: FrinkyEngine.Core.Rendering

Applies engine material settings to Raylib model materials.

```csharp
public static class MaterialApplicator
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [MaterialApplicator](./frinkyengine.core.rendering.materialapplicator)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **ApplyToModel(Model, Int32, MaterialType, String, Single, Single, Boolean)**

Applies material settings to the specified model material slot.

```csharp
public static void ApplyToModel(Model model, int materialIndex, MaterialType materialType, string texturePath, float triplanarScale, float triplanarBlendSharpness, bool triplanarUseWorldSpace)
```

#### Parameters

`model` Model<br>
Target model.

`materialIndex` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Material index in the model.

`materialType` [MaterialType](./frinkyengine.core.rendering.materialtype)<br>
Material mapping mode.

`texturePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Asset-relative albedo texture path.

`triplanarScale` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Triplanar projection scale.

`triplanarBlendSharpness` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Triplanar axis blend sharpness.

`triplanarUseWorldSpace` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
Whether triplanar uses world-space coordinates.
