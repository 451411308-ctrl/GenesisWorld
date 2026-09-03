# GenesisWorld Architecture

**English** | [简体中文](./Architecture.zh-CN.md)

## Design Goals

GenesisWorld uses explicit references and narrow responsibilities to keep gameplay, procedural algorithms, rendering, future AI services, and content workflows loosely coupled.

## Implemented Runtime Structure

```text
Unity Engine + URP
├── PlayerController ── updates player motion
├── CameraController ── observes CameraTarget
├── NPC Interaction [v0.4 In Progress]
│   ├── NPCProfile ── stable authored identity and local greeting data
│   ├── NPCActor ── scene entity and IInteractable implementation
│   ├── PlayerInteractionController ── camera targeting and input routing
│   └── DialogueController ── dialogue UI and player-input state
├── Procedural World [v0.2 Complete / CPU]
│   ├── MeshGenerator ── grid data and noise heights
│   ├── TerrainGenerator ── Mesh lifecycle and terrain event
│   └── EnvironmentSpawner ── deterministic surface placement
└── Rendering Layer [v0.3 Foundation Complete / GPU]
    ├── StylizedTerrain ── height/slope color, lighting, shadow, and fog
    ├── StylizedEnvironment ── textured light bands and alpha-aware shadows
    ├── StylizedSkybox ── view-direction gradient and atmospheric horizon
    └── RenderSettings ── skybox assignment and Linear Fog (no runtime manager)
```

Terrain generation and environment placement are separate: the terrain owns geometry and collision; the spawner waits for `TerrainGenerated` and owns only its generated hierarchy. Local `System.Random` streams make results reproducible without changing global Unity randomness.

The rendering boundary follows the same principle. CPU modules answer where geometry exists; custom GPU Shaders answer how visible surfaces look. Directional Light, shadow settings, Skybox, and Fog connect those surfaces into the final scene without changing procedural state.

The v0.4 interaction boundary now separates authored `NPCProfile` data from `NPCActor` scene entities. `PlayerInteractionController` owns camera raycasts and interact input, while `DialogueController` owns presentation and the temporary movement lock. The current response is a local profile greeting; no provider or networking layer exists yet.

## Future Layers

- The v0.3 stylized rendering foundation is complete. Future graphics studies such as water, wind, Additional Lights, post processing, or LOD remain optional independent work—not existing features and not a claim of a complete rendering engine.
- AI Interaction will next add a provider-independent conversation service and adapter interface. Context, memory, decisions, and scheduling remain future work.
- AIGC Content will be an editor/offline workflow whose outputs require review and optimization.

These are planned boundaries, not implemented runtime features.

## Dependency Principles

1. Depend on Inspector references, events, or abstractions—not hidden global state.
2. Keep procedural seeds explicit and algorithms independently testable.
3. Keep runtime code separate from editor and asset-production tooling.
4. Place shared code in Core only when ownership is genuinely cross-module.
5. Use `Resources` only when path-based loading is required; prefer serialized references and Prefabs.
