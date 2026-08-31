# GenesisWorld 系统架构

[English](./Architecture.md) | **简体中文**

## 设计目标

GenesisWorld 通过显式引用和单一职责，让游戏逻辑、程序化算法、渲染、未来 AI 服务与内容生产流程保持低耦合。

## 已实现运行时结构

```text
Unity Engine + URP
├── PlayerController ── 更新玩家运动
├── CameraController ── 观察 CameraTarget
└── 程序化世界
    ├── MeshGenerator ── 规则网格数据与噪声高度
    ├── TerrainGenerator ── Mesh 生命周期与地形事件
    └── EnvironmentSpawner ── 确定性的表面放置
```

地形生成与环境放置相互分离：地形负责几何与碰撞，Spawner 等待 `TerrainGenerated` 并只管理自己的生成层级。局部 `System.Random` 保证结果可复现，同时不改变 Unity 全局随机状态。

## 未来层级

- 渲染与 Shader 开发将在现有 URP 基础上扩展。
- AI Interaction 未来会隔离 NPC 上下文、决策、调度与服务商适配。
- AIGC Content 将作为编辑器/离线生产流程，其输出必须经过审核和优化。

这些是规划中的职责边界，不代表已实现的运行时功能。

## 依赖原则

1. 依赖 Inspector 引用、事件或抽象，不依赖隐藏的全局状态。
2. 程序化 Seed 必须显式，算法应可独立测试。
3. 运行时代码与编辑器、资产生产工具分离。
4. 只有真正跨模块的公共职责才进入 Core。
5. 仅在必须按路径加载时使用 `Resources`，常规资源优先使用序列化引用和 Prefab。
