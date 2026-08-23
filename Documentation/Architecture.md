# GenesisWorld 架构说明

## 设计目标

GenesisWorld 采用分层、模块化架构。当前提交只建立目录与依赖边界，不包含任何玩法实现。后续模块应通过清晰接口协作，避免场景对象、AI 服务、程序化算法和资产生成流程互相强依赖。

## 总体架构

```text
Unity Engine Layer
        ↓
Gameplay Layer
        ↓
AI Interaction Layer
        ↓
Procedural Generation Layer
        ↓
AIGC Content Layer
```

### Unity Engine Layer

提供场景、GameObject、生命周期、物理、动画、音频、输入系统和 URP 渲染等基础能力。它是所有运行时模块的技术底座。

### Gameplay Layer

负责玩家、摄像机、交互、UI 和核心游戏流程。该层消费引擎能力，并通过稳定接口请求 AI 或程序化内容，不直接依赖具体外部 AI 服务。

### AI Interaction Layer

负责 NPC 对话上下文、行为决策、请求调度和外部 AI API 适配。未来应把领域逻辑与网络供应商实现分离，便于离线模拟、测试和更换服务。

### Procedural Generation Layer

负责地形、环境布局、规则系统、随机种子和可复现内容生成。其输出供 Gameplay Layer 使用，也可接受 AI 层给出的高层语义参数，但算法本身应保持可独立测试。

### AIGC Content Layer

负责 AIGC 辅助资产工作流，例如概念图、纹理、模型草案及其导入规范。该层以编辑器工具和离线生产流程为主，生成内容经过审核与优化后才进入运行时资产目录。

## 目录与模块边界

- `Assets/Art`：模型、纹理、材质与动画源资产。
- `Assets/Audio`：音乐和音效。
- `Assets/Prefabs`：可复用 Unity 预制体。
- `Assets/Scenes`：可运行场景。
- `Assets/Scripts/Core`：启动、服务注册、公共基础设施。
- `Assets/Scripts/Player`：未来的玩家功能。
- `Assets/Scripts/Camera`：未来的摄像机功能。
- `Assets/Scripts/NPC`：NPC 领域模型与表现逻辑。
- `Assets/Scripts/AI`：AI 服务抽象、请求与响应适配。
- `Assets/Scripts/Procedural`：程序化生成算法。
- `Assets/Scripts/UI`：界面与表现层逻辑。
- `Assets/Shaders`：Shader Graph 与自定义 Shader。
- `Assets/Resources`：仅用于确需运行时按路径加载的少量资产，避免滥用。
- `Assets/Plugins`：经过审核的第三方原生或托管插件。
- `Assets/Settings`：URP 与项目级 Unity 资产配置。

## 依赖原则

1. 玩法模块依赖抽象接口，不直接绑定具体 AI API。
2. 程序化生成使用显式随机种子，保证结果可复现。
3. 运行时代码与编辑器/AIGC 生产工具分离。
4. 跨模块共享能力优先放入 `Core`，但避免形成无边界的工具集合。
5. `Resources` 只保存必须由 `Resources.Load` 访问的资产；常规引用使用 Inspector、Prefab 或未来的 Addressables。

