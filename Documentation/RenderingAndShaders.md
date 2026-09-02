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

Same-camera runtime comparisons covered two, three, and four light steps plus hard and soft shadows. Three light steps provided the clearest balance. Hard shadows were selected for the test scene because their silhouettes match the low-poly/cel presentation better; light bias remains `0.05`, normal bias `0.4`, near plane `0.2`, and the High Fidelity quality profile now uses a `2048` main-light shadow map, `40`-unit distance, and two cascades. These values replace the template's excessive `4096` / `150` / four-cascade settings for this compact `20×20` scene. Commit 13 now uses the existing Fog compatibility as described below.

## Atmospheric Rendering

Commit 13 connects the terrain and environment Shaders to an authored atmosphere rather than adding another surface effect. The final direction is a clear stylized day: cool blue zenith, pale blue-green horizon, restrained linear distance fog, slightly warm sunlight, and the existing hard-shadow language. No Volume, HDRI, physical scattering, clouds, or ray marching was added.

The atmosphere pipeline is intentionally small:

```text
Camera view direction → StylizedSkybox gradient
Object camera distance → Unity linear fog factor
Terrain / Environment / Player color → MixFog toward horizon color
Warm Directional Light + hard shadows → local form and contact
```

## Stylized Skybox

`GenesisWorld/StylizedSkybox` is a hand-written URP Shader with four material parameters:

| Property | Final value | Purpose |
|---|---:|---|
| Zenith Color | `(0.18, 0.42, 0.72)` | Cool upper-sky color |
| Horizon Color | `(0.72, 0.84, 0.82)` | Pale atmospheric transition and fog target |
| Lower Color | `(0.32, 0.38, 0.28)` | Natural lower hemisphere when the terrain edge is visible |
| Horizon Exponent | `0.65` | Controls the width and softness of the horizon transition |

## View Direction

A skybox does not need a world-space location for every visible sky pixel. It needs the direction in which the camera is looking. The vertex stage transforms the skybox cube direction to world space; the fragment stage normalizes it and reads `viewDirection.y`.

- Positive Y points toward the zenith.
- Values near zero point toward the horizon.
- Negative Y points into the lower hemisphere.

## Horizon Gradient

The Shader uses `smoothstep` followed by `pow` rather than a mechanical linear ramp:

```text
upper = pow(smoothstep(0, 1, saturate(viewDirection.y)), HorizonExponent)
lower = pow(smoothstep(0, 1, saturate(-viewDirection.y)), HorizonExponent)
upperSky = lerp(HorizonColor, ZenithColor, upper)
lowerSky = lerp(HorizonColor, LowerColor, lower)
```

The two gradients meet at the same Horizon Color, avoiding a visible seam. Full `float` precision is used for the direction and factors; runtime inspection found no pink sky, black sky, seam, or distracting banding.

## Fog

The scene now uses Unity Linear Fog with Start `12` and End `40`. Fog is not a transparent grey plane: each compatible Shader calculates a camera-distance Fog Factor and mixes its surface result toward the configured Fog Color.

```text
FinalColor = lerp(ObjectColor, FogColor, FogFactor)
```

Runtime comparison covered Fog Off, `12–40`, and `10–32`. Fog Off separated distant geometry from the horizon; `10–32` washed out too much of the compact world; `12–40` retained tree/rock color while still providing atmospheric depth. Terrain, bark, alpha-clipped leaves, rocks, shadows, and the Player all remained visually valid.

## Fog and Horizon Matching

Fog Color exactly matches the material Horizon Color: `(0.72, 0.84, 0.82)`. Distant surfaces therefore approach the same color already behind them instead of producing a grey or blue discontinuity. This depth cue makes the small procedural world read as a coherent space without pretending it is infinite.

## Directional Light Polish

Two light directions were compared. A new `42°, -55°, 0°` side angle strengthened facets but produced overly dark foreground foliage and longer shadows. The existing `48°, -32°, 0°` direction gave the better balance, so rotation, warm color `(1.00, 0.94, 0.84)`, and intensity `1.15` remain unchanged. Skybox Ambient mode and intensity `1.0` also remain unchanged; custom terrain/environment materials retain their `0.32–0.35` ambient terms so back faces stay readable without flattening the lit bands.

## Shadow Distance

Commit 12's stable configuration is preserved: Hard Shadows, `2048` main-light shadow map, `40` distance, two cascades, Bias `0.05`, Normal Bias `0.4`, and Near Plane `0.2`. Shadow Distance now matches Fog End, avoiding work on shadows that would be hidden beyond the atmospheric range. Trees, rocks, terrain, and Player remained free of the earlier elongated foliage artifact.

## Atmosphere Pipeline

The final atmosphere is scene configuration plus one sky Shader—not a new runtime manager. It remains independent from `MeshGenerator`, `TerrainGenerator`, `EnvironmentSpawner`, Player, Camera, and World Seed. Same-seed regeneration retained signature `2087925580` with `18` trees and `12` rocks.

## Runtime Validation

- Both custom Shaders supported; all four environment adapter materials retained their source textures
- Height range: `-1.755–2.029`; maximum slope: `28.348°`
- Trees/rocks: `18/12`
- Repeated generation signature: `2087925580` both times
- All six environment Prefabs use the custom Shader and retain enabled colliders
- Player present and grounded after startup; terrain collision remained active
- No C# or Shader compilation errors were found
- Fog Off / `12–40` / `10–32`, two palettes, two light angles, and ground/elevated/horizon views were compared in Play Mode
- Final skybox assigned through RenderSettings; Camera remained in Skybox clear mode

## Current Limitations

The terrain Shader uses vertex normals generated by `RecalculateNormals`, so the terrain remains smoothly shaded. The rendering layer has no terrain texture layers, triplanar mapping, environment normal maps, additional-light loop, physical atmospheric scattering, volumetric fog, clouds, post-processing Volume, custom GI, or platform-specific shadow tuning. The environment adapter deliberately implements direct/ambient banding rather than the full URP Lit PBR feature set.

## Next Rendering Steps

Future rendering commits may address terrain/environment color harmony, distance readability, and platform-specific shadow quality. Texture blending and more advanced techniques should remain separate, measurable additions.
