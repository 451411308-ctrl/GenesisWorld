# GenesisWorld

[English](./README.md) | **简体中文**

> 基于 Unity 的交互式虚拟环境，将确定性程序化生成与自定义 URP 风格化渲染基础结合。

![Unity 2022.3 LTS](https://img.shields.io/badge/Unity-2022.3%20LTS-000000?logo=unity) ![C#](https://img.shields.io/badge/Language-C%23-512BD4?logo=csharp) ![URP](https://img.shields.io/badge/Rendering-URP-0C84FF) ![Milestone](https://img.shields.io/badge/milestone-v0.3.0-2ea44f)

GenesisWorld 探索程序化生成、实时图形与后续生成式 AI 系统如何构成交互式虚拟环境。**当前里程碑：v0.3.0 — 渲染与 Shader。**项目已经形成风格化渲染基础，但不是完整游戏、生产级渲染引擎，也尚未实现 AI 产品功能。

## 项目展示

![拥有协调天空、雾、光照与阴影的 GenesisWorld 风格化程序化世界](Documentation/Images/GenesisWorld_Atmosphere_Ground_01.png)

地面附近真实 Unity Game View，Seed 为 `12345`，使用自定义地形/环境/天空 Shader、Linear Fog 与硬方向光阴影。

### 渲染开发过程

| 风格化地形 — Commit 11 | 风格化环境 — Commit 12 |
|---|---|
| ![高度与坡度驱动的风格化地形](Documentation/Images/GenesisWorld_StylizedTerrain_01.png) | ![分层环境光照与硬阴影](Documentation/Images/GenesisWorld_StylizedEnvironment_01.png) |

Commit 13 通过天空、雾、光照与完整场景呈现统一表面和环境阶段。更早截图继续保留在 `Documentation/Images/`，真实记录项目演进。

## 项目概述

GenesisWorld 是面向数字媒体技术学习、作品集展示与研究探索的 Unity 开源项目，强调模块职责清晰、生成结果可复现、资产来源可追溯，以及按里程碑持续迭代。

## 当前功能

- CharacterController 移动、冲刺、跳跃、重力与地面检测
- 支持鼠标环绕、俯仰限制、平滑跟随和滚轮缩放的第三人称摄像机
- 程序化网格顶点、三角形、UV、法线与包围盒
- 参数化 Perlin Noise 地形高度
- 不污染 Unity 全局随机状态的确定性 World Seed
- Terrain Mesh 生命周期、MeshCollider 更新与生成事件
- 基于 Raycast、坡度限制和最小间距的树木与岩石生成
- 确定性的 Prefab 选择、旋转与缩放
- 使用 URP 材质和简化碰撞体的 Low-poly 环境资产
- 支持接收阴影与 Fog 的自定义 URP 风格化地形 Shader
- 支持光照量化与包裹式漫反射的自定义风格化环境 Shader
- 保留 `BaseMap` / `BaseColor`、透明裁剪与支持 Alpha 的植被阴影
- 支持天顶、地平线、下半球颜色与过渡控制的自定义渐变天空盒
- 与地平线颜色匹配的线性大气雾
- 统一的方向光、硬阴影与环境光呈现

`相同种子 + 相同参数 + 相同资产 = 相同程序化世界`

## 程序化世界生成流程

```mermaid
flowchart TD
    A[世界种子] --> B[地形生成器]
    B --> C[种子噪声偏移]
    C --> D[Perlin Noise 采样]
    D --> E[网格顶点高度]
    E --> F[程序化 Mesh]
    F --> G[MeshCollider]
    G --> H[地形生成完成事件]
    A --> I[独立环境种子]
    H --> J[环境生成器]
    I --> J
    J --> K[候选位置]
    K --> L[地形表面射线检测]
    L --> M[坡度与间距过滤]
    M --> N[树木与岩石 Prefab 选择]
    N --> O[确定性环境]
```

实现细节见[程序化地形](Documentation/ProceduralTerrain.zh-CN.md)与[程序化环境](Documentation/ProceduralEnvironment.zh-CN.md)。

## 渲染流程

```mermaid
flowchart TD
    A[CPU：Mesh 与世界生成] --> B[地形 Mesh]
    A --> C[树木与岩石实例]
    B --> D[StylizedTerrain]
    C --> E[StylizedEnvironment]
    F[方向光] --> D
    F --> E
    D --> G[大气呈现]
    E --> G
    H[StylizedSkybox 与 Linear Fog] --> G
    G --> I[最终风格化场景]
```

CPU 系统负责几何与确定性放置，GPU Shader 负责表面外观、光照与大气。详见[渲染与 Shader](Documentation/RenderingAndShaders.zh-CN.md)。

## 系统架构

| 模块 | 职责 |
|---|---|
| `MeshGenerator` | 网格几何、三角形、UV 与 Perlin 高度采样 |
| `TerrainGenerator` | 参数、种子偏移、Mesh 生命周期、MeshCollider 与生成事件 |
| `EnvironmentSpawner` | 环境随机流、候选点、射线检测、过滤、Prefab Variant 与重新生成 |
| `PlayerController` | 输入、移动、冲刺、跳跃与重力 |
| `CameraController` | 第三人称跟随、环绕、俯仰限制、平滑与缩放 |
| `StylizedTerrain` | GPU 高度/坡度颜色混合与轻量方向光照 |
| `StylizedEnvironment` | 保留贴图的分层光照与支持透明裁剪的环境阴影 |
| `StylizedSkybox` | 基于观察方向的渐变天空与协调大气地平线 |

地形构建与环境放置相互分离，使两个系统拥有清晰的生命周期。局部 `System.Random` 保证结果可复现，同时不影响 `UnityEngine.Random`。详见[架构文档](Documentation/Architecture.zh-CN.md)。

## 操作方式

| 输入 | 功能 |
|---|---|
| WASD | 相对摄像机移动 |
| Shift | 冲刺 |
| Space | 跳跃 |
| 鼠标移动 | 环绕摄像机 |
| 鼠标滚轮 | 缩放 |
| Escape | 释放光标 |

## 技术栈

- Unity `2022.3.62f3` LTS、C#、Universal Render Pipeline
- `Mathf.PerlinNoise`、局部 `System.Random`
- Git 与 GitHub

## 项目结构

```text
GenesisWorld/
├── Assets/{Art,Prefabs,Scenes,Scripts,Settings,ThirdParty}/
├── Documentation/
├── Packages/
├── ProjectSettings/
├── README.md
└── README.zh-CN.md
```

## 快速开始

1. 安装 Unity Hub 与 Unity `2022.3.62f3` LTS。
2. 执行 `git clone https://github.com/451411308-ctrl/GenesisWorld.git`。
3. 在 Unity Hub 添加工程并等待 Package 恢复。
4. 打开 `Assets/Scenes/Test_Player_Controller.unity`。
5. 进入 Play Mode。

## 当前里程碑

**v0.3.0 — 渲染与 Shader 里程碑**

风格化渲染基础已经完成：自定义地形、环境与天空 Shader，方向光、硬阴影、Linear Fog 与统一大气呈现。这不代表渲染工作永远完成。详见 [v0.3.0 里程碑报告](Documentation/RenderingAndShaders_Milestone.zh-CN.md)。

## 开发路线

| 版本 | 阶段 | 状态 |
|---|---|---|
| v0.1.0 | 核心框架 | ✅ 已完成 |
| v0.2.0 | 程序化世界 | ✅ 已完成 |
| v0.3.0 | 渲染与 Shader | ✅ 已完成 |
| v0.4.0 | AI NPC 交互 | ⏳ 计划中 |
| v0.5.0 | AIGC 辅助内容流程 | ⏳ 计划中 |

Biome、Chunk、无限地形、水体、高级 Shader、AI NPC 与运行时 AIGC 均是未来规划，不是当前功能。

## 第三方资源

环境使用 Quaternius [Stylized Nature MegaKit](https://quaternius.com/packs/stylizednaturemegakit.html) 的精选子集，许可证为 CC0 1.0。详见[第三方资源](Documentation/ThirdPartyAssets.zh-CN.md)。

## 项目文档

- [系统架构](Documentation/Architecture.zh-CN.md) · [工程配置](Documentation/ProjectConfiguration.zh-CN.md)
- [开发日志](Documentation/DevelopmentLog.zh-CN.md) · [开发路线](Documentation/Roadmap.zh-CN.md)
- [第一周里程碑](Documentation/Week1_Milestone.zh-CN.md) · [程序化世界里程碑](Documentation/ProceduralWorld_Milestone.zh-CN.md)
- [渲染与 Shader 里程碑](Documentation/RenderingAndShaders_Milestone.zh-CN.md)
- [程序化地形](Documentation/ProceduralTerrain.zh-CN.md) · [程序化环境](Documentation/ProceduralEnvironment.zh-CN.md)
- [渲染与 Shader](Documentation/RenderingAndShaders.zh-CN.md)
- [第三方资源](Documentation/ThirdPartyAssets.zh-CN.md)

## 许可证与资产声明

项目目前尚未声明覆盖源代码的整体许可证。第三方资产遵循各自记录的许可条款；已集成的 Quaternius 子集采用 CC0 1.0。资产许可证不等同于项目源代码许可证。
