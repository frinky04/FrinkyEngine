# UiStyle

Namespace: FrinkyEngine.Core.UI

Optional style data that can be applied by widget helpers.

```csharp
public struct UiStyle
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [UiStyle](./frinkyengine.core.ui.uistyle)<br>
Attributes [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **TextColor**

Optional text color override.

```csharp
public Nullable<Vector4> TextColor { get; set; }
```

#### Property Value

[Nullable&lt;Vector4&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>

### **WrapWidth**

Optional text wrap width in pixels; values less than or equal to zero disable wrapping.

```csharp
public float WrapWidth { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **Disabled**

When `true`, widget content is drawn in disabled state.

```csharp
public bool Disabled { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
