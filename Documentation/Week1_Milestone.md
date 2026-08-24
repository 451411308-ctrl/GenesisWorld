# GenesisWorld Week 1 Milestone
# GenesisWorld 第一周开发里程碑

## Overview / 阶段概述

### English

The Week 1 objective was to establish a maintainable Unity foundation for later work on virtual environment generation, intelligent interaction, and generative content. Version `v0.1.0` represents the **Core Framework Completed** milestone.

This milestone covers project organization, player movement, and a third-person camera. Procedural worlds, NPCs, AI services, custom rendering effects, and AIGC workflows are not included yet.

### 中文

第一周的目标是建立可长期维护的 Unity 项目基础，为后续虚拟环境生成、智能交互和生成式内容研究提供稳定起点。版本 `v0.1.0` 对应 **核心基础框架完成** 里程碑。

本阶段包含工程组织、玩家移动与第三人称摄像机；程序化世界、NPC、AI 服务、自定义渲染效果和 AIGC 工作流尚未实现。

## Completed Features / 已完成功能

### English

- Unity 2022 LTS + URP project initialization
- Modular asset and script directory structure
- GitHub repository conventions and documentation system
- CharacterController-based player movement
- Third-person camera implementation and scene integration

### 中文

- Unity 2022 LTS + URP 工程初始化
- 模块化资产与脚本目录结构
- GitHub 仓库规范与项目文档体系
- 基于 CharacterController 的玩家移动
- 第三人称摄像机实现与测试场景集成

## Project Architecture / 项目架构

### English

The project separates assets and runtime responsibilities into Art, Audio, Prefabs, Scenes, Scripts, Shaders, and Settings. Runtime scripts are divided into Player, Camera, NPC, AI, Procedural, UI, and Core modules. Only Player and Camera contain gameplay implementations at this milestone.

### 中文

项目按照 Art、Audio、Prefabs、Scenes、Scripts、Shaders 和 Settings 划分资源职责；运行时脚本进一步分为 Player、Camera、NPC、AI、Procedural、UI 与 Core 模块。当前里程碑只有 Player 和 Camera 包含实际玩法实现。

## Player Controller / 玩家控制系统

### English

The `PlayerController` uses the New Input System and `CharacterController`. It implements WASD movement, Shift sprinting, Space jumping, ground detection, and manually calculated gravity. Movement parameters are configurable in the Inspector, and an Animator `MoveSpeed` interface is reserved for later animation work.

Camera-relative horizontal movement is supported through an optional `movementReference`; world-space movement remains the fallback when the reference is not assigned.

### 中文

`PlayerController` 使用 New Input System 与 `CharacterController`，实现 WASD 移动、Shift 冲刺、Space 跳跃、地面检测和手动重力计算。移动参数可在 Inspector 中调整，并预留 Animator `MoveSpeed` 接口供后续动画系统使用。

玩家可通过可选的 `movementReference` 实现摄像机相对水平移动；未设置引用时仍回退到世界坐标移动。

## Third-person Camera / 第三人称摄像机系统

### English

The `CameraController` implements target following, mouse orbit controls, pitch constraints, `Vector3.SmoothDamp` movement, scroll-wheel zoom, distance limits, and cursor lock handling. A dedicated `CameraTarget` Transform keeps the camera independent from the internal implementation of `PlayerController`.

The code and serialized references passed compilation and static checks. A final hands-on Play Mode review of mouse orbit, zoom, and cursor behavior remains recommended.

### 中文

`CameraController` 实现目标跟随、鼠标环绕、俯仰角限制、基于 `Vector3.SmoothDamp` 的平滑移动、滚轮缩放、距离限制和鼠标锁定管理。独立的 `CameraTarget` Transform 使摄像机无需依赖 `PlayerController` 的内部实现。

代码与序列化引用已经通过编译和静态检查；仍建议在 Unity Editor 中对鼠标环绕、缩放和光标行为进行一次最终 Play Mode 人工复核。

## Technical Summary / 技术总结

### English

```text
PlayerController
      ↓ updates
Player Transform / CameraTarget
      ↓ observed by
CameraController
```

Player and Camera collaborate through Transform data in one direction. This keeps both modules independently maintainable and leaves room for later animation, environment, and interaction systems.

### 中文

```text
PlayerController
      ↓ 更新
Player Transform / CameraTarget
      ↓ 由其读取
CameraController
```

Player 与 Camera 通过 Transform 数据单向协作，两个模块可独立维护，并为后续动画、环境和交互系统保留扩展空间。

## Development Reflection / 开发总结

### English

The first phase prioritized project boundaries and reusable foundations before content production. Establishing a consistent input solution, Player Prefab, camera target, test scene, documentation structure, and Git workflow reduces integration risk for later features.

### 中文

第一阶段优先建立模块边界和可复用基础，而不是提前生产大量内容。统一输入方案、Player Prefab、摄像机观察点、测试场景、文档结构和 Git 工作流，有助于降低后续功能集成风险。

## Next Plan / 下一阶段计划

### English

The next phase is **Procedural World Generation**, beginning with reproducible random seeds, configurable generation parameters, and basic terrain or environment-block generation.

### 中文

下一阶段为 **程序化世界生成**，计划从可复现随机种子、可配置生成参数以及基础地形或环境块生成开始。
