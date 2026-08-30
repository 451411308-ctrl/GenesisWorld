# GenesisWorld Development Log / GenesisWorld 开发日志

## Week 1 / 第一周

### Commit 1

`Initialize GenesisWorld project structure`

**English**

Initialized the Unity 2022 LTS project, modular asset structure, URP and quality settings, New Input System dependency, GitHub ignore rules, and initial documentation.

**中文**

初始化 GenesisWorld 的 Unity 2022 LTS 工程、模块化资源结构、URP 与画质配置、New Input System 依赖、GitHub 忽略规则和基础文档。

### Commit 2

`Add player controller system`

**English**

Implemented a CharacterController-based player system with WASD movement, Shift sprinting, Space jumping, ground detection, manual gravity, configurable Inspector parameters, and an Animator interface placeholder. Added the Player Prefab and controller test scene.

**中文**

完成基于 CharacterController 的玩家控制系统，包括 WASD 移动、Shift 冲刺、Space 跳跃、地面检测、手动重力、Inspector 可配置参数和 Animator 接口预留；同时创建 Player Prefab 与控制器测试场景。

### Commit 3

`Add third person camera system`

**English**

Implemented third-person target following, mouse orbit controls, pitch constraints, smooth movement, zoom, cursor lock handling, and camera-relative player movement. `CameraController` reads `CameraTarget` through a Transform reference to keep Player and Camera loosely coupled.

**中文**

完成第三人称目标跟随、鼠标环绕、俯仰角限制、平滑移动、缩放、光标锁定管理和摄像机相对玩家移动。`CameraController` 通过 Transform 引用读取 `CameraTarget`，保持 Player 与 Camera 低耦合。

The code and serialized references passed compilation and static validation; a final hands-on Play Mode camera review remains recommended.

代码和序列化引用已通过编译与静态验证；仍建议进行一次完整的 Play Mode 摄像机人工复核。

### Commit 4

`Update documentation and create Week 1 milestone`

**English**

Reorganized the GitHub project overview, added the Week 1 milestone report, and published the annotated `v0.1.0` tag for the completed core framework.

**中文**

整理 GitHub 项目主页，新增第一周里程碑报告，并为已完成的核心基础框架发布注解标签 `v0.1.0`。

## Week 2 / 第二周

### Commit 5

`Add procedural terrain generation foundation`

**English**

Implemented the procedural terrain foundation with a responsibility-separated grid data generator and Unity terrain component. The system generates flat vertices, triangle indices, normalized UV coordinates, recalculated normals and bounds, and supports `MeshFilter`, `MeshRenderer`, and `MeshCollider` integration.

**中文**

完成程序化地形基础模块，将规则网格数据计算与 Unity 地形组件职责分离。系统可生成平坦顶点、三角形索引、归一化 UV，并重新计算法线与包围盒，同时支持 `MeshFilter`、`MeshRenderer` 和 `MeshCollider` 集成。

### Commit 6

`Implement noise-based terrain generation`

**English**

Added centered Perlin-noise-based vertex height generation to the existing grid mesh. Added configurable noise scale, height scale, and sampling offset while preserving triangle topology and UVs. Normals, bounds, and the mesh collider continue to update after generation.

**中文**

在现有规则网格基础上加入中心化 Perlin Noise 高度采样，并提供噪声尺度、高度尺度和采样偏移参数。三角形拓扑与 UV 保持不变，生成后继续更新法线、包围盒和网格碰撞体。

### Commit 7

`Add seeded procedural world generation`

**English**

Added deterministic seed-based terrain generation by mapping each seed through a local `System.Random` instance to a stable Perlin Noise sampling offset. Manual noise offset remains available for controlled debugging, and the module does not modify Unity's global random state.

**中文**

新增基于 Seed 的确定性地形生成机制，通过局部 `System.Random` 将每个 Seed 稳定映射到 Perlin Noise 采样偏移，使相同 Seed 能够复现相同地形。系统保留手动噪声偏移用于可控调试，且不会修改 Unity 的全局随机状态。
