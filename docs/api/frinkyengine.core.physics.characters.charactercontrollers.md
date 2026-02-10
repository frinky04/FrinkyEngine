# CharacterControllers

Namespace: FrinkyEngine.Core.Physics.Characters

System that manages all the characters in a simulation. Responsible for updating movement constraints based on character goals and contact states.

```csharp
public class CharacterControllers : System.IDisposable
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [CharacterControllers](./frinkyengine.core.physics.characters.charactercontrollers)<br>
Implements [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable)

## Properties

### **Simulation**

Gets the simulation to which this set of chracters belongs.

```csharp
public Simulation Simulation { get; private set; }
```

#### Property Value

Simulation<br>

### **CharacterCount**

Gets the number of characters being controlled.

```csharp
public int CharacterCount { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

## Constructors

### **CharacterControllers(BufferPool, Int32, Int32)**

Creates a character controller systme.

```csharp
public CharacterControllers(BufferPool pool, int initialCharacterCapacity, int initialBodyHandleCapacity)
```

#### Parameters

`pool` BufferPool<br>
Pool to allocate resources from.

`initialCharacterCapacity` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Number of characters to initially allocate space for.

`initialBodyHandleCapacity` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Number of body handles to initially allocate space for in the body handle-&gt;character mapping.

## Methods

### **Initialize(Simulation)**

Caches the simulation associated with the characters.

```csharp
public void Initialize(Simulation simulation)
```

#### Parameters

`simulation` Simulation<br>
Simulation to be associated with the characters.

### **GetCharacterIndexForBodyHandle(Int32)**

Gets the current memory slot index of a character using its associated body handle.

```csharp
public int GetCharacterIndexForBodyHandle(int bodyHandle)
```

#### Parameters

`bodyHandle` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Body handle associated with the character to look up the index of.

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Index of the character associated with the body handle.

### **GetCharacterByIndex(Int32)**

Gets a reference to the character at the given memory slot index.

```csharp
public CharacterController& GetCharacterByIndex(int index)
```

#### Parameters

`index` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Index of the character to retrieve.

#### Returns

[CharacterController&](./frinkyengine.core.physics.characters.charactercontroller&)<br>
Reference to the character at the given memory slot index.

### **GetCharacterByBodyHandle(BodyHandle)**

Gets a reference to the character using the handle of the character's body.

```csharp
public CharacterController& GetCharacterByBodyHandle(BodyHandle bodyHandle)
```

#### Parameters

`bodyHandle` BodyHandle<br>
Body handle of the character to look up.

#### Returns

[CharacterController&](./frinkyengine.core.physics.characters.charactercontroller&)<br>
Reference to the character associated with the given body handle.

### **AllocateCharacter(BodyHandle)**

Allocates a character.

```csharp
public CharacterController& AllocateCharacter(BodyHandle bodyHandle)
```

#### Parameters

`bodyHandle` BodyHandle<br>
Body handle associated with the character.

#### Returns

[CharacterController&](./frinkyengine.core.physics.characters.charactercontroller&)<br>
Reference to the allocated character.

### **RemoveCharacterByIndex(Int32)**

Removes a character from the character controllers set by the character's index.

```csharp
public void RemoveCharacterByIndex(int characterIndex)
```

#### Parameters

`characterIndex` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Index of the character to remove.

### **RemoveCharacterByBodyHandle(BodyHandle)**

Removes a character from the character controllers set by the body handle associated with the character.

```csharp
public void RemoveCharacterByBodyHandle(BodyHandle bodyHandle)
```

#### Parameters

`bodyHandle` BodyHandle<br>
Body handle associated with the character to remove.

### **TryReportContacts&lt;TManifold&gt;(CollidablePair&, TManifold&, Int32, PairMaterialProperties&)**

Reports contacts about a collision to the character system. If the pair does not involve a character or there are no contacts, does nothing and returns false.

```csharp
public bool TryReportContacts<TManifold>(CollidablePair& pair, TManifold& manifold, int workerIndex, PairMaterialProperties& materialProperties)
```

#### Type Parameters

`TManifold`<br>

#### Parameters

`pair` CollidablePair&<br>
Pair of objects associated with the contact manifold.

`manifold` TManifold&<br>
Contact manifold between the colliding objects.

`workerIndex` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Index of the currently executing worker thread.

`materialProperties` PairMaterialProperties&<br>
Material properties for this pair. Will be modified if the pair involves a character.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
True if the pair involved a character pair and has contacts, false otherwise.

### **EnsureCapacity(Int32, Int32)**

Ensures that the internal structures of the character controllers system can handle the given number of characters and body handles, resizing if necessary.

```csharp
public void EnsureCapacity(int characterCapacity, int bodyHandleCapacity)
```

#### Parameters

`characterCapacity` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Minimum character capacity to require.

`bodyHandleCapacity` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Minimum number of body handles to allocate space for.

### **Resize(Int32, Int32)**

Resizes the internal structures of the character controllers system for the target sizes. Will not shrink below the currently active data size.

```csharp
public void Resize(int characterCapacity, int bodyHandleCapacity)
```

#### Parameters

`characterCapacity` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Target character capacity to allocate space for.

`bodyHandleCapacity` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Target number of body handles to allocate space for.

### **Dispose()**

Returns pool-allocated resources.

```csharp
public void Dispose()
```
