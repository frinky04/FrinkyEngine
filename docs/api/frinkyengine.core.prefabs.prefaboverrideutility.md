# PrefabOverrideUtility

Namespace: FrinkyEngine.Core.Prefabs

```csharp
public static class PrefabOverrideUtility
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PrefabOverrideUtility](./frinkyengine.core.prefabs.prefaboverrideutility)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Fields

### **EntityPseudoComponent**

```csharp
public static string EntityPseudoComponent;
```

### **NameProperty**

```csharp
public static string NameProperty;
```

### **ActiveProperty**

```csharp
public static string ActiveProperty;
```

### **EnabledProperty**

```csharp
public static string EnabledProperty;
```

### **EditorOnlyProperty**

```csharp
public static string EditorOnlyProperty;
```

## Methods

### **ComputeOverrides(PrefabNodeData, PrefabNodeData)**

```csharp
public static PrefabOverridesData ComputeOverrides(PrefabNodeData sourceRoot, PrefabNodeData instanceRoot)
```

#### Parameters

`sourceRoot` [PrefabNodeData](./frinkyengine.core.prefabs.prefabnodedata)<br>

`instanceRoot` [PrefabNodeData](./frinkyengine.core.prefabs.prefabnodedata)<br>

#### Returns

[PrefabOverridesData](./frinkyengine.core.prefabs.prefaboverridesdata)<br>

### **ApplyOverrides(PrefabNodeData, PrefabOverridesData)**

```csharp
public static void ApplyOverrides(PrefabNodeData root, PrefabOverridesData overrides)
```

#### Parameters

`root` [PrefabNodeData](./frinkyengine.core.prefabs.prefabnodedata)<br>

`overrides` [PrefabOverridesData](./frinkyengine.core.prefabs.prefaboverridesdata)<br>
