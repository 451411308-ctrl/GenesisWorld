# Procedural Terrain Foundation / 程序化地形基础

## Overview / 概述

### English

The first procedural environment milestone builds a flat, reusable grid mesh. It establishes the geometry pipeline needed by later terrain algorithms without introducing noise, chunk streaming, or gameplay logic.

### 中文

程序化环境的首个里程碑实现了一个平坦、可复用的规则网格。它为后续地形算法建立几何生成管线，但本阶段不引入噪声、区块流式加载或玩法逻辑。

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

The default mesh contains:

- Vertices: `(20 + 1) × (20 + 1) = 441`
- Triangles: `20 × 20 × 2 = 800`
- Triangle indices: `800 × 3 = 2400`

默认网格包含：

- 顶点：`(20 + 1) × (20 + 1) = 441`
- 三角形：`20 × 20 × 2 = 800`
- 三角形索引：`800 × 3 = 2400`

## Unity Editor Setup / Unity 编辑器配置

1. Create an empty GameObject named `ProceduralTerrain`.
2. Add `TerrainGenerator`. Unity automatically adds `MeshFilter`, `MeshRenderer`, and `MeshCollider` through component requirements.
3. Assign a URP-compatible material to `MeshRenderer`.
4. Keep the default parameters or adjust the size and segment counts in the Inspector.
5. Enter Play Mode, or use **Generate Terrain** from the component context menu for an editor preview.

中文步骤：

1. 创建名为 `ProceduralTerrain` 的空 GameObject。
2. 添加 `TerrainGenerator`；Unity 会根据组件依赖自动添加 `MeshFilter`、`MeshRenderer` 和 `MeshCollider`。
3. 为 `MeshRenderer` 指定兼容 URP 的材质。
4. 保持默认参数，或在 Inspector 中调整尺寸与分段数量。
5. 进入 Play Mode；也可从组件上下文菜单选择 **Generate Terrain** 进行编辑器预览。

## Current Scope / 当前范围

This commit only provides the flat grid mesh foundation. Height noise, terrain chunks, streaming, level of detail, biome generation, and object placement are reserved for later development.

本次提交仅提供平坦规则网格基础。高度噪声、地形区块、流式加载、细节层级、生态区域生成和物体放置将在后续阶段实现。
