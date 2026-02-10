# PrefabSerializer

Namespace: FrinkyEngine.Core.Serialization

```csharp
public static class PrefabSerializer
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PrefabSerializer](./frinkyengine.core.serialization.prefabserializer)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **Save(PrefabAssetData, String)**

```csharp
public static void Save(PrefabAssetData prefab, string path)
```

#### Parameters

`prefab` [PrefabAssetData](./frinkyengine.core.prefabs.prefabassetdata)<br>

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Load(String)**

```csharp
public static PrefabAssetData Load(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[PrefabAssetData](./frinkyengine.core.prefabs.prefabassetdata)<br>

### **CreateFromEntity(Entity, Boolean)**

```csharp
public static PrefabAssetData CreateFromEntity(Entity root, bool preserveStableIds)
```

#### Parameters

`root` [Entity](./frinkyengine.core.ecs.entity)<br>

`preserveStableIds` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[PrefabAssetData](./frinkyengine.core.prefabs.prefabassetdata)<br>

### **SerializeNode(Entity, Boolean)**

```csharp
public static PrefabNodeData SerializeNode(Entity entity, bool preserveStableIds)
```

#### Parameters

`entity` [Entity](./frinkyengine.core.ecs.entity)<br>

`preserveStableIds` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[PrefabNodeData](./frinkyengine.core.prefabs.prefabnodedata)<br>

### **SerializeNodeNormalized(Entity, Boolean)**

```csharp
public static PrefabNodeData SerializeNodeNormalized(Entity entity, bool preserveStableIds)
```

#### Parameters

`entity` [Entity](./frinkyengine.core.ecs.entity)<br>

`preserveStableIds` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[PrefabNodeData](./frinkyengine.core.prefabs.prefabnodedata)<br>

### **SerializeComponent(Component)**

```csharp
public static PrefabComponentData SerializeComponent(Component component)
```

#### Parameters

`component` [Component](./frinkyengine.core.ecs.component)<br>

#### Returns

[PrefabComponentData](./frinkyengine.core.prefabs.prefabcomponentdata)<br>

### **ApplyComponentData(Entity, PrefabComponentData)**

```csharp
public static bool ApplyComponentData(Entity entity, PrefabComponentData data)
```

#### Parameters

`entity` [Entity](./frinkyengine.core.ecs.entity)<br>

`data` [PrefabComponentData](./frinkyengine.core.prefabs.prefabcomponentdata)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **DeserializeValue(JsonElement, Type)**

```csharp
public static object DeserializeValue(JsonElement value, Type targetType)
```

#### Parameters

`value` JsonElement<br>

`targetType` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

#### Returns

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

### **SerializeValue(Object, Type)**

```csharp
public static JsonElement SerializeValue(object value, Type valueType)
```

#### Parameters

`value` [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

`valueType` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

#### Returns

JsonElement<br>
