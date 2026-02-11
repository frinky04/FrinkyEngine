# InspectorUiMode

Namespace: FrinkyEngine.Core.ECS

Controls when an inspector extension (button/message) is visible.

```csharp
public enum InspectorUiMode
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [InspectorUiMode](./frinkyengine.core.ecs.inspectoruimode)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| Always | 0 | Always show the extension. |
| EditorOnly | 1 | Show only while the editor is in edit/simulate scene-editable mode. |
| RuntimeOnly | 2 | Show only while the editor is in runtime mode (play/simulate). |
