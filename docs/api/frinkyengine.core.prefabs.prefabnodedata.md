# PrefabNodeData

Namespace: FrinkyEngine.Core.Prefabs

```csharp
public class PrefabNodeData
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PrefabNodeData](./frinkyengine.core.prefabs.prefabnodedata)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **StableId**

```csharp
public string StableId { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Name**

```csharp
public string Name { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Active**

```csharp
public bool Active { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Components**

```csharp
public List<PrefabComponentData> Components { get; set; }
```

#### Property Value

[List&lt;PrefabComponentData&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

### **Children**

```csharp
public List<PrefabNodeData> Children { get; set; }
```

#### Property Value

[List&lt;PrefabNodeData&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

## Constructors

### **PrefabNodeData()**

```csharp
public PrefabNodeData()
```

## Methods

### **Clone()**

```csharp
public PrefabNodeData Clone()
```

#### Returns

[PrefabNodeData](./frinkyengine.core.prefabs.prefabnodedata)<br>
