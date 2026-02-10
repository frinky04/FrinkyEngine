# Audio

Namespace: FrinkyEngine.Core.Audio

High-level gameplay audio API inspired by modern engine static helper calls.

```csharp
public static class Audio
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Audio](./frinkyengine.core.audio.audio)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **PlaySound2D(String, AudioPlayParams&)**

Plays a non-spatialized 2D sound.

```csharp
public static AudioHandle PlaySound2D(string soundPath, AudioPlayParams& playParams)
```

#### Parameters

`soundPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`playParams` [AudioPlayParams&](./frinkyengine.core.audio.audioplayparams&)<br>

#### Returns

[AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

### **PlaySoundAtLocation(String, Vector3, AudioPlayParams&)**

Plays a spatialized 3D sound at a world position.

```csharp
public static AudioHandle PlaySoundAtLocation(string soundPath, Vector3 worldPosition, AudioPlayParams& playParams)
```

#### Parameters

`soundPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`worldPosition` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

`playParams` [AudioPlayParams&](./frinkyengine.core.audio.audioplayparams&)<br>

#### Returns

[AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

### **SpawnSoundAttached(String, Entity, AudioPlayParams&)**

Plays a spatialized 3D sound attached to an entity.

```csharp
public static AudioHandle SpawnSoundAttached(string soundPath, Entity attachTo, AudioPlayParams& playParams)
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
public static bool Stop(AudioHandle handle, float fadeOutSeconds)
```

#### Parameters

`handle` [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

`fadeOutSeconds` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **SetPaused(AudioHandle, Boolean)**

Pauses or resumes a voice.

```csharp
public static bool SetPaused(AudioHandle handle, bool paused)
```

#### Parameters

`handle` [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

`paused` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **SetVolume(AudioHandle, Single)**

Sets per-voice volume.

```csharp
public static bool SetVolume(AudioHandle handle, float volume)
```

#### Parameters

`handle` [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

`volume` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **SetPitch(AudioHandle, Single)**

Sets per-voice pitch.

```csharp
public static bool SetPitch(AudioHandle handle, float pitch)
```

#### Parameters

`handle` [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

`pitch` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **SetWorldPosition(AudioHandle, Vector3)**

Sets world position for a voice.

```csharp
public static bool SetWorldPosition(AudioHandle handle, Vector3 position)
```

#### Parameters

`handle` [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

`position` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **IsPlaying(AudioHandle)**

Returns whether a voice is currently active.

```csharp
public static bool IsPlaying(AudioHandle handle)
```

#### Parameters

`handle` [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **SetBusVolume(AudioBusId, Single)**

Sets volume for a mixer bus.

```csharp
public static bool SetBusVolume(AudioBusId bus, float volume)
```

#### Parameters

`bus` [AudioBusId](./frinkyengine.core.audio.audiobusid)<br>

`volume` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **SetBusMuted(AudioBusId, Boolean)**

Sets mute for a mixer bus.

```csharp
public static bool SetBusMuted(AudioBusId bus, bool muted)
```

#### Parameters

`bus` [AudioBusId](./frinkyengine.core.audio.audiobusid)<br>

`muted` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **GetBusVolume(AudioBusId)**

Gets current volume for a mixer bus.

```csharp
public static float GetBusVolume(AudioBusId bus)
```

#### Parameters

`bus` [AudioBusId](./frinkyengine.core.audio.audiobusid)<br>

#### Returns

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
