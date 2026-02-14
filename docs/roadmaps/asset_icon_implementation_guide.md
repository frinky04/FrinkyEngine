# Asset Icon Implementation Guide

This guide tracks the staged rollout for generated asset icons (textures, meshes/models, and prefabs) and the path to make the pipeline reusable for game scripting use cases (for example inventory icons).

## Phase 1: Editor Icon Generation Pipeline

- [x] Add an editor-side icon service with a background-style queue that processes a small amount of work each frame.
- [x] Support generated icon rendering for:
- [x] Textures
- [x] Meshes/models
- [x] Prefabs (model-backed prefabs)
- [x] Render previews off-screen through a temporary preview scene, basic lighting, render, then dispose.
- [x] Cache generated icon PNGs under `.frinky/asset-icons/`.
- [x] Add a cache manifest keyed by asset path with a content hash to skip unchanged assets.
- [x] Wire icon invalidation/requeue into asset refresh/file-change flow.

## Phase 2: Editor UX Integration and Reliability

- [x] Use generated icons in the Asset Browser with fallback to static type icons.
- [x] Use generated icons in `AssetReference` inspector dropdown entries.
- [x] Add an on-demand "Regenerate Icon" context action for supported asset types.
- [x] Add generation status indicators (queued/generating/failed) in the UI.
- [x] Add perf counters (queue length, ms per icon, cache hit rate) to a debug panel.
- [ ] Add retry/backoff behavior for transient generation failures.
- [x] Add prefab preview support for non-model prefab renderables (primitives/custom renderables).

## Phase 3: Generic Runtime/Scripting Extensibility

- [ ] Extract preview rendering into a reusable API surface that does not depend on editor panels.
- [ ] Introduce pluggable icon providers (for example `IIconPreviewProvider`) with registration hooks.
- [ ] Allow script-side registration of custom icon builders for gameplay items/inventory definitions.
- [ ] Add runtime-safe output controls (size, camera framing preset, transparent/solid background).
- [ ] Document scripting-facing usage patterns in `docs/scripting.md`.
- [ ] Add example: generate inventory icons from scripted item definitions at build time or in tooling mode.
