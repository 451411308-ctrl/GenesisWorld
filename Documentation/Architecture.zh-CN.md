# GenesisWorld 系统架构

[English](./Architecture.md) | **简体中文**

## 设计目标

GenesisWorld 通过显式引用和单一职责，让游戏逻辑、程序化算法、渲染、未来 AI 服务与内容生产流程保持低耦合。

## 已实现运行时结构

```text
Unity Engine + URP
├── PlayerController ── 更新玩家运动
├── CameraController ── 观察 CameraTarget
├── NPC 交互 [v0.4 进行中]
│   ├── NPCProfile ── 稳定的编辑身份与本地 Greeting 数据
│   ├── NPCActor ── 场景实体与 IInteractable 实现
│   ├── PlayerInteractionController ── 摄像机目标检测与输入路由
│   └── DialogueController ── 对话 UI 与玩家输入状态
├── 程序化世界 [v0.2 已完成 / CPU]
│   ├── MeshGenerator ── 规则网格数据与噪声高度
│   ├── TerrainGenerator ── Mesh 生命周期与地形事件
│   └── EnvironmentSpawner ── 确定性的表面放置
└── 渲染层 [v0.3 基础已完成 / GPU]
    ├── StylizedTerrain ── 高度/坡度颜色、光照、阴影与雾
    ├── StylizedEnvironment ── 带贴图的明暗分层与透明裁剪阴影
    ├── StylizedSkybox ── 基于观察方向的渐变与大气地平线
    └── RenderSettings ── 天空盒绑定与 Linear Fog（无运行时 Manager）
```

地形生成与环境放置相互分离：地形负责几何与碰撞，Spawner 等待 `TerrainGenerated` 并只管理自己的生成层级。局部 `System.Random` 保证结果可复现，同时不改变 Unity 全局随机状态。

渲染边界遵循相同原则：CPU 模块回答几何在哪里，自定义 GPU Shader 回答可见表面如何呈现。Directional Light、阴影设置、Skybox 与 Fog 将这些表面组合成最终场景，但不改变程序化状态。

v0.4 交互边界已经将可编辑的 `NPCProfile` 数据与 `NPCActor` 场景实体分开。`PlayerInteractionController` 负责摄像机射线和交互输入，`DialogueController` 负责显示与临时移动锁定。当前回复来自 Profile 本地 Greeting，尚不存在 Provider 或网络层。

## 未来层级

- v0.3 风格化渲染基础已经完成。水体、风动、Additional Lights、后处理或 LOD 等图形学研究仍是可选独立工作，不是现有功能，也不代表项目拥有完整渲染引擎。
- AI Interaction 下一步会加入与服务商无关的对话服务和适配接口；上下文、记忆、决策与日程仍属于未来工作。
- AIGC Content 将作为编辑器/离线生产流程，其输出必须经过审核和优化。

这些是规划中的职责边界，不代表已实现的运行时功能。

## 依赖原则

1. 依赖 Inspector 引用、事件或抽象，不依赖隐藏的全局状态。
2. 程序化 Seed 必须显式，算法应可独立测试。
3. 运行时代码与编辑器、资产生产工具分离。
4. 只有真正跨模块的公共职责才进入 Core。
5. 仅在必须按路径加载时使用 `Resources`，常规资源优先使用序列化引用和 Prefab。
