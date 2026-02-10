# AudioFrameStats

Namespace: FrinkyEngine.Core.Audio

Per-frame audio diagnostics snapshot for profiling.

```csharp
public struct AudioFrameStats
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [AudioFrameStats](./frinkyengine.core.audio.audioframestats)<br>
Implements [IEquatable&lt;AudioFrameStats&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.iequatable-1)<br>
Attributes [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **Valid**

```csharp
public bool Valid { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ActiveVoices**

```csharp
public int ActiveVoices { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **VirtualizedVoices**

```csharp
public int VirtualizedVoices { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **StolenVoicesThisFrame**

```csharp
public int StolenVoicesThisFrame { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **StreamingVoices**

```csharp
public int StreamingVoices { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **UpdateTimeMs**

```csharp
public double UpdateTimeMs { get; set; }
```

#### Property Value

[Double](https://docs.microsoft.com/en-us/dotnet/api/system.double)<br>

## Constructors

### **AudioFrameStats(Boolean, Int32, Int32, Int32, Int32, Double)**

Per-frame audio diagnostics snapshot for profiling.

```csharp
AudioFrameStats(bool Valid, int ActiveVoices, int VirtualizedVoices, int StolenVoicesThisFrame, int StreamingVoices, double UpdateTimeMs)
```

#### Parameters

`Valid` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

`ActiveVoices` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`VirtualizedVoices` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`StolenVoicesThisFrame` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`StreamingVoices` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`UpdateTimeMs` [Double](https://docs.microsoft.com/en-us/dotnet/api/system.double)<br>

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

### **Equals(AudioFrameStats)**

```csharp
bool Equals(AudioFrameStats other)
```

#### Parameters

`other` [AudioFrameStats](./frinkyengine.core.audio.audioframestats)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Deconstruct(Boolean&, Int32&, Int32&, Int32&, Int32&, Double&)**

```csharp
void Deconstruct(Boolean& Valid, Int32& ActiveVoices, Int32& VirtualizedVoices, Int32& StolenVoicesThisFrame, Int32& StreamingVoices, Double& UpdateTimeMs)
```

#### Parameters

`Valid` [Boolean&](https://docs.microsoft.com/en-us/dotnet/api/system.boolean&)<br>

`ActiveVoices` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>

`VirtualizedVoices` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>

`StolenVoicesThisFrame` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>

`StreamingVoices` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>

`UpdateTimeMs` [Double&](https://docs.microsoft.com/en-us/dotnet/api/system.double&)<br>
