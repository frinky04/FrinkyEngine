# StaticCharacterMotionConstraint

Namespace: FrinkyEngine.Core.Physics.Characters

Description of a character motion constraint where the support is static.

```csharp
public struct StaticCharacterMotionConstraint
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [StaticCharacterMotionConstraint](./frinkyengine.core.physics.characters.staticcharactermotionconstraint)<br>
Implements IOneBodyConstraintDescription&lt;StaticCharacterMotionConstraint&gt;, IConstraintDescription&lt;StaticCharacterMotionConstraint&gt;

## Fields

### **MaximumHorizontalForce**

Maximum force that the horizontal motion constraint can apply to reach the current velocity goal.

```csharp
public float MaximumHorizontalForce;
```

### **MaximumVerticalForce**

Maximum force that the vertical motion constraint can apply to fight separation.

```csharp
public float MaximumVerticalForce;
```

### **TargetVelocity**

Target horizontal velocity in terms of the basis X and -Z axes.

```csharp
public Vector2 TargetVelocity;
```

### **Depth**

Depth of the supporting contact. The vertical motion constraint permits separating velocity if, after a frame, the objects will still be touching.

```csharp
public float Depth;
```

### **SurfaceBasis**

Stores the quaternion-packed orthonormal basis for the motion constraint. When expanded into a matrix, X and Z will represent the Right and Backward directions respectively. Y will represent Up.
 In other words, a target tangential velocity of (4, 2) will result in a goal velocity of 4 along the (1, 0, 0) * Basis direction and a goal velocity of 2 along the (0, 0, -1) * Basis direction.
 All motion moving along the (0, 1, 0) * Basis axis will be fought against by the vertical motion constraint.

```csharp
public Quaternion SurfaceBasis;
```

### **OffsetFromCharacterToSupportPoint**

World space offset from the character's center to apply impulses at.

```csharp
public Vector3 OffsetFromCharacterToSupportPoint;
```

## Properties

### **ConstraintTypeId**

Gets the constraint type id that this description is associated with.

```csharp
public int ConstraintTypeId { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **TypeProcessorType**

Gets the TypeProcessor type that is associated with this description.

```csharp
public Type TypeProcessorType { get; }
```

#### Property Value

[Type](https://docs.microsoft.com/en-us/dotnet/api/system.type)<br>

## Methods

### **ApplyDescription(TypeBatch&, Int32, Int32)**

```csharp
void ApplyDescription(TypeBatch& batch, int bundleIndex, int innerIndex)
```

#### Parameters

`batch` TypeBatch&<br>

`bundleIndex` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`innerIndex` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **BuildDescription(TypeBatch&, Int32, Int32, StaticCharacterMotionConstraint&)**

```csharp
void BuildDescription(TypeBatch& batch, int bundleIndex, int innerIndex, StaticCharacterMotionConstraint& description)
```

#### Parameters

`batch` TypeBatch&<br>

`bundleIndex` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`innerIndex` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`description` [StaticCharacterMotionConstraint&](./frinkyengine.core.physics.characters.staticcharactermotionconstraint&)<br>
