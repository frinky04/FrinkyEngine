# Scene

Namespace: FrinkyEngine.Core.Scene

A container of [Entity](./frinkyengine.core.ecs.entity) instances that make up a game level or environment.
 Maintains quick-access lists for cameras, lights, and renderables.

```csharp
public class Scene : System.IDisposable
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Scene](./frinkyengine.core.scene.scene)<br>
Implements [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **TimeScale**

Global time scale applied to the game delta time.
 A value of 1.0 is normal speed, 0.5 is half speed, 0 is paused.

```csharp
public static float TimeScale { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Name**

Display name of this scene.

```csharp
public string Name { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **FilePath**

File path this scene was last saved to or loaded from.

```csharp
public string FilePath { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **EditorCameraPosition**

Saved editor camera position, restored when the scene is reopened in the editor.

```csharp
public Nullable<Vector3> EditorCameraPosition { get; set; }
```

#### Property Value

[Nullable&lt;Vector3&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **EditorCameraYaw**

Saved editor camera yaw angle in degrees.

```csharp
public Nullable<float> EditorCameraYaw { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **EditorCameraPitch**

Saved editor camera pitch angle in degrees.

```csharp
public Nullable<float> EditorCameraPitch { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **PhysicsSettings**

Physics settings used by this scene.

```csharp
public PhysicsSettings PhysicsSettings { get; set; }
```

#### Property Value

[PhysicsSettings](./frinkyengine.core.physics.physicssettings)<br>

### **Entities**

All entities currently in this scene.

```csharp
public IReadOnlyList<Entity> Entities { get; }
```

#### Property Value

[IReadOnlyList&lt;Entity&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### **Cameras**

All active [CameraComponent](./frinkyengine.core.components.cameracomponent) instances in the scene.

```csharp
public IReadOnlyList<CameraComponent> Cameras { get; }
```

#### Property Value

[IReadOnlyList&lt;CameraComponent&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### **Lights**

All active [LightComponent](./frinkyengine.core.components.lightcomponent) instances in the scene.

```csharp
public IReadOnlyList<LightComponent> Lights { get; }
```

#### Property Value

[IReadOnlyList&lt;LightComponent&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### **Renderables**

All active [RenderableComponent](./frinkyengine.core.components.renderablecomponent) instances in the scene.

```csharp
public IReadOnlyList<RenderableComponent> Renderables { get; }
```

#### Property Value

[IReadOnlyList&lt;RenderableComponent&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### **MainCamera**

Gets the first enabled camera marked as [CameraComponent.IsMain](./frinkyengine.core.components.cameracomponent#ismain), or `null` if none exists.

```csharp
public CameraComponent MainCamera { get; }
```

#### Property Value

[CameraComponent](./frinkyengine.core.components.cameracomponent)<br>

## Constructors

### **Scene()**

```csharp
public Scene()
```

## Methods

### **GetComponents&lt;T&gt;()**

Gets all components of type  across all entities in the scene.

```csharp
public List<T> GetComponents<T>()
```

#### Type Parameters

`T`<br>
The component type to search for.

#### Returns

List&lt;T&gt;<br>
A list of matching components.

### **GetComponents(Type)**

Gets all components of the specified runtime type across all entities in the scene.

```csharp
public IReadOnlyList<Component> GetComponents(Type type)
```

#### Parameters

`type` [Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>
The component type to search for.

#### Returns

[IReadOnlyList&lt;Component&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>
A read-only list of matching components.

### **CreateEntity(String)**

Creates a new entity with the given name and adds it to the scene.

```csharp
public Entity CreateEntity(string name)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Display name for the entity.

#### Returns

[Entity](./frinkyengine.core.ecs.entity)<br>
The newly created entity.

### **AddEntity(Entity)**

Adds an existing entity to this scene and registers all its components.

```csharp
public void AddEntity(Entity entity)
```

#### Parameters

`entity` [Entity](./frinkyengine.core.ecs.entity)<br>
The entity to add.

### **RemoveEntity(Entity)**

Removes an entity from this scene, destroying all its components.

```csharp
public void RemoveEntity(Entity entity)
```

#### Parameters

`entity` [Entity](./frinkyengine.core.ecs.entity)<br>
The entity to remove.

### **Start()**

Calls [Component.Start()](./frinkyengine.core.ecs.component#start) on all components that haven't started yet.

```csharp
public void Start()
```

### **Update(Single)**

Runs one frame of the game loop by calling [Component.Update(Single)](./frinkyengine.core.ecs.component#updatesingle), stepping physics, publishing physics visual poses, then calling [Component.LateUpdate(Single)](./frinkyengine.core.ecs.component#lateupdatesingle) on all active entities.

```csharp
public void Update(float dt)
```

#### Parameters

`dt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Time elapsed since the previous frame, in seconds.

### **GetPhysicsFrameStats()**

Returns a snapshot of physics diagnostics for the current frame.

```csharp
public PhysicsFrameStats GetPhysicsFrameStats()
```

#### Returns

[PhysicsFrameStats](./frinkyengine.core.physics.physicsframestats)<br>

### **GetAudioFrameStats()**

Returns a snapshot of audio diagnostics for the current frame.

```csharp
public AudioFrameStats GetAudioFrameStats()
```

#### Returns

[AudioFrameStats](./frinkyengine.core.audio.audioframestats)<br>

### **FindEntityById(Guid)**

Finds an entity in this scene by its [Entity.Id](./frinkyengine.core.ecs.entity#id).

```csharp
public Entity FindEntityById(Guid id)
```

#### Parameters

`id` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>
The GUID to search for.

#### Returns

[Entity](./frinkyengine.core.ecs.entity)<br>
The matching entity, or `null` if not found.

### **Dispose()**

Releases runtime resources associated with this scene (for example physics simulation state).

```csharp
public void Dispose()
```
