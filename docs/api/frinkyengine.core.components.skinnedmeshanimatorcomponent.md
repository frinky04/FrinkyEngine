# SkinnedMeshAnimatorComponent

Namespace: FrinkyEngine.Core.Components

Plays skeletal animation clips for a sibling [MeshRendererComponent](./frinkyengine.core.components.meshrenderercomponent) using GPU skinning.
 Supports frame interpolation, multiple animation sources, and can preview in editor viewport rendering.

```csharp
public sealed class SkinnedMeshAnimatorComponent : FrinkyEngine.Core.ECS.Component
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [SkinnedMeshAnimatorComponent](./frinkyengine.core.components.skinnedmeshanimatorcomponent)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute), [ComponentDisplayNameAttribute](./frinkyengine.core.ecs.componentdisplaynameattribute)

## Properties

### **AnimationSources**

Additional .glb files to load animation clips from. When empty, animations are loaded
 from the mesh file only. When populated, animations are loaded from all listed sources
 and merged into the available clip list. Each source is validated against the model's
 skeleton at load time.

```csharp
public List<AssetReference> AnimationSources { get; set; }
```

#### Property Value

[List&lt;AssetReference&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

### **UseEmbeddedAnimations**

Whether to load animation clips embedded in the mesh file.
 When `true` (default), clips from the mesh file are included in the available clip list.
 When `false`, only clips from [SkinnedMeshAnimatorComponent.AnimationSources](./frinkyengine.core.components.skinnedmeshanimatorcomponent#animationsources) are used.

```csharp
public bool UseEmbeddedAnimations { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

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

### **LoopFrameTrim**

Number of leading frames to skip in a looping animation to avoid dwelling on
 the duplicate seam pose. Most glTF exports include one duplicate frame at the
 start/end boundary; some exporters add more. Only applies when [SkinnedMeshAnimatorComponent.Loop](./frinkyengine.core.components.skinnedmeshanimatorcomponent#loop)
 is enabled.

```csharp
public int LoopFrameTrim { get; set; }
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

### **PlayAnimation(String)**

Plays the animation clip with the given name. Searches across all loaded sources.
 Returns `true` if the clip was found and playback started; `false` otherwise.

```csharp
public bool PlayAnimation(string clipName)
```

#### Parameters

`clipName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The name of the animation clip to play.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **GetAnimationNames()**

Returns the names of all available animation clips across all loaded sources.

```csharp
public String[] GetAnimationNames()
```

#### Returns

[String[]](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **HasAnimation(String)**

Returns `true` if an animation clip with the given name is available.

```csharp
public bool HasAnimation(string clipName)
```

#### Parameters

`clipName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The clip name to search for.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **AddAnimationSource(String)**

Adds a new animation source at runtime. The path should be an asset-relative path
 to a .glb file containing animations compatible with this model's skeleton.

```csharp
public void AddAnimationSource(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Asset-relative path to the animation source file.

### **RemoveAnimationSource(String)**

Removes an animation source at runtime by path.
 Returns `true` if the source was found and removed.

```csharp
public bool RemoveAnimationSource(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Asset-relative path of the source to remove.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

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

### **GetAnimationSourceInfo()**

Returns information about all loaded animation clips, grouped by source file.
 Each entry contains the source path and the clip names loaded from that source.

```csharp
public IReadOnlyList<ValueTuple<string, IReadOnlyList<string>, IReadOnlyList<bool>>> GetAnimationSourceInfo()
```

#### Returns

[IReadOnlyList&lt;ValueTuple&lt;String, IReadOnlyList&lt;String&gt;, IReadOnlyList&lt;Boolean&gt;&gt;&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>
