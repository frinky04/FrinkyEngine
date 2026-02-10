# SceneData

Namespace: FrinkyEngine.Core.Serialization

JSON-serializable representation of a scene.

```csharp
public class SceneData
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [SceneData](./frinkyengine.core.serialization.scenedata)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Name**

Scene display name.

```csharp
public string Name { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **EditorCameraPosition**

Saved editor camera position.

```csharp
public Nullable<Vector3> EditorCameraPosition { get; set; }
```

#### Property Value

[Nullable&lt;Vector3&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **EditorCameraYaw**

Saved editor camera yaw angle.

```csharp
public Nullable<float> EditorCameraYaw { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **EditorCameraPitch**

Saved editor camera pitch angle.

```csharp
public Nullable<float> EditorCameraPitch { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **Physics**

Scene physics configuration.

```csharp
public PhysicsSettings Physics { get; set; }
```

#### Property Value

[PhysicsSettings](./frinkyengine.core.physics.physicssettings)<br>

### **Entities**

Serialized root entities (children are nested within each entity).

```csharp
public List<EntityData> Entities { get; set; }
```

#### Property Value

[List&lt;EntityData&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

## Constructors

### **SceneData()**

```csharp
public SceneData()
```
