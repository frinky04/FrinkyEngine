# AudioAttenuationSettings

Namespace: FrinkyEngine.Core.Audio

Distance and panning settings used for spatialized audio playback.

```csharp
public struct AudioAttenuationSettings
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [AudioAttenuationSettings](./frinkyengine.core.audio.audioattenuationsettings)

## Properties

### **Default2D**

Default attenuation for 2D playback.

```csharp
public static AudioAttenuationSettings Default2D { get; }
```

#### Property Value

[AudioAttenuationSettings](./frinkyengine.core.audio.audioattenuationsettings)<br>

### **Default3D**

Default attenuation for 3D playback.

```csharp
public static AudioAttenuationSettings Default3D { get; }
```

#### Property Value

[AudioAttenuationSettings](./frinkyengine.core.audio.audioattenuationsettings)<br>

### **MinDistance**

Distance where attenuation begins.

```csharp
public float MinDistance { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **MaxDistance**

Distance where attenuation reaches silence.

```csharp
public float MaxDistance { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Rolloff**

Rolloff curve mode.

```csharp
public AudioRolloffMode Rolloff { get; set; }
```

#### Property Value

[AudioRolloffMode](./frinkyengine.core.audio.audiorolloffmode)<br>

### **SpatialBlend**

0 = 2D pan only, 1 = fully 3D spatialized.

```csharp
public float SpatialBlend { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **PanStereo**

Stereo pan for 2D playback, from -1 (left) to +1 (right).

```csharp
public float PanStereo { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

## Constructors

### **AudioAttenuationSettings()**

Creates attenuation settings with engine defaults.

```csharp
AudioAttenuationSettings()
```

## Methods

### **Normalize()**

Clamps values to safe ranges.

```csharp
void Normalize()
```

### **EvaluateVolume(Single)**

Computes distance-based gain from 0..1.

```csharp
float EvaluateVolume(float distance)
```

#### Parameters

`distance` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Distance from listener to source.

#### Returns

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Gain multiplier in range 0..1.

### **EvaluatePan(Vector3, Vector3, Vector3)**

Computes stereo pan from listener/source transforms.

```csharp
float EvaluatePan(Vector3 listenerPosition, Vector3 listenerRight, Vector3 sourcePosition)
```

#### Parameters

`listenerPosition` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Listener world position.

`listenerRight` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Listener right unit axis.

`sourcePosition` [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>
Source world position.

#### Returns

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Pan in range -1..1.
