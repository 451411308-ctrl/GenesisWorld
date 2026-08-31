# GenesisWorld 第一周里程碑

[English](./Week1_Milestone.md) | **简体中文**

## 阶段概述

第一周建立可长期维护的 Unity 核心框架，以及可运行的玩家移动与摄像机基础，并以 `v0.1.0` 发布。

## 完成内容

### 工程架构
- Unity 工程结构、URP 基础、GitHub 规范与文档体系
- 明确 Player、Camera、Core、Procedural、AI、NPC、UI 与 Shader 模块边界

### 玩家控制
- WASD 移动、Shift 冲刺、Space 跳跃
- 基于 CharacterController 的地面检测与手动重力
- Inspector 参数与 Animator `MoveSpeed` 接口

### 第三人称摄像机
- 目标跟随、鼠标环绕、俯仰限制、平滑移动、滚轮缩放与光标锁定
- 通过 Transform 引用目标，使 Camera 不依赖 Player 内部实现

## 技术结构

```text
PlayerController → Player Transform → CameraTarget → CameraController
```

该里程碑为后续程序化世界开发提供稳定的可玩基础，但没有实现 NPC、AI、Shader 或 AIGC 系统。

## 后续计划

后续已完成程序化世界阶段，详见 [v0.2.0 里程碑](./ProceduralWorld_Milestone.zh-CN.md)。
