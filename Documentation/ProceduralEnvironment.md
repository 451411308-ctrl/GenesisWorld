# Procedural Environment Generation / 程序化环境生成

## Overview / 概述

### English

Commit 8 adds deterministic tree and rock placement on top of the generated terrain. `EnvironmentSpawner` uses the existing World Seed and configurable spawn rules; it does not implement biomes, chunks, or a general world-management framework.

### 中文

Commit 8 在程序化地形表面加入确定性的树木与岩石放置。`EnvironmentSpawner` 使用现有 World Seed 和可配置生成规则，不包含生态区域、区块或通用世界管理框架。

## Architecture / 架构

```text
World Seed
    |
    v
Environment Seed (independent deterministic stream)
    |
    v
Local System.Random
    |
    v
Candidate Position
    |
    v
Terrain Raycast
    |
    v
Slope + Spacing + Center Safety Filters
    |
    v
Prefab Selection + Yaw + Uniform Scale
    |
    v
Generated Environment
    |-- Trees
    `-- Rocks
```

- `TerrainGenerator` remains responsible for terrain mesh generation and exposes only the seed, dimensions, height scale, collider, and generation event required by the environment layer.
- `EnvironmentSpawner` derives an independent environment seed, places instances, owns the generated hierarchy, and clears only that hierarchy before regeneration.
- `TerrainGenerator` 继续负责地形网格，只向环境层提供必要的 Seed、尺寸、高度尺度、碰撞体和生成完成事件。
- `EnvironmentSpawner` 派生独立环境 Seed、放置实例、管理生成层级，并在重新生成前只清理该层级。

## Determinism / 确定性

### English

The environment seed is derived from the World Seed with a fixed integer mixing rule. A local `System.Random` then controls candidate positions, prefab choices, Y-axis rotations, and uniform scales. The implementation does not call `UnityEngine.Random` and does not depend on dates or frame timing.

```text
Same World Seed + Same Spawn Parameters + Same Prefabs
= Same Environment Layout
```

The environment stream is separated from terrain offset generation. Changing the internal random calls used by terrain generation therefore does not automatically shift the complete environment sequence.

### 中文

环境 Seed 通过固定整数混合规则从 World Seed 派生。局部 `System.Random` 统一决定候选位置、Prefab 选择、Y 轴旋转和等比缩放。实现不调用 `UnityEngine.Random`，也不依赖日期或帧时间。

```text
相同 World Seed + 相同生成参数 + 相同 Prefab
= 相同环境布局
```

环境随机流与地形偏移随机流相互独立，因此未来调整地形内部随机调用时，不会自动改变完整环境生成序列。

## Surface Placement / 表面放置

### English

A raycast works like an invisible ray fired downward from the sky. For each candidate X/Z coordinate, the spawner casts downward and accepts only the exact `MeshCollider` owned by `TerrainGenerator`:

- `hit.point` provides the terrain surface position.
- `hit.normal` provides the surface-facing direction.

Filtering by collider identity prevents the Player and previously generated tree or rock colliders from interfering with later placement passes.

### 中文

Raycast 可以理解为从天空向下发射一条不可见射线。每个候选 X/Z 坐标都会向下检测，并且只接受 `TerrainGenerator` 对应的准确 `MeshCollider`：

- `hit.point` 表示地形表面坐标。
- `hit.normal` 表示表面朝向。

通过严格比较碰撞体，Player 以及先前生成的树木或岩石 Collider 不会干扰后续放置。

## Slope Filtering / 坡度过滤

On a flat surface, the normal is close to `Vector3.up`. On a slope, the normal tilts. The spawner calculates:

```text
slopeAngle = Vector3.Angle(hit.normal, Vector3.up)
```

Trees default to a maximum slope of 30 degrees, while rocks allow up to 45 degrees. Both remain upright and receive only deterministic Y-axis rotation.

平面法线接近 `Vector3.up`，坡面法线会倾斜。系统通过上述夹角得到坡度。树木默认限制为 30 度以内，岩石允许到 45 度；两类对象都保持直立，只进行确定性的 Y 轴旋转。

## Spawn Spacing / 生成间距

The generator stores accepted positions and compares XZ-plane distances before placing another object. The default 1.2-unit minimum spacing prevents obvious overlap without introducing complex Poisson-disk sampling. A 1-unit edge margin avoids clipped objects, and a 2-unit clear radius keeps the terrain center safe for the current player spawn.

生成器记录已接受的位置，并在放置新对象前比较 XZ 平面距离。默认 1.2 单位的最小间距用于避免明显重叠，无需引入复杂泊松圆盘采样；1 单位边界间距避免物体卡在地形边缘，2 单位中心安全半径为当前玩家出生位置保留空间。

## Default Parameters / 默认参数

| Parameter / 参数 | Default / 默认值 |
|---|---:|
| Tree Count / 树木数量 | 30 |
| Rock Count / 岩石数量 | 15 |
| Spawn Margin / 边界间距 | 1 |
| Minimum Spacing / 最小间距 | 1.2 |
| Tree Max Slope / 树木最大坡度 | 30° |
| Rock Max Slope / 岩石最大坡度 | 45° |
| Tree Scale / 树木缩放 | 0.8–1.2 |
| Rock Scale / 岩石缩放 | 0.7–1.3 |
| Center Clear Radius / 中心安全半径 | 2 |
| Max Attempts per Object / 单对象最大尝试次数 | 20 |

## Current Limitations / 当前限制

This version uses simple placeholder assets and one finite terrain. Biomes, chunks, infinite terrain, streaming, LOD, pooling, GPU vegetation, and environment save data are not implemented.

本版本使用简单占位资产与单块有限地形。生态区域、区块、无限地形、流式加载、LOD、对象池、GPU 植被和环境存档均未实现。
