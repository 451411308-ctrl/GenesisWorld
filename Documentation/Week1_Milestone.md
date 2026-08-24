# GenesisWorld Week 1 Milestone

## Overview

第一周的目标是建立可长期维护的 Unity 项目基础框架，为后续虚拟环境生成、智能交互和生成式内容研究提供稳定起点。

当前里程碑版本为 `v0.1.0`，对应 **Core Framework Completed**。本阶段聚焦工程结构、基础玩家移动和第三人称观察方式，不包含程序化地图、NPC、AI、Shader 或 AIGC 功能。

## Completed Features

### 1. Project Architecture

已完成：

- Unity 2022 LTS + URP 工程初始化
- 按 Art、Audio、Prefabs、Scenes、Scripts、Shaders 和 Settings 划分的资源结构
- Player、Camera、NPC、AI、Procedural、UI 与 Core 脚本模块边界
- Unity 官方推荐 Git 忽略规则与 GitHub 仓库规范
- Architecture、Development Log、Roadmap 和 Project Configuration 文档体系

### 2. Player Controller

已实现：

- WASD 移动
- Shift 冲刺
- Space 跳跃
- CharacterController 地面检测
- 手动重力与落地处理
- 可供 Inspector 调整的移动、冲刺、跳跃和重力参数
- Animator `MoveSpeed` 参数接口预留
- 摄像机水平方向相对移动，并保留世界坐标回退行为

### 3. Third Person Camera

已实现：

- Camera Follow
- Mouse Orbit
- Pitch Clamp
- Smooth Follow
- Mouse Scroll Zoom
- Min/Max Distance 限制
- Escape 释放鼠标与左键重新锁定
- 独立的 `CameraTarget` Transform

CameraController 仅依赖目标 Transform，不直接依赖 PlayerController。代码已经完成编译与序列化引用检查；第三人称鼠标操作的完整 Play Mode 验收仍建议在 Unity Editor 中进行一次人工复核。

## Technical Structure

### Player

Player 模块负责角色输入、水平移动、冲刺、跳跃和重力。运行时位移通过 CharacterController 完成，不依赖 Rigidbody 驱动全部移动逻辑。

### Camera

Camera 模块负责第三人称观察、环绕旋转、视角限制、平滑跟随、缩放和鼠标状态管理。

### Architecture

```text
PlayerController
      ↓ 更新
Player Transform / CameraTarget
      ↓ 读取
CameraController
```

Player 与 Camera 通过 Transform 数据单向协作，避免两个控制器互相持有复杂引用。

## Development Reflection

本阶段主要完成 Unity 基础框架建设。通过先明确目录、模块边界、输入方案、Player Prefab 和测试场景，项目已经具备继续扩展的工程基础，可为后续以下方向提供支持：

- 程序化世界生成
- AI NPC 交互
- AIGC 内容生成

第一阶段也建立了按 Commit 更新 README、开发日志和里程碑文档的维护习惯，便于 GitHub 展示和长期迭代。

## Next Plan

下一阶段进入 **Procedural World Generation**，计划从可复现随机种子、基础地形或环境块生成以及生成参数配置开始。

