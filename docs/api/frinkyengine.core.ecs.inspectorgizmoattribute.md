# InspectorGizmoAttribute

Namespace: FrinkyEngine.Core.ECS

Marks a [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3) property so the editor draws a draggable
 gizmo sphere at that position in the viewport.

```csharp
public sealed class InspectorGizmoAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [InspectorGizmoAttribute](./frinkyengine.core.ecs.inspectorgizmoattribute)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute)

## Properties

### **Label**

Short label shown next to the gizmo sphere.

```csharp
public string Label { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GizmoRadius**

Wireframe sphere radius.

```csharp
public float GizmoRadius { get; set; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

### **ColorR**

Gizmo color — red channel.

```csharp
public byte ColorR { get; set; }
```

#### Property Value

[Byte](https://docs.microsoft.com/en-us/dotnet/api/system.byte)<br>

### **ColorG**

Gizmo color — green channel.

```csharp
public byte ColorG { get; set; }
```

#### Property Value

[Byte](https://docs.microsoft.com/en-us/dotnet/api/system.byte)<br>

### **ColorB**

Gizmo color — blue channel.

```csharp
public byte ColorB { get; set; }
```

#### Property Value

[Byte](https://docs.microsoft.com/en-us/dotnet/api/system.byte)<br>

### **SpaceProperty**

Name of a sibling property returning an enum with `World`/`Local` members.
 When the value is `Local`, the gizmo position is transformed from entity-local to
 world space using the entity's world matrix before drawing.

```csharp
public string SpaceProperty { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **TypeId**

```csharp
public object TypeId { get; }
```

#### Property Value

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

## Constructors

### **InspectorGizmoAttribute(String)**

Marks a [Vector3](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3) property so the editor draws a draggable
 gizmo sphere at that position in the viewport.

```csharp
public InspectorGizmoAttribute(string label)
```

#### Parameters

`label` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
