# Unity Project Configuration / Unity 项目配置记录

## Purpose / 文档目的

### English

This document records the baseline Unity configuration established during project initialization. Future configuration changes should be appended with their rationale so that the repository remains reproducible.

### 中文

本文档记录项目初始化阶段建立的 Unity 基础配置。后续配置变更应继续补充并说明原因，确保仓库具备可复现性。

## Editor Version / 编辑器版本

**English**

- Unity: `2022.3.62f3 LTS`
- Rationale: the LTS release provides a stable maintenance cycle and a mature package ecosystem suitable for long-term development and teaching demonstrations.

**中文**

- Unity：`2022.3.62f3 LTS`
- 选择原因：LTS 版本具备稳定的维护周期与成熟的包生态，适合长期开发和教学展示。

## Render Pipeline / 渲染管线

| Setting / 配置项 | Value / 配置值 |
|---|---|
| Universal Render Pipeline | `14.0.12` |
| Graphics default pipeline / Graphics 默认管线 | `Assets/Settings/URP-HighFidelity.asset` |
| Performant quality pipeline / Performant 画质管线 | `URP-Performant.asset` |
| Balanced quality pipeline / Balanced 画质管线 | `URP-Balanced.asset` |
| High Fidelity quality pipeline / High Fidelity 画质管线 | `URP-HighFidelity.asset` |
| Standalone default quality / Standalone 默认画质 | High Fidelity |
| Color Space / 颜色空间 | Linear |

### Why URP / 为什么选择 URP

**English**

URP provides one programmable rendering architecture across desktop, mobile, and WebGL. It supports Shader Graph and modern rendering extensions while keeping hardware requirements and maintenance costs lower than HDRP.

**中文**

URP 在桌面端、移动端和 WebGL 间提供统一的可编程渲染架构，支持 Shader Graph 与现代渲染扩展，同时比 HDRP 更容易控制硬件门槛和维护成本。

### Why URP Fits GenesisWorld / 为什么 URP 适合 GenesisWorld

**English**

GenesisWorld targets a low-poly virtual environment, real-time Shader work, procedural generation, and educational demonstrations. URP balances stylized visuals, cross-platform delivery, and runtime performance while leaving extension points for Shader Graph, Renderer Features, and quality tiers.

**中文**

GenesisWorld 面向 Low-poly 虚拟环境、实时 Shader、程序化生成和教学演示。URP 能兼顾风格化画面、跨平台发布与运行性能，并为 Shader Graph、Renderer Feature 和画质分级保留扩展空间。

## Player and Project Identity / Player 与项目标识

| Setting / 配置项 | Value / 配置值 |
|---|---|
| Company Name | `GenesisWorld` |
| Product Name | `GenesisWorld` |
| Active Input Handling / 输入处理 | Input System Package (New) |
| Default Resolution / 默认分辨率 | 1024 × 768 |

The default resolution is inherited from the template and will be reviewed when a target release platform is selected.

默认分辨率继承自模板，待明确正式发布平台后再进行调整。

## Package Manager / 包管理

| Package / 包 | Version / 版本 |
|---|---|
| Universal RP | `14.0.12` |
| Input System | `1.7.0` |
| Shader Graph | Provided through URP / 由 URP 依赖提供 |
| Test Framework | `1.1.33` |
| TextMesh Pro | `3.0.7` |

**English**

Direct package versions are pinned in `Packages/manifest.json`, and resolved dependencies are recorded in `Packages/packages-lock.json`. Unity Package Manager validates and restores them when the project is opened.

**中文**

直接依赖版本固定在 `Packages/manifest.json`，解析后的完整依赖记录在 `Packages/packages-lock.json`。打开工程时由 Unity Package Manager 校验并恢复依赖。

## Quality Settings / 画质设置

### English

The project retains the URP template's Performant, Balanced, and High Fidelity quality tiers. Each tier uses a separate URP Asset so shadows, anti-aliasing, render scale, and post-processing cost can be adjusted without changing gameplay code.

### 中文

项目保留 URP 模板提供的 Performant、Balanced 和 High Fidelity 三档画质配置。每档绑定独立 URP Asset，后续可以分别调整阴影、抗锯齿、渲染比例和后处理开销，无需修改玩法代码。

## Initialization Changes / 初始化修改清单

**English**

1. Updated the project identity to GenesisWorld.
2. Retained Linear Color Space.
3. Bound Graphics Settings to the High Fidelity URP Asset.
4. Bound all three quality tiers to their corresponding URP Assets.
5. Added Input System and selected the new input backend.
6. Added the modular asset structure, project documentation, and Unity `.gitignore`.

**中文**

1. 将项目标识更新为 GenesisWorld。
2. 保持 Linear Color Space。
3. 将 Graphics Settings 绑定到 High Fidelity URP Asset。
4. 将三档画质分别绑定到对应 URP Asset。
5. 添加 Input System 并切换到新输入后端。
6. 添加模块化资产结构、项目文档与 Unity `.gitignore`。
