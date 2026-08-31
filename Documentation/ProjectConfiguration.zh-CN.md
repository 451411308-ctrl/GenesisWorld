# Unity 工程配置

[English](./ProjectConfiguration.md) | **简体中文**

## 基础配置

| 范围 | 配置 |
|---|---|
| 编辑器 | Unity `2022.3.62f3` LTS |
| 渲染 | Universal Render Pipeline |
| 脚本 | C# |
| 输入 | Unity Input System Package；当前控制器使用工程已配置的输入方式 |
| 画质 | `Assets/Settings` 下的 URP Asset |
| 版本控制 | Visible Meta Files、Force Text 序列化、Unity `.gitignore` |

GenesisWorld 采用 URP，是因为项目面向风格化环境、Shader Graph 实验、多硬件兼容和可控的渲染复杂度。URP 能为后续图形学开发提供实用基础，同时避免只适合高端平台的管线成本。

## 初始化记录

- 创建模块化 `Assets` 与 `Documentation` 目录。
- 配置项目标识与 Unity 文本序列化。
- 在 `Packages/manifest.json` 中安装并记录依赖。
- 为 Graphics 与 Quality Settings 指定 URP Asset。
- Git 忽略 `Library`、`Temp`、`Obj`、`Build`、`Logs` 与 `UserSettings`。

运行时参数和场景程序化配置在对应技术文档中单独记录。
