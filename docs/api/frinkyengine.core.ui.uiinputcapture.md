# UiInputCapture

Namespace: FrinkyEngine.Core.UI

Indicates whether UI wants to capture player input for the current frame.

```csharp
public struct UiInputCapture
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [UiInputCapture](./frinkyengine.core.ui.uiinputcapture)<br>
Implements [IEquatable&lt;UiInputCapture&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.iequatable-1)<br>
Attributes [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **WantsMouse**

When `true`, UI is consuming mouse input.

```csharp
public bool WantsMouse { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **WantsKeyboard**

When `true`, UI is consuming keyboard input.

```csharp
public bool WantsKeyboard { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **WantsTextInput**

When `true`, UI is actively receiving text input.

```csharp
public bool WantsTextInput { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Any**

Gets whether any capture flag is active.

```csharp
public bool Any { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **None**

Gets a value where no input is captured.

```csharp
public static UiInputCapture None { get; }
```

#### Property Value

[UiInputCapture](./frinkyengine.core.ui.uiinputcapture)<br>

## Constructors

### **UiInputCapture(Boolean, Boolean, Boolean)**

Indicates whether UI wants to capture player input for the current frame.

```csharp
UiInputCapture(bool WantsMouse, bool WantsKeyboard, bool WantsTextInput)
```

#### Parameters

`WantsMouse` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, UI is consuming mouse input.

`WantsKeyboard` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, UI is consuming keyboard input.

`WantsTextInput` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
When `true`, UI is actively receiving text input.

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

### **Equals(UiInputCapture)**

```csharp
bool Equals(UiInputCapture other)
```

#### Parameters

`other` [UiInputCapture](./frinkyengine.core.ui.uiinputcapture)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Deconstruct(Boolean&, Boolean&, Boolean&)**

```csharp
void Deconstruct(Boolean& WantsMouse, Boolean& WantsKeyboard, Boolean& WantsTextInput)
```

#### Parameters

`WantsMouse` [Boolean&](https://docs.microsoft.com/en-us/dotnet/api/system.boolean&)<br>

`WantsKeyboard` [Boolean&](https://docs.microsoft.com/en-us/dotnet/api/system.boolean&)<br>

`WantsTextInput` [Boolean&](https://docs.microsoft.com/en-us/dotnet/api/system.boolean&)<br>
