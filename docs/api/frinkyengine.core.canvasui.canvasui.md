# CanvasUI

Namespace: FrinkyEngine.Core.CanvasUI

```csharp
public static class CanvasUI
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [CanvasUI](./frinkyengine.core.canvasui.canvasui)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **RootPanel**

```csharp
public static RootPanel RootPanel { get; }
```

#### Property Value

[RootPanel](./frinkyengine.core.canvasui.rootpanel)<br>

## Methods

### **Initialize()**

```csharp
public static void Initialize()
```

### **Update(Single, Int32, Int32, Nullable&lt;Vector2&gt;)**

```csharp
public static void Update(float dt, int screenWidth, int screenHeight, Nullable<Vector2> mouseOverride)
```

#### Parameters

`dt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

`screenWidth` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`screenHeight` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`mouseOverride` [Nullable&lt;Vector2&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **LoadStyleSheet(String)**

```csharp
public static void LoadStyleSheet(string css)
```

#### Parameters

`css` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **LoadStyleSheetFromAsset(String)**

Loads a CSS stylesheet from an asset path and applies it to CanvasUI.

```csharp
public static bool LoadStyleSheetFromAsset(string assetPath)
```

#### Parameters

`assetPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ClearStyleSheets()**

```csharp
public static void ClearStyleSheets()
```

### **LoadMarkup(String, Object, Boolean)**

Builds UI panels from markup text.

```csharp
public static Panel LoadMarkup(string markup, object bindingContext, bool clearRoot)
```

#### Parameters

`markup` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`bindingContext` [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

`clearRoot` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[Panel](./frinkyengine.core.canvasui.panel)<br>

### **LoadMarkupFromAsset(String, Object, Boolean)**

Builds UI panels from a markup asset path.

```csharp
public static Panel LoadMarkupFromAsset(string assetPath, object bindingContext, bool clearRoot)
```

#### Parameters

`assetPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`bindingContext` [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

`clearRoot` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[Panel](./frinkyengine.core.canvasui.panel)<br>

### **SetBindingContext(Object)**

Assigns the root binding context used by one-way markup bindings.

```csharp
public static void SetBindingContext(object context)
```

#### Parameters

`context` [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

### **EnableHotReload(Boolean)**

Enables or disables CanvasUI asset hot reload polling for loaded
 markup and stylesheet files.

```csharp
public static void EnableHotReload(bool enabled)
```

#### Parameters

`enabled` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Reset()**

Remove all child panels from the root, resetting the UI tree.
 Call between play sessions to prevent duplicate panels.

```csharp
public static void Reset()
```

### **RegisterFont(String, String)**

```csharp
public static void RegisterFont(string name, string path)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Shutdown()**

```csharp
public static void Shutdown()
```
