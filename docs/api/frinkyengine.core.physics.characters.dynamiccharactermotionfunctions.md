# DynamicCharacterMotionFunctions

Namespace: FrinkyEngine.Core.Physics.Characters

```csharp
public struct DynamicCharacterMotionFunctions
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [DynamicCharacterMotionFunctions](./frinkyengine.core.physics.characters.dynamiccharactermotionfunctions)<br>
Implements ITwoBodyConstraintFunctions&lt;DynamicCharacterMotionPrestep, CharacterMotionAccumulatedImpulse&gt;

## Properties

### **RequiresIncrementalSubstepUpdates**

```csharp
public bool RequiresIncrementalSubstepUpdates { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Methods

### **WarmStart(Vector3Wide&, QuaternionWide&, BodyInertiaWide&, Vector3Wide&, QuaternionWide&, BodyInertiaWide&, DynamicCharacterMotionPrestep&, CharacterMotionAccumulatedImpulse&, BodyVelocityWide&, BodyVelocityWide&)**

```csharp
void WarmStart(Vector3Wide& positionA, QuaternionWide& orientationA, BodyInertiaWide& inertiaA, Vector3Wide& positionB, QuaternionWide& orientationB, BodyInertiaWide& inertiaB, DynamicCharacterMotionPrestep& prestep, CharacterMotionAccumulatedImpulse& accumulatedImpulses, BodyVelocityWide& velocityA, BodyVelocityWide& velocityB)
```

#### Parameters

`positionA` Vector3Wide&<br>

`orientationA` QuaternionWide&<br>

`inertiaA` BodyInertiaWide&<br>

`positionB` Vector3Wide&<br>

`orientationB` QuaternionWide&<br>

`inertiaB` BodyInertiaWide&<br>

`prestep` [DynamicCharacterMotionPrestep&](./frinkyengine.core.physics.characters.dynamiccharactermotionprestep&)<br>

`accumulatedImpulses` [CharacterMotionAccumulatedImpulse&](./frinkyengine.core.physics.characters.charactermotionaccumulatedimpulse&)<br>

`velocityA` BodyVelocityWide&<br>

`velocityB` BodyVelocityWide&<br>

### **Solve(Vector3Wide&, QuaternionWide&, BodyInertiaWide&, Vector3Wide&, QuaternionWide&, BodyInertiaWide&, Single, Single, DynamicCharacterMotionPrestep&, CharacterMotionAccumulatedImpulse&, BodyVelocityWide&, BodyVelocityWide&)**

```csharp
void Solve(Vector3Wide& positionA, QuaternionWide& orientationA, BodyInertiaWide& inertiaA, Vector3Wide& positionB, QuaternionWide& orientationB, BodyInertiaWide& inertiaB, float dt, float inverseDt, DynamicCharacterMotionPrestep& prestep, CharacterMotionAccumulatedImpulse& accumulatedImpulses, BodyVelocityWide& velocityA, BodyVelocityWide& velocityB)
```

#### Parameters

`positionA` Vector3Wide&<br>

`orientationA` QuaternionWide&<br>

`inertiaA` BodyInertiaWide&<br>

`positionB` Vector3Wide&<br>

`orientationB` QuaternionWide&<br>

`inertiaB` BodyInertiaWide&<br>

`dt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`inverseDt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`prestep` [DynamicCharacterMotionPrestep&](./frinkyengine.core.physics.characters.dynamiccharactermotionprestep&)<br>

`accumulatedImpulses` [CharacterMotionAccumulatedImpulse&](./frinkyengine.core.physics.characters.charactermotionaccumulatedimpulse&)<br>

`velocityA` BodyVelocityWide&<br>

`velocityB` BodyVelocityWide&<br>

### **IncrementallyUpdateForSubstep(Vector`1&, BodyVelocityWide&, BodyVelocityWide&, DynamicCharacterMotionPrestep&)**

```csharp
void IncrementallyUpdateForSubstep(Vector`1& dt, BodyVelocityWide& velocityA, BodyVelocityWide& velocityB, DynamicCharacterMotionPrestep& prestep)
```

#### Parameters

`dt` [Vector`1&](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector-1&)<br>

`velocityA` BodyVelocityWide&<br>

`velocityB` BodyVelocityWide&<br>

`prestep` [DynamicCharacterMotionPrestep&](./frinkyengine.core.physics.characters.dynamiccharactermotionprestep&)<br>
