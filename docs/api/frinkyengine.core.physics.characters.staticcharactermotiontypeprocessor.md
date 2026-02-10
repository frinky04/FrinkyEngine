# StaticCharacterMotionTypeProcessor

Namespace: FrinkyEngine.Core.Physics.Characters

```csharp
public class StaticCharacterMotionTypeProcessor : BepuPhysics.Constraints.OneBodyTypeProcessor`5[[FrinkyEngine.Core.Physics.Characters.StaticCharacterMotionPrestep, FrinkyEngine.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null],[FrinkyEngine.Core.Physics.Characters.CharacterMotionAccumulatedImpulse, FrinkyEngine.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null],[FrinkyEngine.Core.Physics.Characters.StaticCharacterMotionFunctions, FrinkyEngine.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null],[BepuPhysics.Constraints.AccessAll, BepuPhysics, Version=2.4.0.0, Culture=neutral, PublicKeyToken=9345ce38ee48a1cd],[BepuPhysics.Constraints.AccessAll, BepuPhysics, Version=2.4.0.0, Culture=neutral, PublicKeyToken=9345ce38ee48a1cd]]
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → TypeProcessor → TypeProcessor&lt;Vector&lt;Int32&gt;, StaticCharacterMotionPrestep, CharacterMotionAccumulatedImpulse&gt; → OneBodyTypeProcessor&lt;StaticCharacterMotionPrestep, CharacterMotionAccumulatedImpulse, StaticCharacterMotionFunctions, AccessAll, AccessAll&gt; → [StaticCharacterMotionTypeProcessor](./frinkyengine.core.physics.characters.staticcharactermotiontypeprocessor)

## Fields

### **BatchTypeId**

Simulation-wide unique id for the character motion constraint. Every type has needs a unique compile time id; this is a little bit annoying to guarantee given that there is no central
 registry of all types that can exist (custom ones, like this one, can always be created), but having it be constant helps simplify and optimize its internal usage.

```csharp
public static int BatchTypeId;
```

### **typeId**

```csharp
protected int typeId;
```

### **bodiesPerConstraint**

```csharp
protected int bodiesPerConstraint;
```

## Properties

### **InternalBodiesPerConstraint**

```csharp
protected int InternalBodiesPerConstraint { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **RequiresIncrementalSubstepUpdates**

```csharp
public bool RequiresIncrementalSubstepUpdates { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **InternalConstrainedDegreesOfFreedom**

```csharp
protected int InternalConstrainedDegreesOfFreedom { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **TypeId**

```csharp
public int TypeId { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **BodiesPerConstraint**

```csharp
public int BodiesPerConstraint { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **ConstrainedDegreesOfFreedom**

```csharp
public int ConstrainedDegreesOfFreedom { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

## Constructors

### **StaticCharacterMotionTypeProcessor()**

```csharp
public StaticCharacterMotionTypeProcessor()
```
