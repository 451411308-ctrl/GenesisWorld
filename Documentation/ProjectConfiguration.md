# Unity 项目配置记录

本文档记录 Commit 1 的基础配置，后续变更应继续追加并说明原因。

## 编辑器版本

- Unity：`2022.3.62f3 LTS`
- 选择原因：LTS 版本维护周期稳定、包生态成熟，适合长期维护和教学展示。

## 渲染管线

- Universal Render Pipeline：`14.0.12`
- Graphics 默认管线：`Assets/Settings/URP-HighFidelity.asset`
- Quality 管线：
  - Performant → `URP-Performant.asset`
  - Balanced → `URP-Balanced.asset`
  - High Fidelity → `URP-HighFidelity.asset`
- Standalone 默认质量：High Fidelity
- 颜色空间：Linear

### 为什么选择 URP

URP 在桌面端、移动端和 WebGL 间提供统一的可编程渲染架构，相比内置管线更适合 Shader Graph 与现代渲染扩展，同时比 HDRP 更易控制硬件门槛和维护成本。

### 为什么适合 GenesisWorld

GenesisWorld 以 Low-poly 环境、实时 Shader、程序化生成和教学演示为核心。URP 能兼顾风格化画面、跨平台发布和运行性能，并为后续 Shader Graph、Renderer Feature 与质量分级留下稳定扩展点。

## Player 与项目标识

- Company Name：`GenesisWorld`
- Product Name：`GenesisWorld`
- Active Input Handling：Input System Package (New)
- 默认分辨率：1024 × 768（模板默认值，正式演示阶段再按目标平台调整）

## Package Manager

- Universal RP：`14.0.12`
- Input System：`1.7.0`
- Shader Graph：由 URP 依赖自动提供
- Test Framework：`1.1.33`
- TextMesh Pro：`3.0.7`

包版本固定在 `Packages/manifest.json`，解析后的完整依赖由 `Packages/packages-lock.json` 记录。首次打开工程时由 Unity Package Manager 校验并恢复依赖。

## Quality Settings

保留 URP 模板提供的三档质量配置：Performant、Balanced 和 High Fidelity。每档绑定独立 URP Asset，未来可分别调整阴影、抗锯齿、渲染比例和后处理开销，无需修改玩法代码。

## 本次修改清单

1. 将项目标识从模板名称更新为 GenesisWorld。
2. 保持 Linear Color Space。
3. 确认 Graphics Settings 已绑定 High Fidelity URP Asset。
4. 确认三个质量等级均绑定对应 URP Asset。
5. 添加 Input System 包并切换到新输入系统。
6. 添加模块化资产目录、项目文档与 Unity `.gitignore`。

