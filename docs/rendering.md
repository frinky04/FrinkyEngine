# Rendering & Post-Processing

## Forward+ Tiled Lighting

The renderer uses **forward+ tiled lighting** with Raylib and GLSL shaders.

- The viewport is divided into screen-space tiles
- Each tile maintains a list of lights that affect it
- Per-fragment lighting evaluates only the lights relevant to each tile
- **Shading**: Phong specular with per-fragment lighting

### Configuration

Tile size, max lights, and max lights per tile are configurable in `project_settings.json`:

| Setting | Default | Description |
|---------|---------|-------------|
| `forwardPlusTileSize` | 16 | Screen-space tile size in pixels |
| `forwardPlusMaxLights` | 256 | Maximum total lights in the scene |
| `forwardPlusMaxLightsPerTile` | 64 | Maximum lights evaluated per tile |

See [Project Settings](project-settings.md) for the full settings reference.

## Light Types

| Type | Description |
|------|-------------|
| **Directional** | Parallel rays, simulates sunlight. No position, only direction. |
| **Point** | Radial falloff from a position. Configurable range and intensity. |
| **Skylight** | Ambient hemisphere lighting. Provides base illumination. |

Set up lights by adding a `LightComponent` to an entity.

## Camera Setup

Add a `CameraComponent` to an entity:

| Property | Default | Description |
|----------|---------|-------------|
| `FieldOfView` | 60 | Vertical FOV in degrees |
| `NearPlane` | 0.1 | Near clip distance |
| `FarPlane` | 1000 | Far clip distance |
| `Projection` | Perspective | Perspective or Orthographic |
| `ClearColor` | — | Background clear color |
| `IsMain` | false | Mark as the scene's primary camera |

The scene renders from the entity marked with `IsMain = true`.

## Model Formats

Supported model formats (loaded via Raylib):

- **OBJ** — Wavefront OBJ with MTL materials
- **GLTF** — glTF 2.0 text format
- **GLB** — glTF 2.0 binary format

Drag models from the asset browser into the viewport to create entities with `MeshRendererComponent`.

## Skeletal Animation

Skeletal animations embedded in GLTF/GLB models are played back using GPU skinning. Bone matrices are computed on the CPU with frame interpolation and uploaded to the vertex shader each frame.

### Setup

1. Import a GLTF/GLB model that contains skeletal animations
2. Add a `MeshRendererComponent` pointing to the model
3. Add a `SkinnedMeshAnimatorComponent` to the same entity

The animator automatically loads all animation clips from the model and begins playback.

### Properties

| Property | Default | Description |
|----------|---------|-------------|
| `ClipIndex` | 0 | Selected animation entry. `0` is `(none)` (bind pose), `1..N` are imported clips. |
| `PlayAutomatically` | true | Start playback when the clip is loaded |
| `Playing` | true | Whether playback is currently advancing |
| `Loop` | true | Wrap to the beginning when the clip ends |
| `PlaybackSpeed` | 1.0 | Speed multiplier (0–4) |
| `AnimationFps` | 60 | Sample rate in frames per second (1–120) |
| `AnimationSources` | (empty) | Additional .glb files to load animation clips from. When empty, animations are loaded from the mesh file only. When populated, animations from all sources are merged into the clip list. |
| `UseEmbeddedAnimations` | true | Whether to include animations embedded in the mesh file. Set to false to use only external animation sources. |

Read-only inspector fields show the current `ActionName`, `ActionCount`, and `FrameCount`. A collapsible **Bone Hierarchy** tree is displayed at the bottom of the inspector, showing all bones and their parent-child relationships.

### Bone Preview

Press `F10` (or **View > Bone Preview**) to toggle a viewport overlay that draws bone joints as wireframe spheres and parent-child connections as lines for all skinned mesh entities.

### How It Works

The animator samples two adjacent keyframes from the active clip each frame, interpolates bone transforms, rebuilds skinning matrices, and writes them to the mesh's `BoneMatrices` buffer. The vertex shader transforms vertices by their bone weights when the `useSkinning` uniform is set. Entities with an active animator are excluded from automatic instanced batching since each instance has unique bone data.

### Inverse Kinematics

Add an `InverseKinematicsComponent` to the same entity as `SkinnedMeshAnimatorComponent` to run IK after animation sampling. Solvers are applied in list order after the animation clip is sampled (or from bind pose when `ClipIndex` is `(none)`).

#### TwoBoneIKSolver

The built-in solver for three-joint limb chains (e.g. upper-arm → forearm → hand, thigh → calf → foot). The three bones must form a direct parent chain in the skeleton.

| Property | Default | Description |
|----------|---------|-------------|
| `Enabled` | true | Toggle this solver on/off |
| `Weight` | 1.0 | Blend between animation pose (0) and full IK (1) |
| `RootBone` | (none) | First bone in the chain (e.g. upper arm / thigh) |
| `MidBone` | (none) | Middle joint (e.g. forearm / shin) |
| `EndBone` | (none) | End effector (e.g. hand / foot) |
| `TargetSpace` | World | Coordinate space for the target position — `World` or `Local` |
| `TargetPosition` | (0,0,0) | Position the end effector reaches toward |
| `PoleTargetSpace` | World | Coordinate space for the pole target — `World` or `Local` |
| `PoleTargetPosition` | (0,0,0) | Controls the bend-plane direction (e.g. knee/elbow) |

When `TargetSpace` or `PoleTargetSpace` is set to `Local`, the position is relative to the owning entity's transform.

#### FABRIKSolver

FABRIK (Forward And Backward Reaching Inverse Kinematics) solver for arbitrary-length bone chains. Unlike Two Bone IK, FABRIK supports chains of any length — the bones between root and end are discovered automatically by walking the skeleton hierarchy.

| Property | Default | Description |
|----------|---------|-------------|
| `Enabled` | true | Toggle this solver on/off |
| `Weight` | 1.0 | Blend between animation pose (0) and full IK (1) |
| `RootBone` | (none) | First bone in the chain |
| `EndBone` | (none) | End effector bone |
| `TargetSpace` | World | Coordinate space for the target position — `World` or `Local` |
| `TargetPosition` | (0,0,0) | Position the end effector reaches toward |
| `MaxIterations` | 10 | Maximum FABRIK iterations per solve (1–50) |
| `Tolerance` | 0.001 | Convergence threshold in world units — iteration stops when the end effector is within this distance of the target |

The root and end bones must be connected through a direct parent chain in the skeleton (end is a descendant of root). When the target is unreachable, the chain stretches toward it.

#### LookAtIKSolver

Rotates a single bone so that a chosen local axis points toward a target position. Useful for head tracking, turrets, eye gaze, and similar aim constraints.

| Property | Default | Description |
|----------|---------|-------------|
| `Enabled` | true | Toggle this solver on/off |
| `Weight` | 1.0 | Blend between animation pose (0) and full IK (1) |
| `Bone` | (none) | The bone to rotate |
| `TargetSpace` | World | Coordinate space for the target position — `World` or `Local` |
| `TargetPosition` | (0,0,0) | Position to look at |
| `AimAxis` | Z+ | Which local bone axis should point toward the target |
| `UpAxis` | Y+ | Which local bone axis should align toward world up |

Available axes: `X+`, `Y+`, `Z+`, `X-`, `Y-`, `Z-`.

#### Viewport Gizmos

Solver target positions (and pole targets for Two Bone IK) appear as draggable wireframe spheres in the viewport. Click a sphere to select it, then drag the translate handle to reposition it. A line connects each gizmo to the entity origin.

### Notes

- The animator requires a sibling `MeshRendererComponent` on the same entity. It automatically requests a unique model instance so bone data does not conflict with other entities sharing the same model asset.
- Non-looping clips stop on the last frame and set `Playing` to false.
- Use the **Restart** and **Stop** inspector buttons to control playback during editing.

## Materials

Both `MeshRendererComponent` (via `MaterialSlots` list) and primitive components (via a single `Material` property) use the `Material` class. Three material types:

| Type | Description |
|------|-------------|
| `SolidColor` | Flat color (default) |
| `Textured` | Albedo texture mapped with UV coordinates |
| `TriplanarTexture` | Texture projected along world/local axes, configurable scale and blend sharpness |

## Post-Processing

Add a `PostProcessStackComponent` to a camera entity to enable post-processing. Effects are processed in order and can be individually toggled.

When no post-processing stack is active, rendering goes direct-to-screen with zero overhead.

### Setup

1. Select your camera entity
2. Add a `PostProcessStackComponent`
3. Click "Add Effect" in the inspector to add effects
4. Reorder effects with the up/down arrows; toggle with the enable checkbox

### Built-in Effects

#### Bloom

Multi-pass threshold/downsample/upsample glow.

| Property | Default | Description |
|----------|---------|-------------|
| `Threshold` | 1.2 | Brightness threshold for bloom extraction |
| `SoftKnee` | 0.5 | Soft threshold transition |
| `Intensity` | 0.35 | Bloom strength |
| `Iterations` | 5 | Number of blur passes |
| `Scatter` | 0.6 | Bloom spread (0 = tight glow, 1 = wide cinematic bloom) |

#### Fog

Distance-based atmospheric fog (requires depth).

| Property | Default | Description |
|----------|---------|-------------|
| `FogColor` | (180, 190, 200) | Fog color |
| `FogStart` | 10 | Start distance (Linear mode) |
| `FogEnd` | 100 | End distance (Linear mode) |
| `Density` | 0.02 | Fog density (Exponential modes) |
| `Mode` | Linear | Linear / Exponential / ExponentialSquared |

#### Ambient Occlusion (SSAO)

Screen-space ambient occlusion with bilateral blur (requires depth).

| Property | Default | Description |
|----------|---------|-------------|
| `Radius` | 20.0 | Sampling radius |
| `Intensity` | 1.0 | Occlusion strength |
| `Bias` | 1.0 | Depth comparison bias |
| `SampleCount` | 64 | Number of AO samples |
| `BlurSize` | 16 | Bilateral blur kernel size |

#### Palette

Snaps rendered colors to the nearest color in a loaded JASC-PAL palette file.

| Property | Default | Description |
|----------|---------|-------------|
| `PalettePath` | (none) | Asset reference to a .pal file (JASC-PAL format) |

#### Dither

PSX-style 4x4 Bayer ordered dithering for a retro look.

| Property | Default | Description |
|----------|---------|-------------|
| `ColorLevels` | 32 | Number of color levels per channel (lower = more retro) |
| `DitherStrength` | 1.0 | Blend between original (0) and dithered (1) |

### Writing Custom Effects

Subclass `PostProcessEffect` (an `FObject`) and override `Render`:

```csharp
using FrinkyEngine.Core.Rendering.PostProcessing;
using Raylib_cs;

public class MyEffect : PostProcessEffect
{
    public override string DisplayName => "My Effect";
    public override bool NeedsDepth => false;

    public float Strength { get; set; } = 1.0f;

    public override void Render(Texture2D source, RenderTexture2D destination, PostProcessContext context)
    {
        // Use PostProcessContext.Blit(...) for fullscreen passes.
        PostProcessContext.Blit(source, destination);
    }
}
```

Custom effects are discovered automatically via `FObjectTypeResolver` when loaded through a game assembly.

### Runtime CVar

- `r_postprocess 0` — disable post-processing at runtime
- `r_postprocess 1` — enable post-processing at runtime
- `r_autoinstancing 0` — disable automatic batching/instanced rendering for models and primitives
- `r_autoinstancing 1` — enable automatic batching/instanced rendering for models and primitives
- `r_animation 0` — disable skinned animation playback and force bind pose
- `r_animation 1` — enable skinned animation playback
