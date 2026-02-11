# AssetFilterAttribute

Namespace: FrinkyEngine.Core.Assets

Restricts an [AssetReference](./frinkyengine.core.assets.assetreference) property to a specific asset type in the editor.

```csharp
public class AssetFilterAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [AssetFilterAttribute](./frinkyengine.core.assets.assetfilterattribute)<br>
Attributes [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **Filter**

The asset type filter to apply.

```csharp
public AssetType Filter { get; }
```

#### Property Value

[AssetType](./frinkyengine.core.assets.assettype)<br>

### **TypeId**

```csharp
public object TypeId { get; }
```

#### Property Value

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

## Constructors

### **AssetFilterAttribute(AssetType)**

Creates a new asset filter attribute.

```csharp
public AssetFilterAttribute(AssetType filter)
```

#### Parameters

`filter` [AssetType](./frinkyengine.core.assets.assettype)<br>
