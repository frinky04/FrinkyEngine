# Component

Namespace: FrinkyEngine.Core.ECS

Base class for all components that can be attached to an [Component.Entity](./frinkyengine.core.ecs.component#entity).

```csharp
public abstract class Component
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Component](./frinkyengine.core.ecs.component)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

**Remarks:**

Components follow a Unity-style lifecycle: [Component.Awake()](./frinkyengine.core.ecs.component#awake) is called when the component is added,
 [Component.Start()](./frinkyengine.core.ecs.component#start) runs before the first update (only if [Component.Enabled](./frinkyengine.core.ecs.component#enabled)),
 then [Component.Update(Single)](./frinkyengine.core.ecs.component#updatesingle) and [Component.LateUpdate(Single)](./frinkyengine.core.ecs.component#lateupdatesingle) run each frame.
 [Component.OnDestroy()](./frinkyengine.core.ecs.component#ondestroy) is called when the component is removed or the entity is destroyed.

## Properties

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

### **Component()**

```csharp
protected Component()
```

## Methods

### **Awake()**

Called once when the component is first added to an entity, before [Component.Start()](./frinkyengine.core.ecs.component#start).

```csharp
public void Awake()
```

### **Start()**

Called once before the first [Component.Update(Single)](./frinkyengine.core.ecs.component#updatesingle), only if the component is [Component.Enabled](./frinkyengine.core.ecs.component#enabled).

```csharp
public void Start()
```

### **Update(Single)**

Called every frame while the component is enabled.

```csharp
public void Update(float dt)
```

#### Parameters

`dt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Time elapsed since the previous frame, in seconds.

### **LateUpdate(Single)**

Called every frame after all [Component.Update(Single)](./frinkyengine.core.ecs.component#updatesingle) calls have completed.

```csharp
public void LateUpdate(float dt)
```

#### Parameters

`dt` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Time elapsed since the previous frame, in seconds.

### **OnDestroy()**

Called when the component is removed from its entity or the entity is destroyed.

```csharp
public void OnDestroy()
```

### **OnEnable()**

Called when the component transitions from disabled to enabled.

```csharp
public void OnEnable()
```

### **OnDisable()**

Called when the component transitions from enabled to disabled.

```csharp
public void OnDisable()
```

### **OnTriggerEnter(Entity)**

Called when a trigger collider on this entity first overlaps another entity.

```csharp
public void OnTriggerEnter(Entity other)
```

#### Parameters

`other` [Entity](./frinkyengine.core.ecs.entity)<br>
The other entity involved in the trigger overlap.

### **OnTriggerStay(Entity)**

Called each frame while a trigger collider on this entity continues to overlap another entity.

```csharp
public void OnTriggerStay(Entity other)
```

#### Parameters

`other` [Entity](./frinkyengine.core.ecs.entity)<br>
The other entity involved in the trigger overlap.

### **OnTriggerExit(Entity)**

Called when a trigger collider on this entity stops overlapping another entity.

```csharp
public void OnTriggerExit(Entity other)
```

#### Parameters

`other` [Entity](./frinkyengine.core.ecs.entity)<br>
The other entity involved in the trigger overlap.

### **OnCollisionEnter(CollisionInfo)**

Called when a physics collision first begins with another entity.

```csharp
public void OnCollisionEnter(CollisionInfo info)
```

#### Parameters

`info` [CollisionInfo](./frinkyengine.core.physics.collisioninfo)<br>
Information about the collision contact.

### **OnCollisionStay(CollisionInfo)**

Called each frame while a physics collision continues with another entity.

```csharp
public void OnCollisionStay(CollisionInfo info)
```

#### Parameters

`info` [CollisionInfo](./frinkyengine.core.physics.collisioninfo)<br>
Information about the collision contact.

### **OnCollisionExit(CollisionInfo)**

Called when a physics collision ends with another entity.

```csharp
public void OnCollisionExit(CollisionInfo info)
```

#### Parameters

`info` [CollisionInfo](./frinkyengine.core.physics.collisioninfo)<br>
Information about the collision contact.

### **StartCoroutine(IEnumerator)**

Starts a coroutine on this component. The coroutine runs each frame during the component update loop
 and pauses when the component is disabled. All coroutines are cancelled when the component is destroyed.

```csharp
public Coroutine StartCoroutine(IEnumerator routine)
```

#### Parameters

`routine` [IEnumerator](https://docs.microsoft.com/en-us/dotnet/api/system.collections.ienumerator)<br>
An iterator method that yields [YieldInstruction](./frinkyengine.core.coroutines.yieldinstruction) objects or `null`.

#### Returns

[Coroutine](./frinkyengine.core.coroutines.coroutine)<br>
A [Coroutine](./frinkyengine.core.coroutines.coroutine) handle that can be used to stop the coroutine.

### **StopCoroutine(Coroutine)**

Stops a specific coroutine that was started on this component.

```csharp
public void StopCoroutine(Coroutine coroutine)
```

#### Parameters

`coroutine` [Coroutine](./frinkyengine.core.coroutines.coroutine)<br>
The coroutine handle returned by [Component.StartCoroutine(IEnumerator)](./frinkyengine.core.ecs.component#startcoroutineienumerator).

### **StopAllCoroutines()**

Stops all coroutines running on this component.

```csharp
public void StopAllCoroutines()
```

### **Invoke(Action, Single)**

Schedules a callback to be invoked after a delay. The timer respects [Scene.TimeScale](./frinkyengine.core.scene.scene#timescale).

```csharp
public void Invoke(Action callback, float delaySeconds)
```

#### Parameters

`callback` [Action](https://docs.microsoft.com/en-us/dotnet/api/system.action)<br>
The action to invoke.

`delaySeconds` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Time in scaled seconds before the callback fires.

### **InvokeRepeating(Action, Single, Single)**

Schedules a callback to be invoked repeatedly. The first invocation occurs after `delay`,
 then every `interval` seconds. Timers respect [Scene.TimeScale](./frinkyengine.core.scene.scene#timescale).

```csharp
public void InvokeRepeating(Action callback, float delay, float interval)
```

#### Parameters

`callback` [Action](https://docs.microsoft.com/en-us/dotnet/api/system.action)<br>
The action to invoke.

`delay` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Initial delay in scaled seconds.

`interval` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Interval in scaled seconds between subsequent invocations.

### **CancelInvoke()**

Cancels all pending timer invocations on this component.

```csharp
public void CancelInvoke()
```

### **CancelInvoke(Action)**

Cancels all pending timer invocations that reference a specific callback.

```csharp
public void CancelInvoke(Action callback)
```

#### Parameters

`callback` [Action](https://docs.microsoft.com/en-us/dotnet/api/system.action)<br>
The callback to cancel.
