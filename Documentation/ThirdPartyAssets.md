# Third-Party Assets / 第三方资源

This document records every third-party asset redistributed with GenesisWorld. Asset licenses were checked before repository integration.

本文档记录随 GenesisWorld 仓库重新分发的全部第三方资源。所有许可证均在集成前完成核对。

## Stylized Nature MegaKit — Standard Edition

| Field / 字段 | Record / 记录 |
|---|---|
| Asset Name / 资源名称 | Stylized Nature MegaKit — Standard Edition |
| Author / 作者 | Quaternius (`@Quaternius`) |
| Official Source / 官方来源 | [Quaternius — Stylized Nature MegaKit](https://quaternius.com/packs/stylizednaturemegakit.html) |
| Official Download / 官方下载 | [Official itch.io page / 官方 itch.io 页面](https://quaternius.itch.io/stylized-nature-megakit) |
| License / 许可证 | Creative Commons Zero v1.0 Universal (CC0 1.0) / Public Domain Dedication |
| License Reference / 许可证参考 | [Creative Commons CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) |
| Attribution Required / 是否要求署名 | No / 否（项目仍主动保留作者与来源信息） |
| Modification Allowed / 是否允许修改 | Yes / 是 |
| Commercial and Portfolio Use / 商业及作品集使用 | Yes / 是 |
| Public GitHub Redistribution / 公开 GitHub 再分发 | Yes / 是；CC0 允许复制、修改和再分发 |

### Files Used / 使用文件

Models:

- `CommonTree_2.fbx`
- `CommonTree_4.fbx`
- `Pine_3.fbx`
- `Rock_Medium_1.fbx`
- `Rock_Medium_2.fbx`
- `Rock_Medium_3.fbx`

Textures:

- `Bark_NormalTree.png`
- `Leaves_NormalTree.png`
- `Leaf_Pine.png`
- `Rocks_Diffuse.png`

License copy:

- `Assets/ThirdParty/Quaternius/StylizedNatureMegaKit/LICENSE.txt`

### Modifications / 修改内容

- Imported only the six models and four textures required by the current Tree/Rock spawn categories.
- Rebuilt all runtime materials with `Universal Render Pipeline/Lit`.
- Applied green tint and alpha clipping to foliage mask textures.
- Limited imported texture size to 1024 through Unity import settings.
- Wrapped imported models in clean GenesisWorld Prefab roots.
- Normalized model scale for the current 20×20 procedural terrain.
- Corrected ground pivots by offsetting the imported model child.
- Added simplified capsule and box colliders.
- Preserved the original FBX geometry and source texture files.

- 仅导入当前 Tree/Rock 两类生成器所需的 6 个模型和 4 张贴图。
- 使用 `Universal Render Pipeline/Lit` 重建运行时材质。
- 为叶片遮罩贴图增加绿色 Tint 与 Alpha Clipping。
- 通过 Unity 导入设置将纹理最大尺寸限制为 1024。
- 使用干净的 GenesisWorld Prefab Root 包装导入模型。
- 根据当前 20×20 程序化地形统一模型比例。
- 通过调整模型子节点修正地面 Pivot。
- 添加简化 CapsuleCollider 与 BoxCollider。
- 原始 FBX 几何和源贴图文件保持不变。

### Redistribution Notes / 再分发说明

The repository contains a minimal extracted subset rather than the complete 99 MB Standard pack. The included `LICENSE.txt` is copied from the official download archive. No paid Pro or Source edition files are included.

仓库仅包含从 99 MB Standard 免费包中筛选出的必要子集，不包含完整资源包。随仓库保存的 `LICENSE.txt` 来自官方下载压缩包，不包含付费 Pro 或 Source 版本文件。
