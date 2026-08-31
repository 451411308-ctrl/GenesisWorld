# Procedural Environment

**English** | [简体中文](./ProceduralEnvironment.zh-CN.md)

## Overview

`EnvironmentSpawner` places trees and rocks on the generated terrain. It consumes the World Seed and the terrain generation event but owns a separate deterministic random stream and generated hierarchy.

## Pipeline

```text
World Seed → mixed Environment Seed → local System.Random
→ candidate X/Z → terrain raycast → slope/spacing/center filters
→ prefab variant + yaw + uniform scale → generated instance
```

## Determinism

A fixed integer mixing rule derives the environment seed. The local `System.Random` controls positions, prefab choices, Y rotation, and scale. It does not depend on time or frame rate and does not call `UnityEngine.Random`.

`Same World Seed + same spawn parameters + same prefab arrays = same layout`

The terrain and environment use independent streams, so changes in one random sequence do not automatically consume values from the other.

## Surface Placement and Filtering

For each candidate X/Z point, a downward raycast accepts only `TerrainGenerator.TerrainCollider`. This prevents the Player and previously spawned colliders from affecting later placement. `hit.point` supplies the surface position and `hit.normal` supplies slope information.

The slope is `Vector3.Angle(hit.normal, Vector3.up)`. Trees allow up to 30°, rocks 45°. Accepted XZ positions must also satisfy minimum spacing, a terrain-edge margin, and a center clear radius for player spawn.

## Default Scene Parameters

| Parameter | Value |
|---|---:|
| Trees / Rocks | 18 / 12 |
| Spawn margin | 1 |
| Minimum spacing | 1.85 |
| Tree / Rock max slope | 30° / 45° |
| Tree scale | 0.90–1.12 |
| Rock scale | 0.75–1.15 |
| Center clear radius | 2 |
| Attempts per object | 20 |

## Low-poly Integration

The default scene uses two common trees, one pine, and three medium rocks from a curated CC0 subset of Quaternius's Stylized Nature MegaKit. Each imported model is wrapped in a clean prefab root, normalized for the 20×20 terrain, assigned URP/Lit materials, corrected to a ground-level pivot, and given a simplified collider. Project-created assets remain available as lightweight fallbacks.

Mesh statistics: Common trees contain 8,219/5,648 and 5,639/4,066 vertices/triangles; the pine contains 5,522/4,964; rocks range from 249–531 vertices and 244–522 triangles.

## Regeneration and Validation

The spawner clears only its `GeneratedEnvironment` hierarchy. After terrain regeneration, the event triggers a new placement pass. A/B/A validation with seeds `1001`, `2002`, and `1001` confirmed different layouts for different seeds and identical signatures when returning to the same seed.

## Current Limitations

Categories are limited to trees and rocks. There are no biomes, chunks, LOD, pooled streaming, vegetation simulation, or generalized world-management framework.
