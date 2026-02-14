# PrefabInstantiator

Namespace: FrinkyEngine.Core.Prefabs

Provides static methods to instantiate prefabs into a scene at runtime.
 This logic is shared by both the editor and runtime.

```csharp
public static class PrefabInstantiator
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PrefabInstantiator](./frinkyengine.core.prefabs.prefabinstantiator)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **Instantiate(Scene, String, TransformComponent)**

```csharp
public static Entity Instantiate(Scene scene, string prefabPath, TransformComponent parent)
```

#### Parameters

`scene` [Scene](./frinkyengine.core.scene.scene)<br>

`prefabPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`parent` [TransformComponent](./frinkyengine.core.components.transformcomponent)<br>

#### Returns

[Entity](./frinkyengine.core.ecs.entity)<br>

### **Instantiate(Scene, String, Vector3, Quaternion, TransformComponent)**

```csharp
public static Entity Instantiate(Scene scene, string prefabPath, Vector3 position, Quaternion rotation, TransformComponent parent)
```

#### Parameters

`scene` [Scene](./frinkyengine.core.scene.scene)<br>

`prefabPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`position` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

`rotation` [Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>

`parent` [TransformComponent](./frinkyengine.core.components.transformcomponent)<br>

#### Returns

[Entity](./frinkyengine.core.ecs.entity)<br>

### **Instantiate(Scene, AssetReference, TransformComponent)**

```csharp
public static Entity Instantiate(Scene scene, AssetReference prefab, TransformComponent parent)
```

#### Parameters

`scene` [Scene](./frinkyengine.core.scene.scene)<br>

`prefab` [AssetReference](./frinkyengine.core.assets.assetreference)<br>

`parent` [TransformComponent](./frinkyengine.core.components.transformcomponent)<br>

#### Returns

[Entity](./frinkyengine.core.ecs.entity)<br>

### **Instantiate(Scene, AssetReference, Vector3, Quaternion, TransformComponent)**

```csharp
public static Entity Instantiate(Scene scene, AssetReference prefab, Vector3 position, Quaternion rotation, TransformComponent parent)
```

#### Parameters

`scene` [Scene](./frinkyengine.core.scene.scene)<br>

`prefab` [AssetReference](./frinkyengine.core.assets.assetreference)<br>

`position` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

`rotation` [Quaternion](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.quaternion)<br>

`parent` [TransformComponent](./frinkyengine.core.components.transformcomponent)<br>

#### Returns

[Entity](./frinkyengine.core.ecs.entity)<br>

### **InstantiatePrefabInternal(String, Scene, TransformComponent, PrefabOverridesData, Nullable&lt;Guid&gt;)**

```csharp
public static Entity InstantiatePrefabInternal(string assetPath, Scene scene, TransformComponent parent, PrefabOverridesData overrides, Nullable<Guid> forcedRootId)
```

#### Parameters

`assetPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`scene` [Scene](./frinkyengine.core.scene.scene)<br>

`parent` [TransformComponent](./frinkyengine.core.components.transformcomponent)<br>

`overrides` [PrefabOverridesData](./frinkyengine.core.prefabs.prefaboverridesdata)<br>

`forcedRootId` [Nullable&lt;Guid&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

#### Returns

[Entity](./frinkyengine.core.ecs.entity)<br>
