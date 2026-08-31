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
