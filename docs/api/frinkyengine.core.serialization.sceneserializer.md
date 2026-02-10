# SceneSerializer

Namespace: FrinkyEngine.Core.Serialization

Handles saving and loading scenes in the `.fscene` JSON format.
 Also provides entity duplication via serialization round-trips.

```csharp
public static class SceneSerializer
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [SceneSerializer](./frinkyengine.core.serialization.sceneserializer)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **Save(Scene, String)**

Saves a scene to a `.fscene` file.

```csharp
public static void Save(Scene scene, string path)
```

#### Parameters

`scene` [Scene](./frinkyengine.core.scene.scene)<br>
The scene to save.

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Destination file path.

### **Load(String)**

Loads a scene from a `.fscene` file.

```csharp
public static Scene Load(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path to the scene file.

#### Returns

[Scene](./frinkyengine.core.scene.scene)<br>
The loaded scene, or `null` if the file doesn't exist or is invalid.

### **SerializeToString(Scene)**

Serializes a scene to a JSON string (useful for snapshots and clipboard operations).

```csharp
public static string SerializeToString(Scene scene)
```

#### Parameters

`scene` [Scene](./frinkyengine.core.scene.scene)<br>
The scene to serialize.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The JSON string.

### **DeserializeFromString(String)**

Deserializes a scene from a JSON string.

```csharp
public static Scene DeserializeFromString(string json)
```

#### Parameters

`json` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The JSON string to parse.

#### Returns

[Scene](./frinkyengine.core.scene.scene)<br>
The deserialized scene, or `null` if the JSON is invalid.

### **DuplicateEntity(Entity, Scene)**

Creates a deep copy of an entity (and its children) and adds it to the scene.

```csharp
public static Entity DuplicateEntity(Entity source, Scene scene)
```

#### Parameters

`source` [Entity](./frinkyengine.core.ecs.entity)<br>
The entity to duplicate.

`scene` [Scene](./frinkyengine.core.scene.scene)<br>
The scene to add the duplicate to.

#### Returns

[Entity](./frinkyengine.core.ecs.entity)<br>
The duplicated entity, or `null` if duplication failed.

### **GenerateDuplicateName(String)**

Generates a duplicate name by appending or incrementing a " (N)" suffix.

```csharp
public static string GenerateDuplicateName(string name)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The original entity name.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The new name with an incremented suffix.
