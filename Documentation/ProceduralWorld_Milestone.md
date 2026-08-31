# v0.2.0 Procedural World Milestone

**English** | [简体中文](./ProceduralWorld_Milestone.zh-CN.md)

## Overview

v0.2.0 completes GenesisWorld's procedural-world foundation. It combines a generated terrain Mesh, deterministic seeds, terrain-aware environment placement, and curated low-poly assets while preserving module boundaries.

## Goals

- Learn and document procedural Mesh construction instead of hiding it behind a terrain package.
- Make terrain and environment results reproducible.
- Separate geometry, Unity lifecycle, and environment placement responsibilities.
- Produce an honest, presentable foundation for later graphics and intelligent-interaction research.

## Implemented Systems

- Grid vertices, triangle indices, UVs, normals, and bounds
- Centered Perlin-noise terrain height
- Seed-to-noise-offset mapping through local `System.Random`
- MeshCollider refresh and `TerrainGenerated` event
- Independent deterministic environment stream
- Surface raycasts, surface-normal slope filtering, spacing, margin, and center-clear rules
- Seeded tree/rock prefab selection, yaw, and uniform scale
- Three tree and three rock variants with URP materials and collision

## Development Timeline

| Commit | Step | Reason for separation |
|---|---|---|
| 5 | Flat grid Mesh | Establish topology before deformation |
| 6 | Perlin-noise terrain | Change height while preserving topology and UVs |
| 7 | World Seed | Add reproducibility after generation behavior was understood |
| 8 | Environment spawning | Build placement as a consumer of generated terrain |
| 9 | Low-poly asset integration | Improve presentation without mixing art work into algorithms |

## Architecture

`MeshGenerator` is a scene-independent data generator. `TerrainGenerator` owns Unity components and emits the terrain event. `EnvironmentSpawner` listens to terrain readiness and owns only the generated environment hierarchy. `PlayerController` and `CameraController` remain independent consumers of the playable scene.

## Procedural Pipeline

```text
World Seed → Perlin offset → grid heights → Mesh + MeshCollider
          ↘ independent environment seed → candidates → raycasts
            → slope/spacing filters → prefab variants → environment
```

## Determinism

Local `System.Random` streams isolate procedural state from `UnityEngine.Random`. The environment seed is derived independently from the World Seed. A/B/A runtime validation using `1001`, `2002`, and `1001` produced different A/B signatures and an identical repeated A signature.

## Environment and Third-party Integration

Tree and rock candidates are projected onto the exact terrain collider and filtered by surface normal and distance. The final presentation uses a minimal subset of Quaternius's Stylized Nature MegaKit under CC0 1.0. Models were wrapped, scaled, pivot-corrected, assigned URP/Lit materials, and given simple colliders; project-created placeholders remain available.

## Runtime Validation

- Seed `1001`: 18 trees, 12 rocks, layout signature `-1270850978`
- Seed `2002`: 18 trees, 12 rocks, layout signature `-96201934`
- Seed `1001` repeated: signature `-1270850978`
- Unity import and script refresh completed without compile errors during Commit 9 validation

## Known Limitations

- Small, single terrain; no chunks, streaming, infinite terrain, biome, or LOD system
- Basic material treatment; no advanced terrain texture blending or custom shader system
- Environment categories are limited to trees and rocks
- No water, weather, AI NPC, LLM interaction, or runtime AIGC integration
- Prototype controls and presentation are not a final game loop

## Lessons Learned

- Mesh topology and vertex deformation are easier to reason about when separated.
- Explicit seeds and independent random streams make procedural bugs reproducible.
- Raycasting provides a simple boundary between generated terrain and environment placement.
- Surface normals and spacing rules improve plausibility without a large world framework.
- Asset quality strongly affects how a technical prototype communicates its value.

## Next Phase

v0.3.0 will focus on rendering and shader development. Its scope will be planned separately; no rendering feature is implemented by this documentation milestone.
