# AudioDeviceService

Namespace: FrinkyEngine.Core.Audio

Shared audio-device lifetime management for editor and runtime.

```csharp
public static class AudioDeviceService
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [AudioDeviceService](./frinkyengine.core.audio.audiodeviceservice)

## Methods

### **EnsureInitialized()**

Ensures the audio device is initialized and increments the usage count.

```csharp
public static void EnsureInitialized()
```

### **ShutdownIfUnused()**

Decrements the usage count and closes the audio device when no users remain.

```csharp
public static void ShutdownIfUnused()
```
