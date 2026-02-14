# Bloom Upgrade Statefile

## Status: IN PROGRESS

## Goal
Upgrade bloom to modern UE4/5-style progressive downsample/upsample pipeline.

## Current State
The codebase already has a multi-pass bloom (threshold + downsample chain + upsample + composite) with:
- 13-tap downsample filter
- 9-tap tent upsample filter
- Soft threshold with knee

## Changes Needed
1. Add `Scatter` parameter for per-mip intensity weighting
2. Fix upsample pass to use per-mip scatter weights instead of plain additive blend
3. Fix potential RT leak in upsample loop (mipChain[i] overwritten without release)
4. Pass scatter weight to upsample shader as uniform

## Files Modified
- `src/FrinkyEngine.Core/Rendering/PostProcessing/Effects/BloomEffect.cs` - Add Scatter param, fix upsample loop
- `Shaders/bloom_upsample.fs` - Add scatter weight uniform
