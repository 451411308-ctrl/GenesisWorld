# Rendering and Shaders

**English** | [简体中文](./RenderingAndShaders.zh-CN.md)

## Overview

GenesisWorld's v0.3.0 rendering foundation uses Unity `2022.3.62f3c1`, Universal Render Pipeline `14.0.12`, ShaderLab, and hand-written HLSL. Three compact custom Shaders present generated terrain, textured low-poly assets, and the sky. The design prioritizes readable graphics fundamentals over a complete PBR feature set.

For the portfolio-oriented learning map, see [v0.3.0 Rendering & Shader Milestone](./RenderingAndShaders_Milestone.md).

## Rendering Architecture

```text
CPU World Generation                         GPU World Presentation
MeshGenerator ── geometry ────────────────→ StylizedTerrain
TerrainGenerator ── Mesh/Collider lifecycle ↗    ↑ main light / shadow / fog
EnvironmentSpawner ── placed instances ───→ StylizedEnvironment
Camera view direction ─────────────────────→ StylizedSkybox
RenderSettings ── skybox + Linear Fog ─────→ Final stylized scene
```

Procedural C# decides **where geometry and instances exist**. The GPU decides **how visible surfaces look**. Shaders do not resample Perlin noise, change the World Seed, or own placement.

## Shader Pipeline

```text
Vertex position + normal
        ↓ Vertex stage
Object space → world space → clip space
        ↓ Rasterization / interpolation
World position + world normal + main light
        ↓ Fragment stage
Surface color → lighting → shadow attenuation → fog
        ↓
Final pixel color
```

World space gives terrain slope, main-light direction, and sky direction one stable coordinate frame.

## Stylized Terrain

[`StylizedTerrain.shader`](../Assets/Shaders/Terrain/StylizedTerrain.shader) colors generated ground without a terrain texture set. Its `UniversalForward` pass receives world position, world normal, main-light shadow coordinates, and fog factor. URP Lit `ShadowCaster` and `DepthOnly` passes provide depth and casting support.

### Height-based Coloring

```text
heightFactor = saturate((worldY - HeightMin) / max(HeightMax - HeightMin, 0.0001))
heightColor = lerp(LowColor, HighColor, heightFactor)
```

The epsilon prevents division by zero. The current material uses `-2.5` to `2.5`, matching Height Scale `5` around a centered baseline.

### Slope Detection

```text
upAlignment = saturate(dot(normalWS, float3(0, 1, 0)))
slope = 1 - upAlignment
slopeFactor = smoothstep(SlopeStart, SlopeEnd, slope)
baseColor = lerp(heightColor, SlopeColor, slopeFactor)
```

A flat normal aligns with World Up and produces little slope color. Tilted normals reduce the dot product and smoothly introduce earth/rock color.

### Terrain Lighting

```text
NdotL = saturate(dot(normalWS, lightDirectionWS))
direct = NdotL * distanceAttenuation * shadowAttenuation
lighting = AmbientStrength + mainLightColor * direct
```

This is lightweight Lambert diffuse with a scalar ambient floor—not a full PBR BRDF or sampled GI solution.

## Stylized Environment

[`StylizedEnvironment.shader`](../Assets/Shaders/Environment/StylizedEnvironment.shader) is used by project-owned adapter materials for trees and rocks. It preserves each source texture/tint while making low-poly facets readable through wrapped, quantized direct lighting.

### BaseMap and BaseColor

```text
surface = sample(BaseMap, uv) * BaseColor
```

The project does not modify imported CC0 source materials; adapter materials reference their textures and apply the custom Shader.

### World-space Normals and Lighting Quantization

The detailed `N·L` explanation appears in Terrain Lighting above. Environment shading adds wrapping and bands:

```text
wrapped = saturate((saturate(dot(N, L)) + LightWrap) / (1 + LightWrap))
steps = max(round(LightSteps), 2)
banded = round(wrapped * (steps - 1)) / (steps - 1)
```

The selected default is three bands. Wrapping keeps slightly back-facing facets readable; quantization turns continuous diffuse values into a controlled Low-poly/Cell-style light language.

### Alpha Clipping

When `_ALPHATEST_ON` is enabled, fragments below `_Cutoff` are discarded. Forward, `ShadowCaster`, and `DepthOnly` passes call the same surface sampler, so foliage color, shadow silhouettes, and depth silhouettes agree.

### Shadows and Depth

The custom `ShadowCaster` applies URP shadow bias and supports directional/punctual caster variants. `DepthOnly` writes depth without color. Same-camera Hard/Soft comparisons selected Hard Shadows for clearer silhouettes at this scale.

## Stylized Skybox

[`StylizedSkybox.shader`](../Assets/Shaders/Sky/StylizedSkybox.shader) is an untextured view-direction gradient:

| Property | Current value | Purpose |
|---|---:|---|
| Zenith Color | `(0.18, 0.42, 0.72)` | Upper-sky blue |
| Horizon Color | `(0.72, 0.84, 0.82)` | Atmospheric transition and fog target |
| Lower Color | `(0.32, 0.38, 0.28)` | Natural lower hemisphere |
| Horizon Exponent | `0.65` | Gradient transition shape |

### View Direction and Horizon Gradient

The vertex stage transforms the cube direction to world space. The fragment stage normalizes it and uses Y: positive values blend horizon to zenith; negative values blend horizon to lower color.

```text
upper = pow(smoothstep(0, 1, saturate(viewDirection.y)), HorizonExponent)
lower = pow(smoothstep(0, 1, saturate(-viewDirection.y)), HorizonExponent)
```

Both hemispheres meet at the same Horizon Color, avoiding a deliberate seam.

## Fog and Atmospheric Depth

The scene uses Unity Linear Fog from `12` to `40`. Terrain and environment Forward passes compile fog variants, calculate `ComputeFogFactor`, and call `MixFog` after lighting.

```text
FinalColor = lerp(SurfaceColor, FogColor, FogFactor)
```

Fog Color exactly matches Skybox Horizon Color: `(0.72, 0.84, 0.82)`. Runtime comparisons covered Fog Off, `12–40`, and `10–32`; `12–40` retained local color while adding useful distance depth.

## Lighting and Quality Configuration

- Directional Light: rotation `(48, -32, 0)`, warm color `(1.00, 0.94, 0.84)`, intensity `1.15`
- Ambient source: Skybox, intensity `1.0`; custom materials use scalar ambient values around `0.32–0.35`
- High Fidelity shadows: Hard, `2048` main-light map, `40` distance, two cascades
- Light Bias `0.05`; Normal Bias `0.4`; Near Plane `0.2`
- Shadow distance and fog end both use `40`

No Global Volume or custom atmosphere manager is required by this foundation.

## Runtime Validation

- Scene: `Assets/Scenes/Test_Player_Controller.unity`
- Terrain: `20 × 20`, `50 × 50` segments, Height Scale `5`
- Environment: `18` trees, `12` rocks; adapter materials retain source textures
- Seed `12345`: repeated layout signature `2087925580`
- Terrain, environment, skybox, hard shadows, and Linear Fog render without pink materials
- Project C# and Shader errors: `0` during milestone validation

## Current Limitations

- Main directional light only; no custom Additional Lights loop
- No PBR workflow in the custom terrain/environment Shaders
- Smooth terrain vertex normals; no triplanar texture layer or normal maps
- No water, vegetation wind, weather, day/night, clouds, or volumetric fog
- No screen-space effects or post-processing framework
- Small single procedural map; no biome, chunks, streaming, or LOD

## Future Rendering Work

Optional later studies may include water, wind, Additional Lights, post processing, or LOD. They require separate design and validation and are not part of v0.3.0 or the planned v0.4.0 AI NPC milestone.
