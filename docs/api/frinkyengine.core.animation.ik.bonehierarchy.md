# BoneHierarchy

Namespace: FrinkyEngine.Core.Animation.IK

Caches bone hierarchy data from a Raylib  for efficient IK lookups.

```csharp
public sealed class BoneHierarchy
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [BoneHierarchy](./frinkyengine.core.animation.ik.bonehierarchy)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **BoneCount**

Number of bones in the skeleton.

```csharp
public int BoneCount { get; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **BoneNames**

Bone names indexed by bone index.

```csharp
public String[] BoneNames { get; }
```

#### Property Value

[String[]](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ParentIndices**

Parent index for each bone (-1 for root bones).

```csharp
public Int32[] ParentIndices { get; }
```

#### Property Value

[Int32[]](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **BindPoseLocal**

Bind-pose transforms as stored in the Raylib model (typically model-space for supported formats).

```csharp
public ValueTuple`3[] BindPoseLocal { get; }
```

#### Property Value

[ValueTuple`3[]](https://docs.microsoft.com/en-us/dotnet/api/system.valuetuple-3)<br>

## Constructors

### **BoneHierarchy(Model)**

Creates a [BoneHierarchy](./frinkyengine.core.animation.ik.bonehierarchy) by reading bone data from a Raylib model.

```csharp
public BoneHierarchy(Model model)
```

#### Parameters

`model` Model<br>

## Methods

### **FindBone(String)**

Finds a bone by name. Returns -1 if not found.

```csharp
public int FindBone(string name)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **GetChain(Int32, Int32)**

Walks parent pointers from `endBone` up to `rootBone`,
 returning an ordered chain from root to end (inclusive). Returns null if no path exists.

```csharp
public Int32[] GetChain(int rootBone, int endBone)
```

#### Parameters

`rootBone` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`endBone` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

#### Returns

[Int32[]](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **GetBoneNamesForDropdown()**

Returns bone names for use in a dropdown, prefixed with "(none)" at index 0.
 Bone index = dropdown index - 1.

```csharp
public String[] GetBoneNamesForDropdown()
```

#### Returns

[String[]](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
