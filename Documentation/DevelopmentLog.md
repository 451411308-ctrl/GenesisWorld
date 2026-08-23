# GenesisWorld 开发日志

## Week 1

### Commit 1: Initialize GenesisWorld project structure

完成内容：

- Unity 2022 LTS 工程初始化
- 模块化文件结构创建
- URP、质量等级与 Input System 基础配置完成
- GitHub 忽略规则与项目基础文档创建

当前阶段不包含玩家控制、摄像机控制、NPC、AI 接口、自定义 Shader 或游戏玩法。

### Commit 2: Add player controller system

完成内容：

- 基于新 Input System 实现 WASD 玩家移动
- 实现 Shift 冲刺、Space 跳跃、地面检测与手动重力
- 使用 CharacterController 处理碰撞与位移
- 创建包含 CharacterController、PlayerController 和 Animator 预留组件的 Player Prefab
- 创建玩家移动测试场景并完成运行验证

当前 Player 模块保持独立，不包含第三人称摄像机、NPC、AI、战斗或其他玩法系统。

### Commit 3: Add third person camera system

Implemented:

- Third-person camera follow
- Mouse orbit controls
- Pitch angle constraints
- Smooth camera movement
- Camera zoom
- Cursor lock handling
- Camera-relative player movement

CameraController 仅依赖 CameraTarget Transform；PlayerController 通过可选的 movementReference 获取水平方向，保持两个模块低耦合。
