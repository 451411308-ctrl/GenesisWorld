# GenesisWorld Week 1 Milestone

**English** | [简体中文](./Week1_Milestone.zh-CN.md)

## Overview

Week 1 established a maintainable Unity core framework and a playable movement/camera baseline. It was released as `v0.1.0`.

## Completed Work

### Project Architecture
- Unity project structure, URP foundation, GitHub standards, and documentation system
- Clear Player, Camera, Core, Procedural, AI, NPC, UI, and Shader module boundaries

### Player Controller
- WASD movement, Shift sprint, Space jump
- Ground detection and manual gravity through CharacterController
- Inspector parameters and Animator `MoveSpeed` interface

### Third-person Camera
- Target follow, mouse orbit, pitch clamp, smooth motion, scroll zoom, and cursor lock
- Transform-based target reference keeps Camera independent from Player internals

## Technical Structure

```text
PlayerController → Player Transform → CameraTarget → CameraController
```

The milestone created the stable playable foundation required by later procedural-world work. It did not implement NPC, AI, Shader, or AIGC systems.

## Next Plan

The next completed phase was procedural world generation, now documented in the [v0.2.0 milestone](./ProceduralWorld_Milestone.md).
