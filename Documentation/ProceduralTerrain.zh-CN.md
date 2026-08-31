# 程序化地形

[English](./ProceduralTerrain.md) | **简体中文**

## 概述

地形模块构建以原点为中心的规则网格 Mesh，并使用 Perlin Noise 改变顶点高度。`MeshGenerator` 只计算数据，不持有场景对象；`TerrainGenerator` 负责将结果应用到 Unity 组件并管理重新生成。

## 职责划分

| 组件 | 职责 |
|---|---|
| `GridMeshData` | 保存不可变的顶点、三角形索引和 UV 结果 |
| `MeshGenerator` | 校验参数并生成拓扑与高度 |
| `TerrainGenerator` | 管理参数、Seed 映射、Mesh 生命周期、MeshCollider 与 `TerrainGenerated` 事件 |

## 网格构建

对于 `xSegments × zSegments` 个网格单元：

- 顶点数：`(xSegments + 1) × (zSegments + 1)`
- 三角形数：`xSegments × zSegments × 2`
- 索引：每个单元 6 个，并保持统一向上的绕序
- UV：归一化到 `0–1`
- 法线与包围盒：赋值几何数据后重新计算

顶点保存三维位置，三角形引用三个顶点索引。相邻单元共享顶点，从而形成连续表面。`TerrainGenerator` 根据顶点数选择 16 位或 32 位索引。

## 噪声高度采样

每个局部 X/Z 坐标应用尺度与偏移后采样 `Mathf.PerlinNoise`：

```text
sample = (position + finalNoiseOffset) × noiseScale
height = (PerlinNoise(sampleX, sampleZ) - 0.5) × heightScale
```

将 `0–1` 噪声范围中心化后，地形会围绕局部 `Y = 0` 起伏，同时三角形拓扑与 UV 保持不变。

## Seed 确定性

`TerrainGenerator` 使用局部 `System.Random` 将 `worldSeed` 稳定映射到二维噪声偏移；手动偏移仍可用于调试采样区域。

```text
相同 Seed + 相同地形参数 = 相同顶点与地形
```

局部随机源不会修改 `UnityEngine.Random`，因此地形生成不会意外影响其他系统的随机状态。

## Unity 集成

`TerrainGenerator` 要求存在 `MeshFilter`、`MeshRenderer` 和 `MeshCollider`。生成时依次写入顶点、三角形和 UV，重新计算法线与包围盒，刷新碰撞体、记录统计信息，最后触发 `TerrainGenerated`。网格只在明确生成时构建，不会每帧重复计算。

## 当前范围

当前地形是单个小型规则网格。Chunk Streaming、无限地形、Biome、LOD、高级地形纹理、水体、侵蚀和编辑器工具仍属于未来工作。
