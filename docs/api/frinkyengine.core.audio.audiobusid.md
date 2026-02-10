# AudioBusId

Namespace: FrinkyEngine.Core.Audio

Identifies mixer buses used for routing and volume control.

```csharp
public enum AudioBusId
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [AudioBusId](./frinkyengine.core.audio.audiobusid)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| Master | 0 | Master output bus. |
| Music | 1 | Music bus. |
| Sfx | 2 | Sound effects bus. |
| Ui | 3 | User interface bus. |
| Voice | 4 | Voice/dialog bus. |
| Ambient | 5 | Ambient bus. |
