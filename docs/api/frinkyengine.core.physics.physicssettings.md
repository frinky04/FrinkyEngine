# PhysicsSettings

Namespace: FrinkyEngine.Core.Physics

Scene-level physics configuration (gravity only; other physics settings live in PhysicsProjectSettings).

```csharp
public class PhysicsSettings
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PhysicsSettings](./frinkyengine.core.physics.physicssettings)

## Properties

### **Gravity**

Gravity acceleration applied to dynamic rigidbodies.

```csharp
public Vector3 Gravity { get; set; }
```

#### Property Value

[Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3)<br>

## Constructors

### **PhysicsSettings()**

```csharp
public PhysicsSettings()
```

## Methods

### **Normalize()**

Ensures all settings remain in safe ranges.

```csharp
public void Normalize()
```

### **Clone()**

Returns a deep copy of this settings object.

```csharp
public PhysicsSettings Clone()
```

#### Returns

[PhysicsSettings](./frinkyengine.core.physics.physicssettings)<br>
