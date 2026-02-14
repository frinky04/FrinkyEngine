# Statefile: Scripting Phase 3 — Timers, Coroutines & Scheduling

## Status: Complete

## Summary

All Phase 3 features implemented: timers, coroutines, yield instructions, and scene-level time tracking.

## Files Created
- `src/FrinkyEngine.Core/Coroutines/YieldInstruction.cs` — abstract base for yield instructions
- `src/FrinkyEngine.Core/Coroutines/WaitForSeconds.cs` — wait scaled time
- `src/FrinkyEngine.Core/Coroutines/WaitForSecondsRealtime.cs` — wait unscaled time
- `src/FrinkyEngine.Core/Coroutines/WaitUntil.cs` — wait for condition true
- `src/FrinkyEngine.Core/Coroutines/WaitWhile.cs` — wait while condition false
- `src/FrinkyEngine.Core/Coroutines/Coroutine.cs` — coroutine handle with tick logic
- `src/FrinkyEngine.Core/Coroutines/CoroutineRunner.cs` — per-component coroutine manager
- `src/FrinkyEngine.Core/Coroutines/TimerEntry.cs` — timer data class
- `src/FrinkyEngine.Core/Coroutines/TimerRunner.cs` — per-component timer manager

## Files Modified
- `src/FrinkyEngine.Core/ECS/Component.cs` — added StartCoroutine, StopCoroutine, StopAllCoroutines, Invoke, InvokeRepeating, CancelInvoke; pause/resume on enable/disable
- `src/FrinkyEngine.Core/ECS/Entity.cs` — UpdateComponents passes unscaled dt; cancel coroutines/timers on destroy and RemoveComponent
- `src/FrinkyEngine.Core/Scene/Scene.cs` — added Time, UnscaledTime, UnscaledDeltaTime, FrameCount; passes unscaled dt through update loop
- `docs/scripting.md` — documented timers, coroutines, yield instructions, scene time

## Build Status
- Build succeeded, 0 warnings, 0 errors
