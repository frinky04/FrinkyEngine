# StaticCharacterMotionFunctions

Namespace: FrinkyEngine.Core.Physics.Characters

```csharp
public struct StaticCharacterMotionFunctions
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [StaticCharacterMotionFunctions](./frinkyengine.core.physics.characters.staticcharactermotionfunctions)<br>
Implements IOneBodyConstraintFunctions&lt;StaticCharacterMotionPrestep, CharacterMotionAccumulatedImpulse&gt;

## Properties

### **RequiresIncrementalSubstepUpdates**

```csharp
public bool RequiresIncrementalSubstepUpdates { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Methods

### **WarmStart(Vector3Wide&, QuaternionWide&, BodyInertiaWide&, StaticCharacterMotionPrestep&, CharacterMotionAccumulatedImpulse&, BodyVelocityWide&)**

```csharp
void WarmStart(Vector3Wide& positionA, QuaternionWide& orientationA, BodyInertiaWide& inertiaA, StaticCharacterMotionPrestep& prestep, CharacterMotionAccumulatedImpulse& accumulatedImpulses, BodyVelocityWide& velocityA)
```

#### Parameters

`positionA` Vector3Wide&<br>

`orientationA` QuaternionWide&<br>

`inertiaA` BodyInertiaWide&<br>

`prestep` [StaticCharacterMotionPrestep&](./frinkyengine.core.physics.characters.staticcharactermotionprestep&)<br>

`accumulatedImpulses` [CharacterMotionAccumulatedImpulse&](./frinkyengine.core.physics.characters.charactermotionaccumulatedimpulse&)<br>

`velocityA` BodyVelocityWide&<br>

### **Solve(Vector3Wide&, QuaternionWide&, BodyInertiaWide&, Single, Single, StaticCharacterMotionPrestep&, CharacterMotionAccumulatedImpulse&, BodyVelocityWide&)**

```csharp
void Solve(Vector3Wide& positionA, QuaternionWide& orientationA, BodyInertiaWide& inertiaA, float dt, float inverseDt, StaticCharacterMotionPrestep& prestep, CharacterMotionAccumulatedImpulse& accumulatedImpulses, BodyVelocityWide& velocityA)
```

#### Parameters

`positionA` Vector3Wide&<br>

`orientationA` QuaternionWide&<br>

`inertiaA` BodyInertiaWide&<br>

`dt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`inverseDt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`prestep` [StaticCharacterMotionPrestep&](./frinkyengine.core.physics.characters.staticcharactermotionprestep&)<br>

`accumulatedImpulses` [CharacterMotionAccumulatedImpulse&](./frinkyengine.core.physics.characters.charactermotionaccumulatedimpulse&)<br>

`velocityA` BodyVelocityWide&<br>

### **IncrementallyUpdateForSubstep(Vector`1&, BodyVelocityWide&, StaticCharacterMotionPrestep&)**

```csharp
void IncrementallyUpdateForSubstep(Vector`1& dt, BodyVelocityWide& velocityA, StaticCharacterMotionPrestep& prestep)
```

#### Parameters

`dt` [Vector`1&](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector-1&)<br>

`velocityA` BodyVelocityWide&<br>

`prestep` [StaticCharacterMotionPrestep&](./frinkyengine.core.physics.characters.staticcharactermotionprestep&)<br>
