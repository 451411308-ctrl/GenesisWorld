# GenesisWorld Architecture / GenesisWorld 项目架构

## Design Goals / 设计目标

### English

GenesisWorld uses a layered, modular architecture. Systems should collaborate through explicit references and stable interfaces so that scene objects, rendering, procedural algorithms, AI services, and asset-production workflows do not become tightly coupled.

The current implementation includes the Core Framework, `PlayerController`, and `CameraController`. Procedural generation, AI interaction, and AIGC layers remain planned work.

### 中文

GenesisWorld 采用分层、模块化架构。各系统通过显式引用和稳定接口协作，避免场景对象、渲染逻辑、程序化算法、AI 服务与资产生产流程形成强耦合。

当前实现包括核心工程框架、`PlayerController` 和 `CameraController`；程序化生成、AI 交互与 AIGC 层仍属于后续规划。

## Architecture Overview / 总体架构

```text
AIGC Content Layer / AIGC 内容层                 [Planned / 计划]
AI Interaction Layer / AI 交互层                [Planned / 计划]
Procedural Generation Layer / 程序化生成层      [Planned / 计划]
Rendering Layer / 渲染层                        [URP foundation / URP 基础]
Gameplay Layer / 游戏逻辑层                     [Player + Camera]
Core Layer / 核心层                             [Unity services and project infrastructure]
Unity Engine
```

### Core Layer / 核心层

**English**

Provides Unity lifecycle integration, project configuration, input infrastructure, shared services, and future service registration. Shared code belongs here only when it has a clear cross-module responsibility.

**中文**

负责 Unity 生命周期接入、项目配置、输入基础设施、公共服务以及未来的服务注册。只有职责明确且确实跨模块共享的代码才应进入该层。

### Gameplay Layer / 游戏逻辑层

**English**

Contains the implemented Player and Camera modules and will later coordinate interaction and UI flows. `PlayerController` updates the player Transform; `CameraController` observes `CameraTarget` without directly depending on player-control internals.

**中文**

包含当前已实现的 Player 与 Camera 模块，未来还将协调交互和 UI 流程。`PlayerController` 更新玩家 Transform，`CameraController` 读取 `CameraTarget`，但不直接依赖玩家控制器内部逻辑。

### Rendering Layer / 渲染层

**English**

Uses Universal Render Pipeline (URP) as the rendering foundation. Shader Graph, custom Shader code, renderer features, and visual quality tiers will be developed in later milestones.

**中文**

以 Universal Render Pipeline (URP) 作为渲染基础。Shader Graph、自定义 Shader、Renderer Feature 与画质分级将在后续里程碑中实现。

### Procedural Generation Layer / 程序化生成层

**English**

Planned to manage reproducible random seeds, terrain or environment layout, generation rules, and configurable generation parameters. Algorithms should remain independently testable.

**中文**

计划负责可复现随机种子、地形或环境布局、生成规则和可配置生成参数。相关算法应保持可独立测试。

### AI Interaction Layer / AI 交互层

**English**

Planned to manage NPC context, behavior decisions, request scheduling, and external LLM API adapters. Domain logic should remain separate from a specific network provider.

**中文**

计划负责 NPC 上下文、行为决策、请求调度与外部 LLM API 适配。领域逻辑应与具体网络服务供应商分离。

### AIGC Content Layer / AIGC 内容层

**English**

Planned as an editor-side and offline production workflow for concepts, textures, model drafts, and Unity import standards. Generated assets must be reviewed and optimized before entering runtime asset directories.

**中文**

计划以编辑器工具和离线生产流程支持概念图、纹理、模型草案与 Unity 导入规范。生成资产需要经过审核和优化后才能进入运行时资源目录。

## Directory and Module Boundaries / 目录与模块边界

| Path | English Responsibility | 中文职责 |
|---|---|---|
| `Assets/Art` | Models, textures, materials, and animations | 模型、纹理、材质与动画资产 |
| `Assets/Audio` | Music and sound effects | 音乐与音效 |
| `Assets/Prefabs` | Reusable Unity Prefabs | 可复用 Unity Prefab |
| `Assets/Scenes` | Runtime and test scenes | 运行场景与测试场景 |
| `Assets/Scripts/Core` | Shared infrastructure | 公共基础设施 |
| `Assets/Scripts/Player` | Player input and movement | 玩家输入与移动 |
| `Assets/Scripts/Camera` | Third-person camera behavior | 第三人称摄像机行为 |
| `Assets/Scripts/NPC` | Planned NPC domain logic | 计划中的 NPC 领域逻辑 |
| `Assets/Scripts/AI` | Planned AI service abstractions | 计划中的 AI 服务抽象 |
| `Assets/Scripts/Procedural` | Planned generation algorithms | 计划中的生成算法 |
| `Assets/Scripts/UI` | User-interface logic | 用户界面逻辑 |
| `Assets/Shaders` | Shader Graph and custom Shader assets | Shader Graph 与自定义 Shader 资产 |
| `Assets/Resources` | Minimal path-loaded runtime assets | 少量按路径加载的运行时资产 |
| `Assets/Plugins` | Reviewed third-party plugins | 经过审核的第三方插件 |
| `Assets/Settings` | URP and project asset settings | URP 与项目资产配置 |

## Dependency Principles / 依赖原则

**English**

1. Gameplay modules depend on explicit references or abstractions, not concrete external AI providers.
2. Procedural generation should use explicit seeds so results can be reproduced.
3. Runtime code remains separate from editor and AIGC production tools.
4. Shared infrastructure belongs in Core only when its ownership is clear.
5. `Resources` is reserved for assets that must use `Resources.Load`; normal references should use the Inspector, Prefabs, or a future Addressables workflow.

**中文**

1. 游戏模块依赖显式引用或抽象接口，不直接绑定具体外部 AI 服务。
2. 程序化生成应使用显式随机种子，确保结果可复现。
3. 运行时代码与编辑器、AIGC 生产工具保持分离。
4. 只有归属明确的共享基础设施才放入 Core。
5. `Resources` 仅保存必须由 `Resources.Load` 访问的资产；常规引用应使用 Inspector、Prefab 或未来的 Addressables 工作流。
