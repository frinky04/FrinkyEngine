# ForwardPlusSettings

Namespace: FrinkyEngine.Core.Rendering

Configuration for the Forward+ tiled light culling system.

```csharp
public struct ForwardPlusSettings
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [ForwardPlusSettings](./frinkyengine.core.rendering.forwardplussettings)<br>
Implements [IEquatable&lt;ForwardPlusSettings&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.iequatable-1)<br>
Attributes [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Fields

### **DefaultTileSize**

Default tile size (16 pixels).

```csharp
public static int DefaultTileSize;
```

### **DefaultMaxLights**

Default maximum lights per frame (256).

```csharp
public static int DefaultMaxLights;
```

### **DefaultMaxLightsPerTile**

Default maximum lights per tile (64).

```csharp
public static int DefaultMaxLightsPerTile;
```

## Properties

### **TileSize**

Screen-space tile size in pixels (8–64, default 16).

```csharp
public int TileSize { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **MaxLights**

Maximum number of lights processed per frame (16–2048, default 256).

```csharp
public int MaxLights { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **MaxLightsPerTile**

Maximum lights assigned to any single tile (8–256, default 64).

```csharp
public int MaxLightsPerTile { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **Default**

Gets the default Forward+ settings.

```csharp
public static ForwardPlusSettings Default { get; }
```

#### Property Value

[ForwardPlusSettings](./frinkyengine.core.rendering.forwardplussettings)<br>

## Constructors

### **ForwardPlusSettings(Int32, Int32, Int32)**

Configuration for the Forward+ tiled light culling system.

```csharp
ForwardPlusSettings(int TileSize, int MaxLights, int MaxLightsPerTile)
```

#### Parameters

`TileSize` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Screen-space tile size in pixels (8–64, default 16).

`MaxLights` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Maximum number of lights processed per frame (16–2048, default 256).

`MaxLightsPerTile` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
Maximum lights assigned to any single tile (8–256, default 64).

## Methods

### **Normalize()**

Returns a copy with all values clamped to valid ranges, substituting defaults for out-of-range values.

```csharp
ForwardPlusSettings Normalize()
```

#### Returns

[ForwardPlusSettings](./frinkyengine.core.rendering.forwardplussettings)<br>
A normalized copy of these settings.

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

### **Equals(ForwardPlusSettings)**

```csharp
bool Equals(ForwardPlusSettings other)
```

#### Parameters

`other` [ForwardPlusSettings](./frinkyengine.core.rendering.forwardplussettings)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Deconstruct(Int32&, Int32&, Int32&)**

```csharp
void Deconstruct(Int32& TileSize, Int32& MaxLights, Int32& MaxLightsPerTile)
```

#### Parameters

`TileSize` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>

`MaxLights` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>

`MaxLightsPerTile` [Int32&](https://docs.microsoft.com/en-us/dotnet/api/system.int32&)<br>
