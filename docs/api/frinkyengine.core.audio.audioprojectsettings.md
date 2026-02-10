# AudioProjectSettings

Namespace: FrinkyEngine.Core.Audio

Project-level audio settings applied at runtime startup.

```csharp
public class AudioProjectSettings
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [AudioProjectSettings](./frinkyengine.core.audio.audioprojectsettings)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Current**

Global singleton instance.

```csharp
public static AudioProjectSettings Current { get; private set; }
```

#### Property Value

[AudioProjectSettings](./frinkyengine.core.audio.audioprojectsettings)<br>

### **MasterVolume**

Master bus volume.

```csharp
public float MasterVolume { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **MusicVolume**

Music bus volume.

```csharp
public float MusicVolume { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **SfxVolume**

Sound effects bus volume.

```csharp
public float SfxVolume { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **UiVolume**

UI bus volume.

```csharp
public float UiVolume { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **VoiceVolume**

Voice bus volume.

```csharp
public float VoiceVolume { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **AmbientVolume**

Ambient bus volume.

```csharp
public float AmbientVolume { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **MaxVoices**

Maximum simultaneously active voices.

```csharp
public int MaxVoices { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **DopplerScale**

Reserved scalar for Doppler-like effects.

```csharp
public float DopplerScale { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **EnableVoiceStealing**

Whether low-priority voices can be replaced when voice budget is full.

```csharp
public bool EnableVoiceStealing { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **AudioProjectSettings()**

```csharp
public AudioProjectSettings()
```

## Methods

### **ApplyFrom(RuntimeProjectSettings)**

Applies values from runtime project settings.

```csharp
public static void ApplyFrom(RuntimeProjectSettings runtime)
```

#### Parameters

`runtime` [RuntimeProjectSettings](./frinkyengine.core.assets.runtimeprojectsettings)<br>

### **ApplyFrom(ExportManifest)**

Applies values from export manifest settings.

```csharp
public static void ApplyFrom(ExportManifest manifest)
```

#### Parameters

`manifest` [ExportManifest](./frinkyengine.core.assets.exportmanifest)<br>

### **GetBusVolume(AudioBusId)**

Returns the configured base volume for a bus.

```csharp
public float GetBusVolume(AudioBusId bus)
```

#### Parameters

`bus` [AudioBusId](./frinkyengine.core.audio.audiobusid)<br>

#### Returns

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **SetBusVolume(AudioBusId, Single)**

Sets the configured base volume for a bus.

```csharp
public void SetBusVolume(AudioBusId bus, float volume)
```

#### Parameters

`bus` [AudioBusId](./frinkyengine.core.audio.audiobusid)<br>

`volume` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Normalize()**

Clamps settings into safe ranges.

```csharp
public void Normalize()
```
