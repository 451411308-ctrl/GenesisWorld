# GenesisWorld

GenesisWorld is a Unity-based interactive virtual environment integrating generative AI, procedural generation, and real-time graphics.

## Overview

GenesisWorld is an open-source Unity demo for undergraduate digital media technology education and portfolio presentation. It explores how a low-poly 3D environment can combine procedural content generation, intelligent NPC interaction, real-time rendering, and AIGC-assisted asset workflows in a maintainable architecture.

The current milestone adds a modular CharacterController-based player movement foundation. More gameplay and AI features will be introduced incrementally in later milestones.

## Features

Current:

- Unity project initialization
- WASD player movement
- Sprint, jump, ground detection, and gravity
- Reusable Player prefab and controller test scene

Future:

- Procedural world generation
- AI NPC interaction
- Shader rendering
- AIGC asset generation

## Technology Stack

- Unity 2022 LTS
- C#
- Universal Render Pipeline (URP)
- Shader Graph
- Git
- AI API (planned)

## Development Progress

Current Version: **v0.1.0**

Completed:

- ✓ Project structure initialization
- ✓ Basic player controller system

For the architecture, roadmap, and configuration decisions, see the [`Documentation`](Documentation/) directory.

## Getting Started

1. Install Unity Hub and Unity **2022.3.62f3 LTS**.
2. Add this repository as a project in Unity Hub.
3. Open the project and allow Package Manager to restore dependencies.
4. Open `Assets/Scenes/Test_Player_Controller.unity` and enter Play Mode.
5. Use WASD to move, Shift to sprint, and Space to jump.

> NPCs, AI integration, procedural generation, custom shader effects, and a third-person camera are intentionally not implemented yet.
