# Rendering and Shaders

**English** | [简体中文](./RenderingAndShaders.zh-CN.md)

## Rendering Foundation

GenesisWorld uses Unity `2022.3.62f3`, Universal Render Pipeline `14.0.12`, ShaderLab, and hand-written HLSL. Commit 11 introduces `GenesisWorld/StylizedTerrain`, the project's first custom URP Shader. The implementation is intentionally color-based and compact so its graphics concepts remain visible and maintainable.

## CPU Geometry vs GPU Shading

The CPU and GPU solve different parts of the terrain:

- C# (`MeshGenerator` and `TerrainGenerator`) answers: **Where are the vertices?** It creates topology, samples Perlin noise, assigns height, and updates collision.
- The GPU Shader answers: **What should the surface look like?** It uses the finished geometry's world position, normal, and lighting to calculate each visible color.

The fragment Shader does not resample Perlin noise or modify procedural generation.

## Rendering Pipeline Overview

```text
Vertex position + normal
        ↓ Vertex stage
Object → World → Clip position
        ↓ Interpolated values
World height + world normal + main light
        ↓ Fragment stage
Height color → slope blend → stylized Lambert light → fog
        ↓
Final pixel color
```

The vertex stage transforms each vertex to clip space for rasterization and passes world-space position and a correctly transformed normal. The fragment stage chooses terrain colors and lighting for the resulting fragments/pixels.

## Height-based Terrain Color

World-space height is normalized to `0–1`:

```text
heightFactor = saturate((worldY - HeightMin) / max(HeightMax - HeightMin, 0.0001))
heightColor = lerp(LowColor, HighColor, heightFactor)
```

`lerp` selects the low color at 0, the high color at 1, and a proportional blend in between. The epsilon prevents division by zero. World space makes the behavior explicit even if the terrain GameObject moves.

## Slope Detection and Surface Normals

A surface normal is a unit vector pointing away from the surface. The Mesh normals are transformed and normalized in world space before comparison with World Up:

```text
upAlignment = saturate(dot(normalWS, float3(0, 1, 0)))
slope = 1 - upAlignment
slopeFactor = smoothstep(SlopeStart, SlopeEnd, slope)
```

For flat ground, `N ≈ (0,1,0)`, so `dot(N, Up) ≈ 1` and slope approaches 0. As the surface tilts, the dot product decreases and the slope factor rises. `smoothstep` avoids a hard grass-to-rock boundary.

## Dot Product and Basic Lighting

The dot product `dot(N, L)` measures directional alignment between the normalized surface normal and main-light direction. A surface facing the light has a larger value and appears brighter; a surface facing away approaches zero.

The Shader uses Lambert diffuse multiplied by URP main-light color, distance attenuation, and shadow attenuation. `_AmbientStrength` adds a minimum light level so unlit faces do not become pure black. This is a lightweight stylized model rather than a full PBR BRDF.

## Shader Parameters

| Group | Property | Default | Purpose |
|---|---|---:|---|
| Height | Low Color | `(0.10, 0.24, 0.07)` | Low-elevation dark grass |
| Height | High Color | `(0.48, 0.62, 0.24)` | High-elevation dry/light grass |
| Height | Height Min / Max | `-2.5 / 2.5` | World-height normalization range |
| Slope | Slope Color | `(0.36, 0.32, 0.27)` | Rock/earth color for steep surfaces |
| Slope | Slope Start / End | `0.04 / 0.12` | Smooth slope transition |
| Lighting | Ambient Strength | `0.32` | Minimum brightness |

The height range follows the current `heightScale = 5`. Runtime measurement for Seed `12345` produced world heights from `-1.755` to `2.029` and a maximum vertex-normal slope of `28.348°`.

## URP Integration

- `UniversalForward` pass with URP `Core.hlsl` and `Lighting.hlsl`
- Main directional light color/direction and shadow attenuation variants
- Custom `ShadowCaster` and `DepthOnly` passes preserve alpha-clipped silhouettes
- Fog variants through `ComputeFogFactor` and `MixFog`
- Material values stored in `UnityPerMaterial` CBUFFER for SRP Batcher compatibility

## Stylized Environment Shader

Commit 12 adds `GenesisWorld/StylizedEnvironment` for the generated trees and rocks. The Shader multiplies each asset's original base texture by its existing tint, evaluates `dot(N,L)` in world space, adds a small wrapped-light term, and quantizes the result into `_LightSteps`. The default is three bands: enough to make low-poly planes readable without flattening every texture detail.

Low-poly faces have different normals even when they are neighbors. Their `N·L` values therefore differ under one light direction, which is what makes the mesh facets readable. Instead of leaving continuous Lambert samples such as `0.12, 0.35, 0.63, 0.91`, quantization maps them to a small set of stable brightness levels:

```text
wrapped = saturate((saturate(dot(normalWS, lightDirectionWS)) + LightWrap) / (1 + LightWrap))
banded = round(wrapped * (LightSteps - 1)) / (LightSteps - 1)
final = BaseMap × BaseColor × (AmbientStrength + MainLightColor × banded × attenuation)
```

`LightSteps` is clamped to at least two in HLSL, avoiding division by zero.

Four project-owned adapter materials cover bark, common leaves, pine leaves, and rocks. They reference the existing CC0 textures rather than copying or editing third-party source assets. Leaf materials keep `_ALPHATEST_ON` and a `0.5` cutoff, so transparent parts of the foliage texture are rejected consistently in Forward, Depth, and ShadowCaster passes.

| Property | Default | Purpose |
|---|---:|---|
| Base Map | Asset texture / white fallback | Authored surface detail or texture-free material support |
| Base Color | Existing material tint | Multiplies the sampled texture |
| Light Steps | `3` | Number of discrete diffuse brightness bands |
| Ambient Strength | `0.32` (`0.35` leaves) | Prevents back-facing surfaces from becoming black |
| Light Wrap | `0.20` (`0.25` leaves) | Keeps foliage interiors readable |
| Alpha Cutoff | `0.50` leaves | Preserves foliage cutout silhouettes |

The environment and terrain Shaders have different surface responsibilities but share the same main Directional Light:

- `StylizedTerrain` derives color from procedural height and slope because the generated ground has no authored texture set.
- `StylizedEnvironment` preserves authored textures and tints, then adds adjustable banded light to tree and rock normals.

## Shadow Notes

Commit 11 found jagged, elongated foliage silhouettes when Soft Shadows were enabled, so that scene remained shadowless. Commit 12 traced the runtime Prefabs and material alpha settings, added an alpha-aware custom ShadowCaster pass, and compared None, Hard, and Soft modes after allowing URP to rebuild its shadow resources between captures.

Same-camera runtime comparisons covered two, three, and four light steps plus hard and soft shadows. Three light steps provided the clearest balance. Hard shadows were selected for the test scene because their silhouettes match the low-poly/cel presentation better; light bias remains `0.05`, normal bias `0.4`, near plane `0.2`, and the High Fidelity quality profile now uses a `2048` main-light shadow map, `40`-unit distance, and two cascades. These values replace the template's excessive `4096` / `150` / four-cascade settings for this compact `20×20` scene. Fog compatibility exists, but the scene does not enable a new fog system.

## Runtime Validation

- Both custom Shaders supported; all four environment adapter materials retained their source textures
- Height range: `-1.755–2.029`; maximum slope: `28.348°`
- Trees/rocks: `18/12`
- Repeated generation signature: `2087925580` both times
- All six environment Prefabs use the custom Shader and retain enabled colliders
- Player present and grounded after startup; terrain collision remained active
- No C# or Shader compilation errors were found

## Current Limitations

The terrain Shader uses vertex normals generated by `RecalculateNormals`, so the terrain remains smoothly shaded. The rendering layer has no terrain texture layers, triplanar mapping, environment normal maps, additional-light loop, custom GI, or platform-specific shadow tuning. The environment adapter deliberately implements direct/ambient banding rather than the full URP Lit PBR feature set.

## Next Rendering Steps

Future rendering commits may address terrain/environment color harmony, distance readability, and platform-specific shadow quality. Texture blending and more advanced techniques should remain separate, measurable additions.
