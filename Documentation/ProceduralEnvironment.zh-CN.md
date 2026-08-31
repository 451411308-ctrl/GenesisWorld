# 程序化环境

[English](./ProceduralEnvironment.md) | **简体中文**

## 概述

`EnvironmentSpawner` 在生成后的地形表面放置树木与岩石。它读取 World Seed 和地形生成事件，但拥有独立的确定性随机流与生成层级。

## 生成流程

```text
World Seed → 混合后的 Environment Seed → 局部 System.Random
→ 候选 X/Z → 地形 Raycast → 坡度/间距/中心过滤
→ Prefab Variant + Yaw + 等比缩放 → 生成实例
```

## 确定性

系统通过固定整数混合规则派生环境 Seed。局部 `System.Random` 决定位置、Prefab 选择、Y 轴旋转与缩放，不依赖时间或帧率，也不调用 `UnityEngine.Random`。

`相同 World Seed + 相同生成参数 + 相同 Prefab 数组 = 相同布局`

地形与环境使用独立随机流，一侧的随机调用变化不会自动消耗另一侧的序列。

## 表面放置与过滤

系统针对每个候选 X/Z 坐标向下发射射线，只接受 `TerrainGenerator.TerrainCollider`，避免 Player 和已生成物体的 Collider 干扰后续放置。`hit.point` 提供表面位置，`hit.normal` 提供坡度信息。

坡度通过 `Vector3.Angle(hit.normal, Vector3.up)` 计算。树木最大 30°，岩石最大 45°。通过坡度检查的位置还必须满足 XZ 最小间距、地形边缘留白和玩家出生区域的中心安全半径。

## 默认场景参数

| 参数 | 数值 |
|---|---:|
| 树木 / 岩石 | 18 / 12 |
| 边界间距 | 1 |
| 最小间距 | 1.85 |
| 树木 / 岩石最大坡度 | 30° / 45° |
| 树木缩放 | 0.90–1.12 |
| 岩石缩放 | 0.75–1.15 |
| 中心安全半径 | 2 |
| 单对象最大尝试次数 | 20 |

## Low-poly 集成

默认场景使用 Quaternius Stylized Nature MegaKit 精选 CC0 子集中的 2 种阔叶树、1 种松树与 3 种中型岩石。导入模型使用干净的 Prefab Root 包装，按 20×20 地形归一化比例，配置 URP/Lit 材质、底部 Pivot 和简化 Collider。项目自制资产继续作为轻量备用资源。

Mesh 统计：两种阔叶树分别为 8,219/5,648 与 5,639/4,066 顶点/三角形；松树为 5,522/4,964；岩石为 249–531 个顶点、244–522 个三角形。

## 重新生成与验证

Spawner 只清理自己管理的 `GeneratedEnvironment` 层级。地形重新生成后，事件会触发新的放置流程。使用 Seed `1001`、`2002`、`1001` 进行 A/B/A 验证，确认不同 Seed 产生不同布局，返回相同 Seed 后 Signature 完全一致。

## 当前限制

环境类别目前只有树木和岩石，尚无 Biome、Chunk、LOD、对象池流式加载、植被模拟或通用 World Manager。
