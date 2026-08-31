# Procedural Terrain

**English** | [简体中文](./ProceduralTerrain.zh-CN.md)

## Overview

The terrain module constructs a centered grid Mesh and deforms its vertex heights with Perlin noise. `MeshGenerator` computes data without scene ownership; `TerrainGenerator` applies it to Unity components and controls regeneration.

## Responsibilities

| Component | Responsibility |
|---|---|
| `GridMeshData` | Immutable vertices, triangle indices, and UV result |
| `MeshGenerator` | Validates parameters and generates topology and heights |
| `TerrainGenerator` | Owns parameters, seed mapping, Mesh lifecycle, MeshCollider, and `TerrainGenerated` event |

## Mesh Construction

For `xSegments × zSegments` cells:

- Vertices: `(xSegments + 1) × (zSegments + 1)`
- Triangles: `xSegments × zSegments × 2`
- Indices: six per cell, with consistent upward winding
- UVs: normalized to `0–1`
- Normals and bounds: recalculated after geometry assignment

A vertex stores a 3D position; a triangle references three vertex indices. Shared grid vertices produce continuous geometry. `TerrainGenerator` selects 16- or 32-bit indices from vertex count.

## Noise Height Sampling

Each local X/Z position samples `Mathf.PerlinNoise` after applying scale and offset:

```text
sample = (position + finalNoiseOffset) × noiseScale
height = (PerlinNoise(sampleX, sampleZ) - 0.5) × heightScale
```

Centering the `0–1` noise range around zero lets terrain rise and fall around local `Y = 0` while topology and UVs remain unchanged.

## Seed Determinism

`TerrainGenerator` maps `worldSeed` to a stable two-dimensional noise offset using a local `System.Random`. A manual offset can still move the sampling window for debugging.

```text
Same seed + same terrain parameters = same vertices and terrain
```

The local random source does not modify `UnityEngine.Random`, so terrain generation cannot unexpectedly change unrelated gameplay randomness.

## Unity Integration

`TerrainGenerator` requires `MeshFilter`, `MeshRenderer`, and `MeshCollider`. Generation assigns vertices, triangles, and UVs, recalculates normals and bounds, refreshes the collider, records statistics, then raises `TerrainGenerated`. Mesh construction occurs on explicit generation rather than every frame.

## Current Scope

The current terrain is a single small grid. Chunk streaming, infinite terrain, biomes, LOD, advanced terrain texturing, water, erosion, and editor tooling remain future work.
