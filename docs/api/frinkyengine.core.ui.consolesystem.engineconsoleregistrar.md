# EngineConsoleRegistrar

Namespace: FrinkyEngine.Core.UI.ConsoleSystem

Registers engine-provided developer-console commands and cvars.

```csharp
public static class EngineConsoleRegistrar
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [EngineConsoleRegistrar](./frinkyengine.core.ui.consolesystem.engineconsoleregistrar)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **RegisterAll(RegistrationContext)**

Registers all engine commands and cvars. Safe when called once per process.

```csharp
public static void RegisterAll(RegistrationContext context)
```

#### Parameters

`context` [RegistrationContext](./frinkyengine.core.ui.consolesystem.engineconsoleregistrar.registrationcontext)<br>
Registration context for overlay-bound actions.
