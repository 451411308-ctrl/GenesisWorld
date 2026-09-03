# 第三方资源

[English](./ThirdPartyAssets.md) | **简体中文**

本文档记录随 GenesisWorld 仓库重新分发的第三方内容。所有许可条款均在集成前完成核对。

## Stylized Nature MegaKit — Standard Edition

| 字段 | 记录 |
|---|---|
| 资源名称 | Stylized Nature MegaKit — Standard Edition |
| 作者 | Quaternius（`@Quaternius`） |
| 来源 | [官方资源页](https://quaternius.com/packs/stylizednaturemegakit.html) |
| 下载 | [官方 itch.io 页面](https://quaternius.itch.io/stylized-nature-megakit) |
| 许可证 | [CC0 1.0 Universal](https://creativecommons.org/publicdomain/zero/1.0/) / 公共领域贡献声明 |
| 署名 | 不强制；GenesisWorld 仍主动保留作者与来源记录 |
| 修改、商业使用、再分发 | 允许 |

### 使用文件

- 模型：`CommonTree_2.fbx`、`CommonTree_4.fbx`、`Pine_3.fbx`、`Rock_Medium_1.fbx`、`Rock_Medium_2.fbx`、`Rock_Medium_3.fbx`
- 贴图：`Bark_NormalTree.png`、`Leaves_NormalTree.png`、`Leaf_Pine.png`、`Rocks_Diffuse.png`
- 许可证副本：`Assets/ThirdParty/Quaternius/StylizedNatureMegaKit/LICENSE.txt`

### 项目修改

- 仅提取当前树木与岩石类别所需的 6 个模型和 4 张贴图。
- 使用 URP/Lit 重建运行时材质，并为叶片配置绿色 Tint 与 Alpha Clipping。
- 通过 Unity 导入设置将纹理最大尺寸限制为 1024。
- 使用 GenesisWorld Prefab Root 包装模型，统一比例、修正底部 Pivot，并添加简化 Collider。
- 保留原始 FBX 几何与源贴图。

### 再分发说明

仓库仅包含必要子集，不包含完整的 99 MB Standard 资源包。`LICENSE.txt` 来自官方下载压缩包；仓库未包含任何付费 Pro 或 Source 版本文件。

## TextMesh Pro Essential Resources

| 字段 | 记录 |
|---|---|
| Package | TextMesh Pro `3.0.7`（Unity Package） |
| 用途 | 对话 UI 所需的运行时字体资源、设置、Shader、断行数据与可选 Sprite 资源 |
| 导入路径 | `Assets/TextMesh Pro/` |
| 范围 | 仅保留 Essentials；Documentation 与 Examples & Extras 未提交 |

导入资源保留随附声明。`Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt` 记录 Liberation Sans 的 SIL Open Font License，`Assets/TextMesh Pro/Sprites/EmojiOne Attribution.txt` 记录随附 EmojiOne 署名信息。GenesisWorld 不修改这些资源，也不主张其所有权。

## 项目自制 NPC 占位模型

`Assets/Prefabs/NPC/GuideNPC.prefab` 及其两个材质由 Unity Primitive 在 GenesisWorld 内创建，不包含下载的角色模型或第三方动画。
