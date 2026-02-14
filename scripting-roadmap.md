# Scripting API Roadmap

Status: **Draft** | Last updated: 2026-02-14

---

## Phase 1 — Runtime Spawning & Entity Queries ✅

The foundation for dynamic gameplay. Without these, scripts can't create or find things at runtime.

### Prefab Instantiation from Scripts
- [x] Expose `PrefabService.InstantiatePrefab` through a public script-facing API
- [x] `Scene.Instantiate(prefabPath)` and `Scene.Instantiate(prefabPath, position, rotation)`
- [x] `AssetReference` overload so prefab references are drag-drop in the inspector
- [x] Entity remapping (EntityReferences inside spawned prefabs resolve correctly)

### Entity Finding
- [x] `Scene.FindEntityByName(string name)` — first match
- [x] `Scene.FindEntitiesByName(string name)` — all matches
- [x] `Scene.FindEntitiesWithComponent<T>()` — wrapper over existing `GetComponents<T>()` returning entities

### Hierarchy Traversal
- [x] `Entity.GetComponentInChildren<T>(bool includeInactive = false)`
- [x] `Entity.GetComponentInParent<T>()`
- [x] `Entity.GetComponents<T>()` — all components of type on this entity
- [x] `Entity.GetComponentsInChildren<T>(bool includeInactive = false)`

### Destroy API
- [x] `Entity.Destroy()` convenience (calls `Scene.RemoveEntity`)
- [x] Optional timed destroy: `Entity.Destroy(float delaySeconds)` (queued, processed end-of-frame)

**Exit criteria:** A script can spawn a prefab, find entities in the scene, walk the hierarchy, and destroy things — enough to build a basic spawner or projectile system.

---

## Phase 2 — Physics Queries & Collision ✅

Unlocks spatial gameplay: AoE, melee, ground detection, collision response.

### Shape Casts
- [x] `Physics.SphereCast(origin, radius, direction, maxDistance, out hit, params)`
- [x] `Physics.BoxCast(origin, halfExtents, orientation, direction, maxDistance, out hit, params)`
- [x] `Physics.CapsuleCast(origin, radius, length, orientation, direction, maxDistance, out hit, params)`
- [x] All with `*All` variants returning lists

### Overlap Queries
- [x] `Physics.OverlapSphere(center, radius, params)` → `List<Entity>`
- [x] `Physics.OverlapBox(center, halfExtents, orientation, params)` → `List<Entity>`
- [x] `Physics.OverlapCapsule(center, radius, length, orientation, params)` → `List<Entity>`

### Collision Callbacks
- [x] `OnCollisionEnter(CollisionInfo)`, `OnCollisionStay(CollisionInfo)`, `OnCollisionExit(CollisionInfo)`
- [x] `CollisionInfo` struct: `Entity Other`, `Vector3 ContactPoint`, `Vector3 Normal`, `float PenetrationDepth`
- [x] Wire into BEPU's contact manifold reporting

### Collision Layers — Deferred
- [ ] Layer enum or bitmask on colliders (e.g., Default, Player, Enemy, Projectile, Environment)
- [ ] Layer matrix for include/exclude rules
- [ ] Extend `RaycastParams` and shape queries to accept layer masks

Collision layers deferred to a future phase — requires layer matrix UI, serialization, and cross-cutting narrow phase integration. `RaycastParams` filtering (IgnoredEntities, IncludeTriggers) covers the most common needs.

**Exit criteria:** A script can detect what's in a radius, sweep a hitbox, and react to physics contacts with contact data. Layer filtering deferred.

---

## Phase 3 — Timers, Coroutines & Scheduling

Removes the biggest day-to-day friction in gameplay scripting.

### Simple Timers
- `Invoke(Action callback, float delaySeconds)`
- `InvokeRepeating(Action callback, float delay, float interval)`
- `CancelInvoke()` / `CancelInvoke(Action)`
- Processed in the component update loop, respects `TimeScale`

### Coroutines
- `StartCoroutine(IEnumerator routine)` → `Coroutine` handle
- `StopCoroutine(Coroutine)` / `StopAllCoroutines()`
- Yield instructions:
  - `yield return null` — resume next frame
  - `yield return new WaitForSeconds(float)` — wait scaled time
  - `yield return new WaitForSecondsRealtime(float)` — wait unscaled
  - `yield return new WaitUntil(Func<bool>)` — wait for condition
  - `yield return new WaitWhile(Func<bool>)` — wait while condition true
- Coroutines pause when component is disabled, cancel on destroy

### Scene-Level Time
- `Scene.Time` — total elapsed time (scaled)
- `Scene.UnscaledTime` — total elapsed time (unscaled)
- `Scene.UnscaledDeltaTime` — frame delta ignoring TimeScale
- `Scene.FrameCount` — total frames elapsed

**Exit criteria:** A script can delay actions, run multi-step sequences over time, and write readable async-style gameplay logic without manual state machines.

---

## Phase 4 — Input Abstraction

Makes games shippable with rebindable controls and controller support.

### Input Actions
- `InputAction` class: named action bound to one or more inputs
- `InputAction.IsPressed`, `InputAction.WasPressedThisFrame`, `InputAction.WasReleasedThisFrame`
- `InputAction.Value` (float, for analog axes)
- `InputAction.Vector2Value` (for combined stick/WASD)
- Default bindings configurable in project settings

### Input Map
- `InputMap` grouping actions (e.g., "Gameplay", "UI", "Vehicle")
- `InputMap.Enable()` / `InputMap.Disable()` for context switching
- Stack-based: enabling "UI" map can suppress "Gameplay" map

### Gamepad Support
- Expose Raylib's gamepad API: buttons, sticks, triggers
- Dead zones, stick curves
- Gamepad detection / hot-plug events

**Exit criteria:** A script can define named actions, bind them to keyboard and gamepad, switch input contexts, and support rebinding.

---

## Phase 5 — Runtime Asset Loading & Cross-Scene Persistence

Removes remaining barriers to shipping a full game.

### Runtime Asset Loading
- `Assets.Load<T>(string path)` — load asset by project-relative path
- Supported types: Model, Texture, Sound, Prefab (returns handle/reference)
- Caching: loaded assets stay cached until explicitly released or scene unloads

### Scene Transition
- `SceneManager.LoadScene(path, LoadMode)` — `LoadMode.Single` (replace) or `LoadMode.Additive`
- `SceneManager.UnloadScene(scene)`
- `Entity.DontDestroyOnLoad()` — persist entity across scene loads
- Scene load callbacks: `OnSceneLoaded`, `OnSceneUnloaded`

### Object Pooling (optional, stretch)
- `Pool<T>.Get()` / `Pool<T>.Release(T)` built-in utility
- Or engine-level entity pool: `Scene.Instantiate(prefab, usePool: true)`

**Exit criteria:** A script can load assets on demand, transition between scenes cleanly, and persist key entities (player, game manager) across loads.

---

## Phase 6 — Developer Experience & Debugging

Quality-of-life that speeds up iteration.

### Debug Drawing
- `Debug.DrawLine(from, to, color, duration)`
- `Debug.DrawRay(origin, direction, color, duration)`
- `Debug.DrawWireSphere(center, radius, color, duration)`
- `Debug.DrawWireBox(center, size, color, duration)`
- Rendered as overlay in editor viewport, optional in runtime builds

### Math Utilities
- `FMath.Lerp`, `FMath.InverseLerp`, `FMath.Remap`
- `FMath.MoveTowards`, `FMath.SmoothDamp`
- `FMath.Clamp`, `FMath.Clamp01`
- `FMath.Approximately`
- `Vector3` extensions: `MoveTowards`, `SmoothDamp`, `ProjectOnPlane`

### FixedUpdate
- `FixedUpdate(float fixedDt)` lifecycle hook synced to physics timestep
- Runs 0..N times per frame depending on accumulated time

### Events / Messaging (lightweight)
- `GameEvent<T>` — simple typed pub/sub
- `GameEvent<T>.Raise(T data)`, `GameEvent<T>.Subscribe(Action<T>)`, `GameEvent<T>.Unsubscribe(Action<T>)`
- No global bus — events are explicit objects passed via component references or statics

**Exit criteria:** Developers can visualize spatial logic, write physics-correct code in FixedUpdate, do common math without rolling their own, and decouple components through lightweight events.

---

## Phase Summary

| Phase | Theme | Key Unlocks |
|-------|-------|-------------|
| **1** ✅ | Spawning & Queries | Dynamic gameplay, projectiles, spawners |
| **2** ✅ | Physics & Collision | Melee, AoE, ground checks, contact response |
| **3** | Timers & Coroutines | Readable async logic, cooldowns, sequences |
| **4** | Input Abstraction | Gamepad, rebinding, shippable controls |
| **5** | Assets & Scenes | Scene transitions, runtime loading, persistence |
| **6** | DX & Debugging | Debug drawing, math helpers, FixedUpdate, events |

Phases 1-3 are the core gameplay loop. A game can ship after Phase 3 with keyboard-only input. Phases 4-6 are about polish and developer experience.
