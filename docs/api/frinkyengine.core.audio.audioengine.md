# AudioEngine

Namespace: FrinkyEngine.Core.Audio

Runtime audio mixer and voice manager used by active scenes.

```csharp
public sealed class AudioEngine : System.IDisposable
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [AudioEngine](./frinkyengine.core.audio.audioengine)<br>
Implements [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **ListenerVolumeScale**

Additional listener-level volume scale.

```csharp
public float ListenerVolumeScale { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

## Constructors

### **AudioEngine()**

Creates a new audio engine with the default Raylib backend.

```csharp
public AudioEngine()
```

## Methods

### **PlaySound2D(String, AudioPlayParams&)**

Plays a non-spatialized 2D sound.

```csharp
public AudioHandle PlaySound2D(string soundPath, AudioPlayParams& playParams)
```

#### Parameters

`soundPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`playParams` [AudioPlayParams&](./frinkyengine.core.audio.audioplayparams&)<br>

#### Returns

[AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

### **PlaySoundAtLocation(String, Vector3, AudioPlayParams&)**

Plays a spatialized 3D sound at a world position.

```csharp
public AudioHandle PlaySoundAtLocation(string soundPath, Vector3 worldPosition, AudioPlayParams& playParams)
```

#### Parameters

`soundPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`worldPosition` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

`playParams` [AudioPlayParams&](./frinkyengine.core.audio.audioplayparams&)<br>

#### Returns

[AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

### **SpawnSoundAttached(String, Entity, AudioPlayParams&)**

Plays a spatialized 3D sound attached to an entity transform.

```csharp
public AudioHandle SpawnSoundAttached(string soundPath, Entity attachTo, AudioPlayParams& playParams)
```

#### Parameters

`soundPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`attachTo` [Entity](./frinkyengine.core.ecs.entity)<br>

`playParams` [AudioPlayParams&](./frinkyengine.core.audio.audioplayparams&)<br>

#### Returns

[AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

### **Stop(AudioHandle, Single)**

Stops a voice immediately or after a fade-out.

```csharp
public bool Stop(AudioHandle handle, float fadeOutSeconds)
```

#### Parameters

`handle` [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

`fadeOutSeconds` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **SetPaused(AudioHandle, Boolean)**

Pauses or resumes a voice.

```csharp
public bool SetPaused(AudioHandle handle, bool paused)
```

#### Parameters

`handle` [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

`paused` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **SetVolume(AudioHandle, Single)**

Sets per-voice volume.

```csharp
public bool SetVolume(AudioHandle handle, float volume)
```

#### Parameters

`handle` [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

`volume` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **SetPitch(AudioHandle, Single)**

Sets per-voice pitch.

```csharp
public bool SetPitch(AudioHandle handle, float pitch)
```

#### Parameters

`handle` [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

`pitch` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **SetWorldPosition(AudioHandle, Vector3)**

Sets world position for a voice and detaches it from any bound entity.

```csharp
public bool SetWorldPosition(AudioHandle handle, Vector3 position)
```

#### Parameters

`handle` [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

`position` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **IsPlaying(AudioHandle)**

Returns true if a voice exists and is currently active.

```csharp
public bool IsPlaying(AudioHandle handle)
```

#### Parameters

`handle` [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ConfigureHandle(AudioHandle, AudioBusId, Boolean, AudioAttenuationSettings, Int32, Boolean)**

Updates routing/spatial parameters for an existing voice.

```csharp
public bool ConfigureHandle(AudioHandle handle, AudioBusId bus, bool spatialized, AudioAttenuationSettings attenuation, int priority, bool looping)
```

#### Parameters

`handle` [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

`bus` [AudioBusId](./frinkyengine.core.audio.audiobusid)<br>

`spatialized` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

`attenuation` [AudioAttenuationSettings](./frinkyengine.core.audio.audioattenuationsettings)<br>

`priority` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`looping` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **SetBusVolume(AudioBusId, Single)**

Sets bus volume.

```csharp
public bool SetBusVolume(AudioBusId bus, float volume)
```

#### Parameters

`bus` [AudioBusId](./frinkyengine.core.audio.audiobusid)<br>

`volume` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **GetBusVolume(AudioBusId)**

Gets bus volume.

```csharp
public float GetBusVolume(AudioBusId bus)
```

#### Parameters

`bus` [AudioBusId](./frinkyengine.core.audio.audiobusid)<br>

#### Returns

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **SetBusMuted(AudioBusId, Boolean)**

Sets bus mute state.

```csharp
public bool SetBusMuted(AudioBusId bus, bool muted)
```

#### Parameters

`bus` [AudioBusId](./frinkyengine.core.audio.audiobusid)<br>

`muted` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **SetListener(Vector3, Vector3, Vector3)**

Updates listener transform used by spatialization.

```csharp
public void SetListener(Vector3 position, Vector3 right, Vector3 forward)
```

#### Parameters

`position` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

`right` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

`forward` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

### **Update(Single)**

Updates active voices and mixer state.

```csharp
public void Update(float dt)
```

#### Parameters

`dt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Frame delta time in seconds.

### **GetFrameStats()**

Returns latest frame diagnostics.

```csharp
public AudioFrameStats GetFrameStats()
```

#### Returns

[AudioFrameStats](./frinkyengine.core.audio.audioframestats)<br>

### **Dispose()**

```csharp
public void Dispose()
```
