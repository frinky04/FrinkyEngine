# RuntimeProjectSettings

Namespace: FrinkyEngine.Core.Assets

Settings that control runtime behavior (window, rendering, performance).

```csharp
public class RuntimeProjectSettings
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [RuntimeProjectSettings](./frinkyengine.core.assets.runtimeprojectsettings)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **TargetFps**

Target frames per second (0 for uncapped, otherwise clamped to 30-500; defaults to 120).

```csharp
public int TargetFps { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **VSync**

Whether vertical sync is enabled.

```csharp
public bool VSync { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **WindowTitle**

Title displayed in the window title bar.

```csharp
public string WindowTitle { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **WindowWidth**

Initial window width in pixels.

```csharp
public int WindowWidth { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **WindowHeight**

Initial window height in pixels.

```csharp
public int WindowHeight { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Resizable**

Whether the window can be resized by the user.

```csharp
public bool Resizable { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Fullscreen**

Whether the game starts in fullscreen mode.

```csharp
public bool Fullscreen { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **StartMaximized**

Whether the game window starts maximized.

```csharp
public bool StartMaximized { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **StartupSceneOverride**

Optional scene path that overrides the project's default scene on startup.

```csharp
public string StartupSceneOverride { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ForwardPlusTileSize**

Forward+ tile size in pixels (clamped to 8–64, defaults to 16).

```csharp
public int ForwardPlusTileSize { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **ForwardPlusMaxLights**

Maximum total lights processed by the Forward+ renderer (clamped to 16–2048, defaults to 256).

```csharp
public int ForwardPlusMaxLights { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **ForwardPlusMaxLightsPerTile**

Maximum lights assigned to a single tile (clamped to 8–256, defaults to 64).

```csharp
public int ForwardPlusMaxLightsPerTile { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **PhysicsFixedTimestep**

Fixed simulation step duration in seconds (clamped to 1/240–1/15, defaults to 1/60).

```csharp
public float PhysicsFixedTimestep { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **PhysicsMaxSubstepsPerFrame**

Maximum simulation steps per frame (clamped to 1–16, defaults to 4).

```csharp
public int PhysicsMaxSubstepsPerFrame { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **PhysicsSolverVelocityIterations**

Solver velocity iterations per substep (clamped to 1–32, defaults to 8).

```csharp
public int PhysicsSolverVelocityIterations { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **PhysicsSolverSubsteps**

Solver substep count (clamped to 1–8, defaults to 1).

```csharp
public int PhysicsSolverSubsteps { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **PhysicsContactSpringFrequency**

Contact spring angular frequency (clamped to 1–300, defaults to 30).

```csharp
public float PhysicsContactSpringFrequency { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **PhysicsContactDampingRatio**

Contact spring damping ratio (clamped to 0–10, defaults to 1).

```csharp
public float PhysicsContactDampingRatio { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **PhysicsMaximumRecoveryVelocity**

Recovery velocity cap before restitution scaling (clamped to 0–100, defaults to 2).

```csharp
public float PhysicsMaximumRecoveryVelocity { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **PhysicsDefaultFriction**

Default friction for colliders without overrides (clamped to 0–10, defaults to 0.8).

```csharp
public float PhysicsDefaultFriction { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **PhysicsDefaultRestitution**

Default restitution for colliders without overrides (clamped to 0–1, defaults to 0).

```csharp
public float PhysicsDefaultRestitution { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **PhysicsInterpolationEnabled**

Enables visual interpolation for eligible dynamic rigidbodies.

```csharp
public bool PhysicsInterpolationEnabled { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **AudioMasterVolume**

Master bus volume (clamped to 0–2, defaults to 1).

```csharp
public float AudioMasterVolume { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **AudioMusicVolume**

Music bus volume (clamped to 0–2, defaults to 1).

```csharp
public float AudioMusicVolume { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **AudioSfxVolume**

SFX bus volume (clamped to 0–2, defaults to 1).

```csharp
public float AudioSfxVolume { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **AudioUiVolume**

UI bus volume (clamped to 0–2, defaults to 1).

```csharp
public float AudioUiVolume { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **AudioVoiceVolume**

Voice bus volume (clamped to 0–2, defaults to 1).

```csharp
public float AudioVoiceVolume { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **AudioAmbientVolume**

Ambient bus volume (clamped to 0–2, defaults to 1).

```csharp
public float AudioAmbientVolume { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **AudioMaxVoices**

Maximum active voices (clamped to 16–512, defaults to 128).

```csharp
public int AudioMaxVoices { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **AudioDopplerScale**

Doppler scalar reserved for advanced spatialization (clamped to 0–10, defaults to 1).

```csharp
public float AudioDopplerScale { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **AudioEnableVoiceStealing**

Allows low-priority voice stealing when the voice budget is full.

```csharp
public bool AudioEnableVoiceStealing { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ScreenPercentage**

Screen percentage (10-200, defaults to 100). Below 100 renders at lower resolution for a pixelated look.

```csharp
public int ScreenPercentage { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

## Constructors

### **RuntimeProjectSettings()**

```csharp
public RuntimeProjectSettings()
```
