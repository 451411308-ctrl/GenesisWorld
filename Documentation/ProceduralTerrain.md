# Procedural Terrain Foundation / 程序化地形基础

## Overview / 概述

### English

The procedural terrain system builds a reusable grid mesh and now samples Perlin Noise to produce continuous hills. The geometry pipeline remains independent from chunk streaming and gameplay logic.

### 中文

程序化地形系统以可复用规则网格为基础，现已通过 Perlin Noise 采样生成连续山丘。几何生成管线仍与区块流式加载和玩法逻辑保持独立。

## Architecture / 架构

```text
TerrainGenerator (Unity component / Unity 组件)
        |
        | generation parameters / 生成参数
        v
MeshGenerator (pure grid calculation / 纯网格计算)
        |
        v
GridMeshData (vertices, indices, UV / 顶点、索引、UV)
        |
        v
Unity Mesh
  |-- MeshFilter   -> geometry / 几何体
  |-- MeshRenderer -> material rendering / 材质渲染
  `-- MeshCollider -> collision / 碰撞
```

- `MeshGenerator` calculates grid data and has no scene or component responsibility.
- `TerrainGenerator` owns the Unity `Mesh`, applies the data, recalculates normals and bounds, and synchronizes the optional collider.
- `MeshGenerator` 负责规则网格数据计算，不承担场景或组件职责。
- `TerrainGenerator` 管理 Unity `Mesh`，写入数据、重新计算法线与包围盒，并按配置同步碰撞体。

## Mesh Concepts / 网格概念

### Mesh / 网格

A mesh is the geometric representation rendered by Unity. It combines vertex attributes with triangle indices.

网格是 Unity 用于渲染的几何表示，由顶点属性与三角形索引共同组成。

### Vertex / 顶点

A vertex stores a position in local space. The grid also assigns a UV coordinate to every vertex.

顶点记录局部空间中的位置。本规则网格还为每个顶点分配一个 UV 坐标。

### Triangle / 三角形

A triangle is the smallest surface primitive in the generated mesh. Every grid cell is divided into two triangles.

三角形是生成网格中的最小表面图元。每个网格单元被拆分为两个三角形。

### Triangle Index / 三角形索引

Triangle indices reference vertices in groups of three. The indices use Unity's upward-facing winding order on the XZ plane, producing positive Y surface normals.

三角形索引以每组三个索引引用顶点。在 XZ 平面上使用 Unity 的朝上绕序，从而生成 Y 轴正方向的表面法线。

### Normal / 法线

A normal describes the facing direction used by lighting calculations. This foundation calls `RecalculateNormals` after assigning the triangles.

法线描述表面朝向，供光照计算使用。本阶段在写入三角形后调用 `RecalculateNormals` 生成法线。

### UV

UV coordinates map the grid to the normalized `[0, 1]` texture space, allowing a material texture to cover the complete generated surface.

UV 坐标将规则网格映射到归一化的 `[0, 1]` 纹理空间，使材质纹理能够覆盖整个生成表面。

## Default Configuration / 默认配置

| Parameter / 参数 | Default / 默认值 |
|---|---:|
| Width / 宽度 | 20 |
| Depth / 深度 | 20 |
| X Segments / X 轴分段 | 20 |
| Z Segments / Z 轴分段 | 20 |
| Noise Scale / 噪声尺度 | 0.1 |
| Height Scale / 高度尺度 | 5 |
| Noise Offset / 噪声偏移 | (0, 0) |

The default mesh contains:

- Vertices: `(20 + 1) × (20 + 1) = 441`
- Triangles: `20 × 20 × 2 = 800`
- Triangle indices: `800 × 3 = 2400`

默认网格包含：

- 顶点：`(20 + 1) × (20 + 1) = 441`
- 三角形：`20 × 20 × 2 = 800`
- 三角形索引：`800 × 3 = 2400`

## Noise-based Height Generation / 基于噪声的高度生成

### English

Commit 5 established the grid topology:

```text
Vertex = (x, 0, z)
```

Commit 6 preserves the same vertices, triangle connections, and UV layout, but changes each vertex's Y coordinate:

```text
sampleX = (x + offsetX) * noiseScale
sampleZ = (z + offsetZ) * noiseScale
height  = (PerlinNoise(sampleX, sampleZ) - 0.5) * heightScale
Vertex  = (x, height, z)
```

`Mathf.PerlinNoise` is not independent random noise at every point. Neighboring samples change smoothly, unlike an abrupt sequence such as `1, 9, 2, 8`. This continuity makes Perlin Noise useful for terrain, clouds, textures, and procedural generation.

- **Noise Scale** controls sampling frequency. Lower values produce broad, gentle hills; higher values produce denser changes.
- **Height Scale** controls only the Y-axis amplitude. It does not change the mesh topology. A value of `0` produces a flat grid.
- **Noise Offset** moves the sampled region and produces a different terrain shape without introducing a random seed.

The noise value is centered from `[0, 1]` to approximately `[-0.5, 0.5]`. Therefore, the generated surface varies above and below local `Y = 0` instead of being lifted entirely upward. Normals and bounds are recalculated after height generation, and the `MeshCollider` receives the updated mesh.

### 中文

Commit 5 建立了规则网格拓扑：

```text
Vertex = (x, 0, z)
```

Commit 6 保持顶点数量、三角形连接关系和 UV 布局不变，只修改每个顶点的 Y 坐标：

```text
sampleX = (x + offsetX) * noiseScale
sampleZ = (z + offsetZ) * noiseScale
height  = (PerlinNoise(sampleX, sampleZ) - 0.5) * heightScale
Vertex  = (x, height, z)
```

`Mathf.PerlinNoise` 并不是每个点彼此独立的完全随机数。相邻采样值会平滑变化，不会像 `1、9、2、8` 这样突然跳变。因此，它适合用于地形、云层、纹理和程序化生成。

- **Noise Scale** 控制采样频率。较小值生成宽阔、平缓的山丘；较大值产生更密集的变化。
- **Height Scale** 只控制 Y 轴起伏幅度，不改变网格拓扑。设为 `0` 时得到平坦网格。
- **Noise Offset** 用于移动采样区域，在不引入随机种子的情况下得到不同地形形态。

实现中将 `[0, 1]` 的噪声值中心化到约 `[-0.5, 0.5]`，因此地形围绕局部 `Y = 0` 上下起伏，而不是整体向上抬升。高度生成后会重新计算法线和包围盒，并将更新后的网格同步给 `MeshCollider`。

## Unity Editor Setup / Unity 编辑器配置

1. Create an empty GameObject named `ProceduralTerrain`.
2. Add `TerrainGenerator`. Unity automatically adds `MeshFilter`, `MeshRenderer`, and `MeshCollider` through component requirements.
3. Assign a URP-compatible material to `MeshRenderer`.
4. Keep the default parameters or adjust the size, segment counts, and noise settings in the Inspector.
5. Enter Play Mode, or use **Generate Terrain** from the component context menu for an editor preview.

中文步骤：

1. 创建名为 `ProceduralTerrain` 的空 GameObject。
2. 添加 `TerrainGenerator`；Unity 会根据组件依赖自动添加 `MeshFilter`、`MeshRenderer` 和 `MeshCollider`。
3. 为 `MeshRenderer` 指定兼容 URP 的材质。
4. 保持默认参数，或在 Inspector 中调整尺寸、分段数量与噪声设置。
5. 进入 Play Mode；也可从组件上下文菜单选择 **Generate Terrain** 进行编辑器预览。

## Current Scope / 当前范围

This stage provides one centered Perlin Noise height layer. Random seeds, complex noise stacks, terrain chunks, streaming, level of detail, biome generation, and object placement remain outside the current scope.

本阶段仅提供一层中心化 Perlin Noise 高度。随机种子、复杂噪声叠加、地形区块、流式加载、细节层级、生态区域生成和物体放置仍不在当前范围内。
