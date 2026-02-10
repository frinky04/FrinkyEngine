# BodyMotionType

Namespace: FrinkyEngine.Core.Components

Controls how a rigidbody is simulated.

```csharp
public enum BodyMotionType
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [BodyMotionType](./frinkyengine.core.components.bodymotiontype)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| Dynamic | 0 | Fully simulated by physics. |
| Kinematic | 1 | Moved by gameplay code, collides with dynamics. |
| Static | 2 | Immovable collision geometry. |
