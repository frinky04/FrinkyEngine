# AudioSourceComponent

Namespace: FrinkyEngine.Core.Components

Plays an audio asset from an entity with optional 3D spatialization.

```csharp
public class AudioSourceComponent : FrinkyEngine.Core.ECS.Component
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [AudioSourceComponent](./frinkyengine.core.components.audiosourcecomponent)<br>
Attributes [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute)

## Properties

### **SoundPath**

Asset-relative or absolute path to the sound file.

```csharp
public AssetReference SoundPath { get; set; }
```

#### Property Value

[AssetReference](./frinkyengine.core.assets.assetreference)<br>

### **PlayOnStart**

Automatically starts playback in [AudioSourceComponent.Start()](./frinkyengine.core.components.audiosourcecomponent#start).

```csharp
public bool PlayOnStart { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Looping**

Loops the voice until explicitly stopped.

```csharp
public bool Looping { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Spatialized**

Enables 3D spatialization.

```csharp
public bool Spatialized { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **AutoDestroyOnFinish**

Clears runtime handle state when one-shot playback ends.

```csharp
public bool AutoDestroyOnFinish { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **AutoResumeOnEnable**

Automatically resumes playback when this component is re-enabled.

```csharp
public bool AutoResumeOnEnable { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Bus**

Mixer bus route for this source.

```csharp
public AudioBusId Bus { get; set; }
```

#### Property Value

[AudioBusId](./frinkyengine.core.audio.audiobusid)<br>

### **Volume**

Per-source volume multiplier.

```csharp
public float Volume { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Pitch**

Per-source pitch multiplier.

```csharp
public float Pitch { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **StartTimeSeconds**

Start offset in seconds for streamed clips.

```csharp
public float StartTimeSeconds { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Mute**

Whether this source is muted.

```csharp
public bool Mute { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Paused**

Whether this source is paused.

```csharp
public bool Paused { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Priority**

Priority used by voice stealing.

```csharp
public int Priority { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Attenuation**

Spatial attenuation settings.

```csharp
public AudioAttenuationSettings Attenuation { get; set; }
```

#### Property Value

[AudioAttenuationSettings](./frinkyengine.core.audio.audioattenuationsettings)<br>

### **CurrentHandle**

Current runtime handle for active playback, if any.

```csharp
public AudioHandle CurrentHandle { get; }
```

#### Property Value

[AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

### **IsPlaying**

True while this source has an active voice handle.

```csharp
public bool IsPlaying { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

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

### **AudioSourceComponent()**

```csharp
public AudioSourceComponent()
```

## Methods

### **Play()**

Starts playback using current source settings.

```csharp
public void Play()
```

### **Stop(Single)**

Stops playback.

```csharp
public void Stop(float fadeOutSeconds)
```

#### Parameters

`fadeOutSeconds` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Optional fade-out duration in seconds.

### **Pause()**

Pauses playback.

```csharp
public void Pause()
```

### **Resume()**

Resumes playback.

```csharp
public void Resume()
```

### **SetSound(String)**

Assigns a new sound path.

```csharp
public void SetSound(string soundPath)
```

#### Parameters

`soundPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **FadeIn(Single)**

Starts playback with fade-in.

```csharp
public void FadeIn(float seconds)
```

#### Parameters

`seconds` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **FadeOut(Single)**

Stops playback with fade-out.

```csharp
public void FadeOut(float seconds)
```

#### Parameters

`seconds` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Start()**

```csharp
public void Start()
```

### **OnEnable()**

```csharp
public void OnEnable()
```

### **OnDisable()**

```csharp
public void OnDisable()
```

### **OnDestroy()**

```csharp
public void OnDestroy()
```
