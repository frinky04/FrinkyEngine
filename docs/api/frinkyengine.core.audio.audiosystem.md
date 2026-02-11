# AudioSystem

Namespace: FrinkyEngine.Core.Audio

Scene-level bridge that synchronizes ECS audio components with the audio engine.

```csharp
public sealed class AudioSystem : System.IDisposable
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [AudioSystem](./frinkyengine.core.audio.audiosystem)<br>
Implements [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Constructors

### **AudioSystem(Scene)**

Creates a new scene audio system.

```csharp
public AudioSystem(Scene scene)
```

#### Parameters

`scene` [Scene](./frinkyengine.core.scene.scene)<br>
Owning scene.

## Methods

### **Update(Single)**

Updates listener state, source sync, and mixer state.

```csharp
public void Update(float dt)
```

#### Parameters

`dt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Frame delta time in seconds.

### **GetFrameStats()**

Returns latest frame audio diagnostics.

```csharp
public AudioFrameStats GetFrameStats()
```

#### Returns

[AudioFrameStats](./frinkyengine.core.audio.audioframestats)<br>

### **Dispose()**

```csharp
public void Dispose()
```
