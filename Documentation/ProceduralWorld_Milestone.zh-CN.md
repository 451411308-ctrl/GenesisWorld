# v0.2.0 程序化世界里程碑

[English](./ProceduralWorld_Milestone.md) | **简体中文**

## 阶段概述

v0.2.0 完成 GenesisWorld 的程序化世界基础，将程序化地形 Mesh、确定性 Seed、适应地形表面的环境放置与精选 Low-poly 资产组合起来，同时保持模块职责边界。

## 阶段目标

- 不依赖现成地形包，理解并记录程序化 Mesh 构建过程。
- 让地形与环境结果可复现。
- 分离几何计算、Unity 生命周期与环境放置职责。
- 为后续图形学和智能交互研究建立真实、可展示的技术基础。

## 已实现系统

- 规则网格顶点、三角形索引、UV、法线与包围盒
- 中心化 Perlin Noise 地形高度
- 使用局部 `System.Random` 将 Seed 映射到噪声偏移
- MeshCollider 刷新与 `TerrainGenerated` 事件
- 独立的确定性环境随机流
- 地形 Raycast、表面法线坡度过滤、间距、边界与中心安全规则
- 基于 Seed 的树木/岩石 Prefab 选择、Yaw 与等比缩放
- 3 种树木、3 种岩石以及 URP 材质与碰撞体

## 开发时间线

| Commit | 步骤 | 拆分原因 |
|---|---|---|
| 5 | 平面规则网格 | 在高度变形前先建立并理解拓扑 |
| 6 | Perlin Noise 地形 | 在保持拓扑和 UV 的前提下改变高度 |
| 7 | World Seed | 理解生成行为后再加入可复现机制 |
| 8 | 环境生成 | 将环境放置设计为程序化地形的消费者 |
| 9 | Low-poly 资产集成 | 提升视觉表现，同时避免美术工作混入算法职责 |

## 系统架构

`MeshGenerator` 是不依赖场景的数据生成器；`TerrainGenerator` 管理 Unity 组件并发出地形事件；`EnvironmentSpawner` 监听地形就绪状态，只管理生成环境层级。`PlayerController` 和 `CameraController` 继续作为可玩场景中的独立模块。

## 程序化流程

```text
World Seed → Perlin 偏移 → 网格高度 → Mesh + MeshCollider
          ↘ 独立环境 Seed → 候选点 → Raycast
            → 坡度/间距过滤 → Prefab Variant → 环境
```

## 确定性

局部 `System.Random` 将程序化状态与 `UnityEngine.Random` 隔离，环境 Seed 从 World Seed 独立派生。使用 `1001`、`2002`、`1001` 进行 A/B/A 运行验证，A/B 的 Signature 不同，重复 A 的 Signature 完全一致。

## 环境与第三方资源集成

树木和岩石候选点会投射到准确的地形 Collider，并通过表面法线与距离过滤。最终展示使用 Quaternius Stylized Nature MegaKit 的最小 CC0 1.0 子集。模型经过 Prefab 包装、比例调整、Pivot 修正、URP/Lit 材质重建与简化碰撞配置；项目自制占位资产仍被保留。

## 运行验证

- Seed `1001`：18 棵树、12 块岩石，Layout Signature `-1270850978`
- Seed `2002`：18 棵树、12 块岩石，Layout Signature `-96201934`
- 再次使用 Seed `1001`：Signature `-1270850978`
- Commit 9 验证期间 Unity 资源导入与脚本刷新无编译错误

## 已知限制

- 当前为单个小型地形，没有 Chunk、流式加载、无限地形、Biome 或 LOD
- 材质处理较基础，没有高级地形纹理混合或自定义 Shader 系统
- 环境类别目前只有树木与岩石
- 尚无水体、天气、AI NPC、LLM 交互或运行时 AIGC
- 当前控制与展示仍是技术原型，不构成最终游戏循环

## 技术总结

- 将 Mesh 拓扑与顶点变形分离后更容易理解和维护。
- 显式 Seed 与独立随机流使程序化问题可以稳定复现。
- Raycast 为生成地形与环境放置建立了简单清晰的接口。
- 表面法线与间距规则无需大型世界框架即可提升合理性。
- 美术资产质量会显著影响技术原型的表达效果。

## 下一阶段

v0.3.0 将聚焦渲染与 Shader 开发，具体范围会单独规划；本次文档里程碑没有实现新的渲染功能。
