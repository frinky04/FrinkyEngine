# Dither Effect - Statefile

## Status: Complete

## Files Created
- `Shaders/dither.fs` — 4x4 Bayer dither fragment shader
- `src/FrinkyEngine.Core/Rendering/PostProcessing/Effects/DitherEffect.cs` — C# effect class

## Implementation Notes
- Uses shared `postprocess.vs` vertex shader (same as fog, etc.)
- 4x4 Bayer matrix hardcoded as const array in shader
- Effect auto-discovered by `FObjectTypeResolver` (no manual registration needed)
- Parameters: ColorLevels (float, default 32), DitherStrength (float, default 1), Enabled (bool, inherited)
- Does not need depth texture
- Build: passes with 0 errors, 0 warnings
