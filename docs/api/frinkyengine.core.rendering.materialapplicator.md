# MaterialApplicator

Namespace: FrinkyEngine.Core.Rendering

Applies engine material settings to Raylib model materials.

```csharp
public static class MaterialApplicator
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [MaterialApplicator](./frinkyengine.core.rendering.materialapplicator)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **ApplyToModel(Model, Int32, Material)**

Applies material settings from a [Material](./frinkyengine.core.components.material) to the specified model material slot.

```csharp
public static void ApplyToModel(Model model, int materialIndex, Material material)
```

#### Parameters

`model` Model<br>
Target model.

`materialIndex` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Material index in the model.

`material` [Material](./frinkyengine.core.components.material)<br>
Material configuration to apply.
