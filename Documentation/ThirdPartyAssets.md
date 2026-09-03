# Third-Party Assets

**English** | [简体中文](./ThirdPartyAssets.zh-CN.md)

This document records third-party content redistributed with GenesisWorld. Asset terms were checked before integration.

## Stylized Nature MegaKit — Standard Edition

| Field | Record |
|---|---|
| Asset | Stylized Nature MegaKit — Standard Edition |
| Author | Quaternius (`@Quaternius`) |
| Source | [Official pack page](https://quaternius.com/packs/stylizednaturemegakit.html) |
| Download | [Official itch.io page](https://quaternius.itch.io/stylized-nature-megakit) |
| License | [CC0 1.0 Universal](https://creativecommons.org/publicdomain/zero/1.0/) / Public Domain Dedication |
| Attribution | Not required; GenesisWorld retains author and source records |
| Modification, commercial use, redistribution | Permitted |

### Files Used

- Models: `CommonTree_2.fbx`, `CommonTree_4.fbx`, `Pine_3.fbx`, `Rock_Medium_1.fbx`, `Rock_Medium_2.fbx`, `Rock_Medium_3.fbx`
- Textures: `Bark_NormalTree.png`, `Leaves_NormalTree.png`, `Leaf_Pine.png`, `Rocks_Diffuse.png`
- License copy: `Assets/ThirdParty/Quaternius/StylizedNatureMegaKit/LICENSE.txt`

### Project Modifications

- Extracted only the six models and four textures needed by current tree/rock categories.
- Rebuilt runtime materials with URP/Lit; added foliage tint and alpha clipping.
- Limited imported textures to 1024 through Unity import settings.
- Wrapped models in GenesisWorld prefab roots, normalized scale, corrected ground pivots, and added simple colliders.
- Preserved original FBX geometry and source textures.

### Redistribution

The repository contains a minimal subset rather than the complete 99 MB Standard pack. `LICENSE.txt` comes from the official archive. No paid Pro or Source edition files are included.

## TextMesh Pro Essential Resources

| Field | Record |
|---|---|
| Package | TextMesh Pro `3.0.7` (Unity package) |
| Purpose | Runtime font asset, settings, shaders, line-breaking data, and optional sprite resources required by the dialogue UI |
| Imported path | `Assets/TextMesh Pro/` |
| Scope | Essentials only; documentation and Examples & Extras were not committed |

The imported resources retain their bundled notices. `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt` records the SIL Open Font License for Liberation Sans, and `Assets/TextMesh Pro/Sprites/EmojiOne Attribution.txt` records the bundled EmojiOne attribution. GenesisWorld does not modify or claim ownership of these resources.

## Project-created NPC Placeholder

`Assets/Prefabs/NPC/GuideNPC.prefab` and its two materials are created for GenesisWorld from Unity primitives. They do not include a downloaded character model or a third-party animation.
