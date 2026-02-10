# PhysicsProjectSettings

Namespace: FrinkyEngine.Core.Physics

Project-level physics settings (shared across all scenes).
 Populated from [RuntimeProjectSettings](./frinkyengine.core.assets.runtimeprojectsettings) at startup.

```csharp
public class PhysicsProjectSettings
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PhysicsProjectSettings](./frinkyengine.core.physics.physicsprojectsettings)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **Current**

Global singleton instance, set by the editor or runtime at startup.

```csharp
public static PhysicsProjectSettings Current { get; set; }
```

#### Property Value

[PhysicsProjectSettings](./frinkyengine.core.physics.physicsprojectsettings)<br>

### **FixedTimestep**

Fixed simulation step duration, in seconds.

```csharp
public float FixedTimestep { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **MaxSubstepsPerFrame**

Maximum number of simulation steps allowed for one frame.

```csharp
public int MaxSubstepsPerFrame { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **SolverVelocityIterations**

Solver velocity iterations per substep.

```csharp
public int SolverVelocityIterations { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **SolverSubsteps**

Solver substep count.

```csharp
public int SolverSubsteps { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **ContactSpringFrequency**

Contact spring angular frequency.

```csharp
public float ContactSpringFrequency { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **ContactDampingRatio**

Contact spring damping ratio.

```csharp
public float ContactDampingRatio { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **MaximumRecoveryVelocity**

Recovery velocity cap before restitution scaling.

```csharp
public float MaximumRecoveryVelocity { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **DefaultFriction**

Default friction for colliders without overrides.

```csharp
public float DefaultFriction { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **DefaultRestitution**

Default restitution for colliders without overrides.

```csharp
public float DefaultRestitution { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **InterpolationEnabled**

Enables visual interpolation for eligible rigidbodies.

```csharp
public bool InterpolationEnabled { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **PhysicsProjectSettings()**

```csharp
public PhysicsProjectSettings()
```

## Methods

### **ApplyFrom(RuntimeProjectSettings)**

Populates [PhysicsProjectSettings.Current](./frinkyengine.core.physics.physicsprojectsettings#current) from the given runtime project settings.

```csharp
public static void ApplyFrom(RuntimeProjectSettings runtime)
```

#### Parameters

`runtime` [RuntimeProjectSettings](./frinkyengine.core.assets.runtimeprojectsettings)<br>

### **ApplyFrom(ExportManifest)**

Populates [PhysicsProjectSettings.Current](./frinkyengine.core.physics.physicsprojectsettings#current) from an export manifest's physics settings.

```csharp
public static void ApplyFrom(ExportManifest manifest)
```

#### Parameters

`manifest` [ExportManifest](./frinkyengine.core.assets.exportmanifest)<br>

### **Normalize()**

Ensures all settings remain in safe ranges.

```csharp
public void Normalize()
```
