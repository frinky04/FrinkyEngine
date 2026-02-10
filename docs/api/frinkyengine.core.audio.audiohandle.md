# AudioHandle

Namespace: FrinkyEngine.Core.Audio

Opaque handle to a playing voice managed by the audio engine.

```csharp
public struct AudioHandle
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>
Implements [IEquatable&lt;AudioHandle&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.iequatable-1)<br>
Attributes [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **Id**

Unique voice identifier.

```csharp
public int Id { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Generation**

Handle generation used for stale-handle safety.

```csharp
public int Generation { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Invalid**

Invalid handle value.

```csharp
public static AudioHandle Invalid { get; }
```

#### Property Value

[AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

### **IsValid**

Returns `true` when this handle references a potentially live voice.

```csharp
public bool IsValid { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **AudioHandle(Int32, Int32)**

Opaque handle to a playing voice managed by the audio engine.

```csharp
AudioHandle(int Id, int Generation)
```

#### Parameters

`Id` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Unique voice identifier.

`Generation` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Handle generation used for stale-handle safety.

## Methods

### **ToString()**

```csharp
string ToString()
```

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GetHashCode()**

```csharp
int GetHashCode()
```

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Equals(Object)**

```csharp
bool Equals(object obj)
```

#### Parameters

`obj` [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Equals(AudioHandle)**

```csharp
bool Equals(AudioHandle other)
```

#### Parameters

`other` [AudioHandle](./frinkyengine.core.audio.audiohandle)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Deconstruct(Int32&, Int32&)**

```csharp
void Deconstruct(Int32& Id, Int32& Generation)
```

#### Parameters

`Id` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>

`Generation` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>
