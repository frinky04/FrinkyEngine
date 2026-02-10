# UiScope

Namespace: FrinkyEngine.Core.UI

Disposable helper used for UI begin/end scopes.

```csharp
public struct UiScope
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [UiScope](./frinkyengine.core.ui.uiscope)<br>
Implements [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable)<br>
Attributes [IsReadOnlyAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isreadonlyattribute)

## Properties

### **IsVisible**

Indicates whether the scope's contents should be emitted.

```csharp
public bool IsVisible { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Methods

### **Dispose()**

```csharp
void Dispose()
```
