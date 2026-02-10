# PrefabOverridesData

Namespace: FrinkyEngine.Core.Prefabs

```csharp
public class PrefabOverridesData
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PrefabOverridesData](./frinkyengine.core.prefabs.prefaboverridesdata)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **PropertyOverrides**

```csharp
public List<PrefabPropertyOverrideData> PropertyOverrides { get; set; }
```

#### Property Value

[List&lt;PrefabPropertyOverrideData&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

### **AddedComponents**

```csharp
public List<PrefabComponentOverrideData> AddedComponents { get; set; }
```

#### Property Value

[List&lt;PrefabComponentOverrideData&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

### **RemovedComponents**

```csharp
public List<PrefabRemovedComponentOverrideData> RemovedComponents { get; set; }
```

#### Property Value

[List&lt;PrefabRemovedComponentOverrideData&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

### **AddedChildren**

```csharp
public List<PrefabAddedChildOverrideData> AddedChildren { get; set; }
```

#### Property Value

[List&lt;PrefabAddedChildOverrideData&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

### **RemovedChildren**

```csharp
public List<string> RemovedChildren { get; set; }
```

#### Property Value

[List&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

## Constructors

### **PrefabOverridesData()**

```csharp
public PrefabOverridesData()
```

## Methods

### **Clone()**

```csharp
public PrefabOverridesData Clone()
```

#### Returns

[PrefabOverridesData](./frinkyengine.core.prefabs.prefaboverridesdata)<br>
