# Audio

The audio system supports UE-style static gameplay helpers plus ECS components for 2D/3D sound.

## Static API

Play sounds from any component without needing a reference to an audio source:

```csharp
// 2D sound (not spatialized)
Audio.PlaySound2D("Sounds/click.wav");

// 3D sound at a world position
Audio.PlaySoundAtLocation("Sounds/explosion.wav", worldPosition);

// 3D sound attached to an entity (follows it)
Audio.SpawnSoundAttached("Sounds/engine.wav", entity);

// With parameters
Audio.PlaySound2D("Sounds/music.wav", new AudioPlayParams
{
    Bus = AudioBus.Music,
    Volume = 0.8f,
    Pitch = 1.0f,
    Looping = true
});

// Stop a playing sound
Audio.Stop(handle, fadeOutSeconds: 0.5f);
```

### Volume Control

```csharp
Audio.SetBusVolume(AudioBus.Music, 0.5f);
Audio.SetBusMuted(AudioBus.Sfx, true);
```

## Components

### AudioSourceComponent

Attach to an entity for persistent sound emitters:

| Property | Default | Description |
|----------|---------|-------------|
| `SoundPath` | — | Path to the audio file |
| `PlayOnStart` | false | Auto-play when the scene starts |
| `Spatialized` | false | Enable 3D positioning |
| `Looping` | false | Loop playback |
| `Bus` | Sfx | Mixer bus routing |
| `Volume` | 1.0 | Volume multiplier |
| `Pitch` | 1.0 | Pitch multiplier |
| `Attenuation` | — | Distance attenuation settings |

### AudioListenerComponent

Marks the entity as the active audio listener for 3D spatialization:

| Property | Default | Description |
|----------|---------|-------------|
| `IsPrimary` | true | Whether this is the active listener |
| `MasterVolumeScale` | 1.0 | Global volume multiplier |

If no `AudioListenerComponent` exists in the scene, the main camera is used as the fallback listener.

## Mixer Buses

Audio is routed through mixer buses for category-based volume control:

| Bus | Typical Use |
|-----|-------------|
| `Master` | Overall volume (all other buses feed into this) |
| `Music` | Background music |
| `Sfx` | Sound effects |
| `Ui` | Menu and UI sounds |
| `Voice` | Dialogue and voice lines |
| `Ambient` | Environmental audio |

Bus volumes can be configured in `project_settings.json` (see [Project Settings](project-settings.md)) or at runtime via `Audio.SetBusVolume()`.

## AudioPlayParams

Optional parameters for static API calls:

| Property | Default | Description |
|----------|---------|-------------|
| `Bus` | Sfx | Mixer bus |
| `Volume` | 1.0 | Volume multiplier |
| `Pitch` | 1.0 | Pitch multiplier |
| `Looping` | false | Loop playback |
| `Attenuation` | — | Distance attenuation override |

## AudioAttenuationSettings

Controls how 3D sounds fall off with distance:

| Property | Description |
|----------|-------------|
| `MinDistance` | Distance at which attenuation begins |
| `MaxDistance` | Distance at which the sound is silent |
| `RolloffFactor` | How quickly volume decreases with distance |

## Runtime Notes

- Missing audio assets fail safe with warnings and do not crash the runtime
- `AudioSourceComponent` can auto-play in `Start` via `PlayOnStart = true`
- The listener fallback to main camera means 3D audio works even without an explicit `AudioListenerComponent`

## See Also

- [Audio Roadmap](roadmaps/audio_roadmap.md) — planned audio features
