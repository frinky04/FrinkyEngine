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
| `Threshold` | 1.0 | Brightness threshold for bloom extraction |
| `SoftKnee` | 0.5 | Soft threshold transition |
| `Intensity` | 1.0 | Bloom strength |
| `Iterations` | 5 | Number of blur passes |

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
