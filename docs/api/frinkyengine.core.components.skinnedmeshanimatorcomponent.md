# SkinnedMeshAnimatorComponent

Namespace: FrinkyEngine.Core.Components

Plays skeletal animation clips for a sibling [MeshRendererComponent](./frinkyengine.core.components.meshrenderercomponent) using GPU skinning.
 Supports frame interpolation and can preview in editor viewport rendering.

```csharp
public sealed class SkinnedMeshAnimatorComponent : FrinkyEngine.Core.ECS.Component
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [SkinnedMeshAnimatorComponent](./frinkyengine.core.components.skinnedmeshanimatorcomponent)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute), [ComponentDisplayNameAttribute](./frinkyengine.core.ecs.componentdisplaynameattribute)

## Properties

### **PlayAutomatically**

Whether playback starts automatically once a valid clip is available.

```csharp
public bool PlayAutomatically { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Loop**

Whether playback loops when reaching the end of the selected clip.

```csharp
public bool Loop { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Playing**

Whether playback advances over time.

```csharp
public bool Playing { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **PlaybackSpeed**

Playback speed multiplier where 1.0 is normal speed.

```csharp
public float PlaybackSpeed { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **AnimationFps**

Animation sample rate in frames per second.

```csharp
public float AnimationFps { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **ClipIndex**

Selected animation clip index.

```csharp
public int ClipIndex { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **ActionName**

Name of the currently selected animation action.

```csharp
public string ActionName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ActionCount**

Number of animation actions loaded for the current model.

```csharp
public int ActionCount { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **FrameCount**

Frame count of the currently selected animation clip.

```csharp
public int FrameCount { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **CurrentModelPose**

Returns the current model-space bone transforms after animation has been applied.
 Each element contains the translation, rotation and scale for that bone index.
 Returns an empty span when no animation state is available.

```csharp
public ReadOnlySpan<ValueTuple<Vector3, Quaternion, Vector3>> CurrentModelPose { get; }
```

#### Property Value

[ReadOnlySpan&lt;ValueTuple&lt;Vector3, Quaternion, Vector3&gt;&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.readonlyspan-1)<br>

### **Entity**

The [Entity](./frinkyengine.core.ecs.entity) this component is attached to.

```csharp
public Entity Entity { get; internal set; }
```

#### Property Value

[Entity](./frinkyengine.core.ecs.entity)<br>

### **Enabled**

Whether this component is active. Disabled components skip [Component.Update(Single)](./frinkyengine.core.ecs.component#updatesingle) and [Component.LateUpdate(Single)](./frinkyengine.core.ecs.component#lateupdatesingle).

```csharp
public bool Enabled { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **EditorOnly**

When `true`, this component is only active in the editor and is skipped during runtime play.

```csharp
public bool EditorOnly { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **HasStarted**

Indicates whether [Component.Start()](./frinkyengine.core.ecs.component#start) has already been called on this component.

```csharp
public bool HasStarted { get; internal set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **SkinnedMeshAnimatorComponent()**

```csharp
public SkinnedMeshAnimatorComponent()
```

## Methods

### **Restart()**

Resets playback time to clip start.

```csharp
public void Restart()
```

### **StopAndResetPose()**

Stops playback and applies bind pose.

```csharp
public void StopAndResetPose()
```

### **Start()**

```csharp
public void Start()
```

### **Awake()**

```csharp
public void Awake()
```

### **OnDestroy()**

```csharp
public void OnDestroy()
```
