# AudioPlayParams

Namespace: FrinkyEngine.Core.Audio

Optional playback overrides used when spawning an audio voice.

```csharp
public struct AudioPlayParams
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [AudioPlayParams](./frinkyengine.core.audio.audioplayparams)

## Properties

### **Volume**

Per-voice volume multiplier.

```csharp
public float Volume { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Pitch**

Per-voice pitch multiplier.

```csharp
public float Pitch { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Looping**

Whether the voice should loop.

```csharp
public bool Looping { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Spatialized**

Whether playback should be spatialized in 3D.

```csharp
public bool Spatialized { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Bus**

Destination bus.

```csharp
public AudioBusId Bus { get; set; }
```

#### Property Value

[AudioBusId](./frinkyengine.core.audio.audiobusid)<br>

### **AttenuationOverride**

Optional attenuation override.

```csharp
public Nullable<AudioAttenuationSettings> AttenuationOverride { get; set; }
```

#### Property Value

[Nullable&lt;AudioAttenuationSettings&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **StartTimeSeconds**

Start offset in seconds (streamed clips only).

```csharp
public float StartTimeSeconds { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **FadeInSeconds**

Fade-in time in seconds.

```csharp
public float FadeInSeconds { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Priority**

Priority used by voice stealing.

```csharp
public int Priority { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

## Constructors

### **AudioPlayParams()**

Creates playback params with engine defaults.

```csharp
AudioPlayParams()
```
