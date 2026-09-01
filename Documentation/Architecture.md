# GenesisWorld Architecture

**English** | [简体中文](./Architecture.zh-CN.md)

## Design Goals

GenesisWorld uses explicit references and narrow responsibilities to keep gameplay, procedural algorithms, rendering, future AI services, and content workflows loosely coupled.

## Implemented Runtime Structure

```text
Unity Engine + URP
├── PlayerController ── updates player motion
├── CameraController ── observes CameraTarget
├── Procedural World
    ├── MeshGenerator ── grid data and noise heights
    ├── TerrainGenerator ── Mesh lifecycle and terrain event
    └── EnvironmentSpawner ── deterministic surface placement
└── Rendering Layer [In Progress]
    ├── StylizedTerrain ── height/slope color and directional lighting
    └── StylizedEnvironment ── texture-preserving light bands and alpha-aware shadows
```

Terrain generation and environment placement are separate: the terrain owns geometry and collision; the spawner waits for `TerrainGenerated` and owns only its generated hierarchy. Local `System.Random` streams make results reproducible without changing global Unity randomness.

## Future Layers

- Rendering and Shader development is in progress. `StylizedTerrain` handles generated ground, while `StylizedEnvironment` applies the shared lighting direction to textured tree and rock Prefabs. A complete rendering layer is not yet finished.
- AI Interaction will later isolate NPC context, decisions, scheduling, and provider adapters.
- AIGC Content will be an editor/offline workflow whose outputs require review and optimization.

These are planned boundaries, not implemented runtime features.

## Dependency Principles

1. Depend on Inspector references, events, or abstractions—not hidden global state.
2. Keep procedural seeds explicit and algorithms independently testable.
3. Keep runtime code separate from editor and asset-production tooling.
4. Place shared code in Core only when ownership is genuinely cross-module.
5. Use `Resources` only when path-based loading is required; prefer serialized references and Prefabs.
