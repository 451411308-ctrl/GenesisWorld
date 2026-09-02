# GenesisWorld Development Log

**English** | [简体中文](./DevelopmentLog.zh-CN.md)

## Week 1 — Core Framework

### Commit 1 — `Initialize GenesisWorld project structure`
Initialized Unity 2022 LTS, modular assets, URP/project settings, Git rules, and documentation.

### Commit 2 — `Add player controller system`
Added CharacterController movement, sprint, jump, ground detection, gravity, configurable parameters, and Animator parameter support.

### Commit 3 — `Add third person camera system`
Added target follow, mouse orbit, pitch clamp, smoothing, zoom, cursor handling, and camera-relative movement.

### Commit 4 — `Update documentation and create Week 1 milestone`
Published the core-framework summary and annotated `v0.1.0` tag.

## Week 2 — Procedural World

### Commit 5 — `Add procedural terrain generation foundation`
Separated flat grid data generation from Unity Mesh ownership; generated vertices, triangles, UVs, normals, bounds, and collision.

### Commit 6 — `Implement noise-based terrain generation`
Added centered Perlin-noise heights with configurable frequency, amplitude, and offset.

### Commit 7 — `Add seeded procedural world generation`
Mapped World Seed to a stable noise offset with local `System.Random` while preserving a manual debug offset.

### Commit 8 — `Add procedural environment spawning`
Added deterministic trees and rocks through an independent stream, terrain raycasts, slope filtering, spacing, and regeneration events.

### Commit 9 — `Integrate low-poly environment`
Added project-created low-poly fallback variants, URP materials, colliders, and the first real Game View showcase.

### Commit 9 Art Upgrade — `Integrate curated third-party environment assets`
Integrated a minimal CC0 subset of Quaternius's Stylized Nature MegaKit, rebuilt URP materials, normalized prefabs, documented licensing, and validated seed A/B/A reproducibility.

### Commit 10 — `Update procedural world documentation and milestone`
Separated public documentation into English and Simplified Chinese files, redesigned the GitHub landing page, documented the complete pipeline, and prepared the `v0.2.0` Procedural World milestone.

## Week 3 — Rendering and Shaders

### Commit 11 — `Add stylized terrain shader foundation`
Introduced the first custom URP terrain Shader. It uses world-space height, world-space surface normals, smooth slope blending, main directional Lambert lighting, adjustable ambient strength, shadow variants, and Fog compatibility to create a parameterized stylized terrain appearance. The original terrain material remains available as a fallback.

### Commit 12 — `Add stylized environment lighting`
Added a custom URP environment Shader with configurable light bands, wrapped diffuse response, ambient fill, source-texture and color preservation, alpha-clipped foliage, and alpha-aware depth/shadow passes. Project-owned adapter materials now style all integrated tree and rock Prefabs without modifying the CC0 source assets. Hard directional shadows were selected after same-camera hard/soft runtime comparison.

### Commit 13 — `Add atmospheric rendering polish`
Added a hand-written gradient skybox, scale-matched Linear Fog, coordinated horizon/fog colors, and runtime presentation polish. Play Mode comparisons covered Fog Off and two ranges, neutral and warm palettes, original and side-light angles, and ground/elevated/horizon views. The final clear-day setup uses Fog `12–40`, preserves the balanced warm Directional Light and Commit 12 shadow settings, adds no post-processing Volume, and leaves procedural/gameplay systems unchanged.
