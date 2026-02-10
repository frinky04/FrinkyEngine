# UiImageHandle

Namespace: FrinkyEngine.Core.UI

Opaque texture handle used by the UI wrapper API.

```csharp
public struct UiImageHandle
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [UiImageHandle](./frinkyengine.core.ui.uiimagehandle)<br>
Implements [IEquatable&lt;UiImageHandle&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.iequatable-1)<br>
Attributes [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **TextureId**

Underlying GPU texture identifier.

```csharp
public uint TextureId { get; set; }
```

#### Property Value

[UInt32](https://docs.microsoft.com/en-us/dotnet/api/system.uint32)<br>

### **IsValid**

Gets whether this handle references a valid texture.

```csharp
public bool IsValid { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **UiImageHandle(UInt32)**

Opaque texture handle used by the UI wrapper API.

```csharp
UiImageHandle(uint TextureId)
```

#### Parameters

`TextureId` [UInt32](https://docs.microsoft.com/en-us/dotnet/api/system.uint32)<br>
Underlying GPU texture identifier.

## Methods

### **FromTexture(Texture2D)**

Creates a UI image handle from a Raylib texture.

```csharp
UiImageHandle FromTexture(Texture2D texture)
```

#### Parameters

`texture` Texture2D<br>
Source texture.

#### Returns

[UiImageHandle](./frinkyengine.core.ui.uiimagehandle)<br>
A UI image handle for the texture.

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

### **Equals(UiImageHandle)**

```csharp
bool Equals(UiImageHandle other)
```

#### Parameters

`other` [UiImageHandle](./frinkyengine.core.ui.uiimagehandle)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Deconstruct(UInt32&)**

```csharp
void Deconstruct(UInt32& TextureId)
```

#### Parameters

`TextureId` [UInt32&](https://docs.microsoft.com/en-us/dotnet/api/system.uint32&)<br>
