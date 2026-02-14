# MeshRendererComponent

Namespace: FrinkyEngine.Core.Components

Renders a 3D model loaded from a file (e.g. .obj, .gltf, .glb).
 Supports multiple material slots for per-material mapping and texture assignment.

```csharp
public class MeshRendererComponent : RenderableComponent
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component) → [RenderableComponent](./frinkyengine.core.components.renderablecomponent) → [MeshRendererComponent](./frinkyengine.core.components.meshrenderercomponent)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute), [ComponentCategoryAttribute](./frinkyengine.core.ecs.componentcategoryattribute)

## Properties

### **ModelPath**

Asset-relative path to the model file. Changing this triggers a reload on the next frame.

```csharp
public AssetReference ModelPath { get; set; }
```

#### Property Value

[AssetReference](./frinkyengine.core.assets.assetreference)<br>

### **MaterialSlots**

Per-material configurations for this model. Slots are auto-created to match the model's material count.

```csharp
public List<Material> MaterialSlots { get; set; }
```

#### Property Value

[List&lt;Material&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>

### **HasLoadedModel**

Whether this component has a loaded model ready for rendering.

```csharp
public bool HasLoadedModel { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Entity**

The [Entity](./frinkyengine.core.ecs.entity) this component is attached to.

```csharp
public Entity Entity { get; internal set; }
```

#### Property Value

[Entity](./frinkyengine.core.ecs.entity)<br>

### **Enabled**

Whether this component is active. Disabled components skip [Component.Update(Single)](./frinkyengine.core.ecs.component#updatesingle) and [Component.LateUpdate(Single)](./frinkyengine.core.ecs.component#lateupdatesingle).

```csharp
public bool Enabled { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **EditorOnly**

When `true`, this component is only active in the editor and is skipped during runtime play.

```csharp
public bool EditorOnly { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **HasStarted**

Indicates whether [Component.Start()](./frinkyengine.core.ecs.component#start) has already been called on this component.

```csharp
public bool HasStarted { get; internal set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **MeshRendererComponent()**

```csharp
public MeshRendererComponent()
```

## Methods

### **RefreshMaterials()**

Forces the model and materials to be reloaded from disk on the next frame.

```csharp
public void RefreshMaterials()
```

### **TransferModelFrom(MeshRendererComponent)**

Transfers ownership of the loaded model from another [MeshRendererComponent](./frinkyengine.core.components.meshrenderercomponent).
 Used by the undo/redo system to avoid reloading models from disk when restoring a snapshot.
 The source component's model is cleared so it will not unload the transferred resources.

```csharp
public void TransferModelFrom(MeshRendererComponent source)
```

#### Parameters

`source` [MeshRendererComponent](./frinkyengine.core.components.meshrenderercomponent)<br>
The component to transfer the model from.

### **Invalidate()**

```csharp
public void Invalidate()
```

### **Start()**

```csharp
public void Start()
```

### **OnDestroy()**

```csharp
public void OnDestroy()
```
