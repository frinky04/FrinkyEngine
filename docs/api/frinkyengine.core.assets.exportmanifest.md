# ExportManifest

Namespace: FrinkyEngine.Core.Assets

Metadata embedded in an exported game package, describing the project and its runtime settings.

```csharp
public class ExportManifest
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ExportManifest](./frinkyengine.core.assets.exportmanifest)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **ProjectName**

Name of the project.

```csharp
public string ProjectName { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **DefaultScene**

Asset-relative path to the default scene.

```csharp
public string DefaultScene { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GameAssembly**

File name of the game assembly DLL, if any.

```csharp
public string GameAssembly { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ProductName**

Product name for the exported game.

```csharp
public string ProductName { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **BuildVersion**

Build version string.

```csharp
public string BuildVersion { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **TargetFps**

Target frames per second.

```csharp
public Nullable<int> TargetFps { get; set; }
```

#### Property Value

[Nullable&lt;Int32&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **VSync**

Whether vertical sync is enabled.

```csharp
public Nullable<bool> VSync { get; set; }
```

#### Property Value

[Nullable&lt;Boolean&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **WindowTitle**

Window title bar text.

```csharp
public string WindowTitle { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **WindowWidth**

Initial window width in pixels.

```csharp
public Nullable<int> WindowWidth { get; set; }
```

#### Property Value

[Nullable&lt;Int32&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **WindowHeight**

Initial window height in pixels.

```csharp
public Nullable<int> WindowHeight { get; set; }
```

#### Property Value

[Nullable&lt;Int32&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **Resizable**

Whether the window is resizable.

```csharp
public Nullable<bool> Resizable { get; set; }
```

#### Property Value

[Nullable&lt;Boolean&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **Fullscreen**

Whether the game starts in fullscreen.

```csharp
public Nullable<bool> Fullscreen { get; set; }
```

#### Property Value

[Nullable&lt;Boolean&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **StartMaximized**

Whether the game window starts maximized.

```csharp
public Nullable<bool> StartMaximized { get; set; }
```

#### Property Value

[Nullable&lt;Boolean&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **ForwardPlusTileSize**

Forward+ tile size in pixels.

```csharp
public Nullable<int> ForwardPlusTileSize { get; set; }
```

#### Property Value

[Nullable&lt;Int32&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **ForwardPlusMaxLights**

Maximum total lights for the Forward+ renderer.

```csharp
public Nullable<int> ForwardPlusMaxLights { get; set; }
```

#### Property Value

[Nullable&lt;Int32&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **ForwardPlusMaxLightsPerTile**

Maximum lights per tile for the Forward+ renderer.

```csharp
public Nullable<int> ForwardPlusMaxLightsPerTile { get; set; }
```

#### Property Value

[Nullable&lt;Int32&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **PhysicsFixedTimestep**

```csharp
public Nullable<float> PhysicsFixedTimestep { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **PhysicsMaxSubstepsPerFrame**

```csharp
public Nullable<int> PhysicsMaxSubstepsPerFrame { get; set; }
```

#### Property Value

[Nullable&lt;Int32&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **PhysicsSolverVelocityIterations**

```csharp
public Nullable<int> PhysicsSolverVelocityIterations { get; set; }
```

#### Property Value

[Nullable&lt;Int32&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **PhysicsSolverSubsteps**

```csharp
public Nullable<int> PhysicsSolverSubsteps { get; set; }
```

#### Property Value

[Nullable&lt;Int32&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **PhysicsContactSpringFrequency**

```csharp
public Nullable<float> PhysicsContactSpringFrequency { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **PhysicsContactDampingRatio**

```csharp
public Nullable<float> PhysicsContactDampingRatio { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **PhysicsMaximumRecoveryVelocity**

```csharp
public Nullable<float> PhysicsMaximumRecoveryVelocity { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **PhysicsDefaultFriction**

```csharp
public Nullable<float> PhysicsDefaultFriction { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **PhysicsDefaultRestitution**

```csharp
public Nullable<float> PhysicsDefaultRestitution { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **PhysicsInterpolationEnabled**

```csharp
public Nullable<bool> PhysicsInterpolationEnabled { get; set; }
```

#### Property Value

[Nullable&lt;Boolean&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **AudioMasterVolume**

Master bus volume.

```csharp
public Nullable<float> AudioMasterVolume { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **AudioMusicVolume**

Music bus volume.

```csharp
public Nullable<float> AudioMusicVolume { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **AudioSfxVolume**

SFX bus volume.

```csharp
public Nullable<float> AudioSfxVolume { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **AudioUiVolume**

UI bus volume.

```csharp
public Nullable<float> AudioUiVolume { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **AudioVoiceVolume**

Voice bus volume.

```csharp
public Nullable<float> AudioVoiceVolume { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **AudioAmbientVolume**

Ambient bus volume.

```csharp
public Nullable<float> AudioAmbientVolume { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **AudioMaxVoices**

Maximum active voices.

```csharp
public Nullable<int> AudioMaxVoices { get; set; }
```

#### Property Value

[Nullable&lt;Int32&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **AudioDopplerScale**

Doppler scalar.

```csharp
public Nullable<float> AudioDopplerScale { get; set; }
```

#### Property Value

[Nullable&lt;Single&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **AudioEnableVoiceStealing**

Whether voice stealing is enabled.

```csharp
public Nullable<bool> AudioEnableVoiceStealing { get; set; }
```

#### Property Value

[Nullable&lt;Boolean&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **ScreenPercentage**

Screen percentage for resolution scaling.

```csharp
public Nullable<int> ScreenPercentage { get; set; }
```

#### Property Value

[Nullable&lt;Int32&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

## Constructors

### **ExportManifest()**

```csharp
public ExportManifest()
```

## Methods

### **ToJson()**

Serializes this manifest to a JSON string.

```csharp
public string ToJson()
```

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The JSON representation.

### **FromJson(String)**

Deserializes an export manifest from a JSON string.

```csharp
public static ExportManifest FromJson(string json)
```

#### Parameters

`json` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The JSON string to parse.

#### Returns

[ExportManifest](./frinkyengine.core.assets.exportmanifest)<br>
The deserialized manifest.
