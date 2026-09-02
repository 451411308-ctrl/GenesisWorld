# 渲染与 Shader

[English](./RenderingAndShaders.md) | **简体中文**

## 渲染基础

GenesisWorld 使用 Unity `2022.3.62f3`、Universal Render Pipeline `14.0.12`、ShaderLab 与手写 HLSL。Commit 11 新增项目首个自定义 URP Shader：`GenesisWorld/StylizedTerrain`。实现刻意保持纯颜色与轻量结构，便于理解图形学概念和长期维护。

## CPU 几何与 GPU 着色

CPU 与 GPU 分别解决地形的不同问题：

- C#（`MeshGenerator`、`TerrainGenerator`）回答：**顶点在哪里？** 它构建拓扑、采样 Perlin Noise、设置高度并更新碰撞。
- GPU Shader 回答：**表面应该呈现什么颜色？** 它使用完成后的世界坐标、法线与光照计算可见颜色。

Fragment Shader 不会重新采样 Perlin Noise，也不会改变程序化生成结果。

## 渲染流程

```text
顶点位置 + 法线
        ↓ Vertex Stage
Object → World → Clip Position
        ↓ 插值数据
世界高度 + 世界法线 + 主光
        ↓ Fragment Stage
高度颜色 → 坡度混合 → 风格化 Lambert 光照 → Fog
        ↓
最终像素颜色
```

Vertex Shader 将每个顶点转换到裁剪空间，并传递世界空间坐标与正确转换后的法线。Fragment Shader 为光栅化后的 Fragment/Pixel 计算地形颜色与光照。

## 基于高度的地形颜色

世界空间高度首先归一化到 `0–1`：

```text
heightFactor = saturate((worldY - HeightMin) / max(HeightMax - HeightMin, 0.0001))
heightColor = lerp(LowColor, HighColor, heightFactor)
```

`lerp` 在 0 时选择低处颜色，在 1 时选择高处颜色，中间按比例混合。epsilon 防止除零。使用世界空间后，即使未来移动 Terrain GameObject，高度含义仍然明确。

## 坡度检测与表面法线

Surface Normal 是垂直指向表面外侧的单位向量。Mesh Normal 会先正确转换并归一化到世界空间，再与 World Up 比较：

```text
upAlignment = saturate(dot(normalWS, float3(0, 1, 0)))
slope = 1 - upAlignment
slopeFactor = smoothstep(SlopeStart, SlopeEnd, slope)
```

平地上 `N ≈ (0,1,0)`，因此 `dot(N, Up) ≈ 1`，坡度接近 0。表面越倾斜，点积越小，Slope Factor 越大。`smoothstep` 用于避免草地与岩石颜色之间出现生硬分界。

## 点积与基础光照

`dot(N, L)` 表示归一化表面法线和主光方向的接近程度。面向光源的表面点积更大、更亮；背向光源时接近零。

Shader 使用 Lambert Diffuse，并乘以 URP 主光颜色、距离衰减和阴影衰减。`_AmbientStrength` 提供最低亮度，避免背光面完全变黑。这是轻量风格化模型，而非完整 PBR BRDF。

## Shader 参数

| 分组 | 属性 | 默认值 | 作用 |
|---|---|---:|---|
| 高度 | Low Color | `(0.10, 0.24, 0.07)` | 低处深色草地 |
| 高度 | High Color | `(0.48, 0.62, 0.24)` | 高处浅色/干草色 |
| 高度 | Height Min / Max | `-2.5 / 2.5` | 世界高度归一化范围 |
| 坡度 | Slope Color | `(0.36, 0.32, 0.27)` | 陡坡岩土色 |
| 坡度 | Slope Start / End | `0.04 / 0.12` | 平滑坡度过渡 |
| 光照 | Ambient Strength | `0.32` | 最低亮度 |

高度范围对应当前 `heightScale = 5`。Seed `12345` 的运行时测量结果为世界高度 `-1.755～2.029`，最大顶点法线坡度 `28.348°`。

## URP 集成

- 使用 URP `Core.hlsl` 与 `Lighting.hlsl` 的 `UniversalForward` Pass
- 支持主方向光颜色、方向以及阴影衰减变体
- 自定义 `ShadowCaster` 与 `DepthOnly` Pass 保留透明裁剪轮廓
- 通过 `ComputeFogFactor` 和 `MixFog` 兼容 Fog
- 材质参数位于 `UnityPerMaterial` CBUFFER，兼容 SRP Batcher

## 风格化环境 Shader

Commit 12 新增 `GenesisWorld/StylizedEnvironment`，用于程序化生成的树木与岩石。Shader 将各资产的原始 Base Texture 与既有 Tint 相乘，在世界空间计算 `dot(N,L)`，加入少量包裹光后按 `_LightSteps` 量化。默认采用三档明暗，既增强 Low-poly 表面朝向，又不会抹去全部贴图细节。

Low-poly 相邻面也可能拥有不同法线，因此同一光照方向下的 `N·L` 不同，这正是几何切面可读的重要来源。Shader 不直接保留 `0.12、0.35、0.63、0.91` 这类连续 Lambert 结果，而是将它们映射到少量稳定亮度：

```text
wrapped = saturate((saturate(dot(normalWS, lightDirectionWS)) + LightWrap) / (1 + LightWrap))
banded = round(wrapped * (LightSteps - 1)) / (LightSteps - 1)
final = BaseMap × BaseColor × (AmbientStrength + MainLightColor × banded × attenuation)
```

HLSL 内会将 `LightSteps` 限制为至少 2，避免除零。

树皮、阔叶、针叶与岩石分别使用四个项目自有适配材质。材质直接引用既有 CC0 贴图，不复制或修改第三方源资产。叶片继续启用 `_ALPHATEST_ON` 与 `0.5` Cutoff，因此透明区域会在 Forward、Depth 和 ShadowCaster Pass 中一致剔除。

| 参数 | 默认值 | 作用 |
|---|---:|---|
| Base Map | 资产贴图 / White Fallback | 保留美术细节，并兼容无贴图材质 |
| Base Color | 既有材质 Tint | 与采样贴图相乘 |
| Light Steps | `3` | 离散漫反射亮度档数 |
| Ambient Strength | `0.32`（叶片 `0.35`） | 避免背光面全黑 |
| Light Wrap | `0.20`（叶片 `0.25`） | 提升树冠内部可读性 |
| Alpha Cutoff | 叶片 `0.50` | 保留植被透明裁剪轮廓 |

环境与地形 Shader 的表面职责不同，但共用同一个主 Directional Light：

- `StylizedTerrain` 面向没有美术贴图集的生成地面，依据高度和坡度决定颜色。
- `StylizedEnvironment` 保留树木和岩石的原贴图与 Tint，再对其法线加入可调分层光照。

## 阴影记录

Commit 11 在启用 Soft Shadow 时发现锯齿和拉长的植被轮廓，因此当时场景保持无阴影。Commit 12 追踪了运行时 Prefab 与材质 Alpha 设置，新增支持透明裁剪的自定义 ShadowCaster Pass，并在每次切换后等待 URP 重建阴影资源，再比较 None、Hard 与 Soft 三种模式。

同机位运行对比覆盖 2、3、4 档明暗，以及 Hard/Soft Shadow。三档明暗在层次与可读性之间最平衡；测试场景最终选择 Hard Shadow，因为轮廓更符合 Low-poly/Cell 风格。Light Bias 保持 `0.05`、Normal Bias `0.4`、Near Plane `0.2`；High Fidelity 质量档现使用 `2048` 主光阴影贴图、`40` 单位阴影距离与两级 Cascade，替换模板对当前 `20×20` 小场景过高的 `4096` / `150` / 四级配置。Commit 13 开始实际使用已有 Fog 兼容能力，具体如下。

## 大气渲染

Commit 13 将地形与环境 Shader 放进明确设计的大气环境，而不是继续增加表面效果。最终方向为清爽风格化日间：冷蓝天顶、浅蓝绿色地平线、克制的线性距离雾、微暖日光，以及既有硬阴影语言。本次没有加入 Volume、HDRI、物理散射、云或 Ray Marching。

大气流程保持轻量：

```text
相机观察方向 → StylizedSkybox 渐变
物体到相机距离 → Unity Linear Fog Factor
Terrain / Environment / Player 颜色 → MixFog 混合到地平线色
暖 Directional Light + Hard Shadow → 局部形体与接触关系
```

## 风格化天空

`GenesisWorld/StylizedSkybox` 是手写 URP Shader，仅包含四个材质参数：

| 参数 | 最终值 | 作用 |
|---|---:|---|
| Zenith Color | `(0.18, 0.42, 0.72)` | 冷色上层天空 |
| Horizon Color | `(0.72, 0.84, 0.82)` | 浅色大气过渡与雾目标色 |
| Lower Color | `(0.32, 0.38, 0.28)` | 地形边缘可见时的自然下半球颜色 |
| Horizon Exponent | `0.65` | 控制地平线过渡宽度与柔和程度 |

## 观察方向

Skybox 不需要知道每个天空 Pixel 的世界坐标，主要需要相机正在观察的方向。Vertex Stage 将天空盒立方体方向转换到世界空间；Fragment Stage 将其归一化并读取 `viewDirection.y`。

- 正 Y 指向 Zenith。
- 接近 0 指向 Horizon。
- 负 Y 指向下半球。

## 地平线渐变

Shader 使用 `smoothstep` 再接 `pow`，而不是机械线性渐变：

```text
upper = pow(smoothstep(0, 1, saturate(viewDirection.y)), HorizonExponent)
lower = pow(smoothstep(0, 1, saturate(-viewDirection.y)), HorizonExponent)
upperSky = lerp(HorizonColor, ZenithColor, upper)
lowerSky = lerp(HorizonColor, LowerColor, lower)
```

上下渐变在同一个 Horizon Color 汇合，因此不会形成接缝。方向与渐变因子使用完整 `float` 精度；运行检查未发现粉色天空、黑色天空、接缝或明显色带。

## 雾

场景现使用 Unity Linear Fog，Start 为 `12`，End 为 `40`。Fog 不是放在世界中的一层灰色透明平面；兼容 Shader 会依据相机距离计算 Fog Factor，再将表面结果混合到 Fog Color。

```text
FinalColor = lerp(ObjectColor, FogColor, FogFactor)
```

运行对比覆盖 Fog Off、`12–40` 与 `10–32`。关闭 Fog 时远景与地平线分离；`10–32` 对紧凑世界的淡化过强；`12–40` 在保留树木/岩石颜色的同时提供了大气深度。Terrain、树皮、Alpha Clip 叶片、岩石、阴影与 Player 均保持正常。

## 雾与地平线匹配

Fog Color 与材质 Horizon Color 完全一致：`(0.72, 0.84, 0.82)`。远处表面会逐渐靠近其背景色，不会出现灰色或蓝色断层。这种深度线索能让小型程序化世界形成统一空间感，但不会伪装成无限世界。

## 方向光调整

实际比较了两组光照方向。新的 `42°, -55°, 0°` 侧向角度强化了切面，但前景植被过暗且阴影过长；既有 `48°, -32°, 0°` 更均衡，因此 Rotation、暖色 `(1.00, 0.94, 0.84)` 与强度 `1.15` 均保持不变。Skybox Ambient Mode 与强度 `1.0` 也不变；自定义地形/环境材质继续使用 `0.32–0.35` Ambient，保证背光面可读但不抹平明暗分档。

## 阴影距离

保留 Commit 12 的稳定配置：Hard Shadow、`2048` 主光阴影贴图、`40` 距离、两级 Cascade、Bias `0.05`、Normal Bias `0.4`、Near Plane `0.2`。Shadow Distance 与 Fog End 现在一致，避免计算大气范围之外不可见的阴影。树木、岩石、地形和 Player 均未再出现早期植被拉长伪影。

## 大气渲染流程

最终大气只由场景配置和一个天空 Shader 组成，没有新增运行时 Manager。它与 `MeshGenerator`、`TerrainGenerator`、`EnvironmentSpawner`、Player、Camera 和 World Seed 保持独立。同 Seed 重生成仍得到签名 `2087925580`、`18` 棵树与 `12` 块岩石。

## 运行验证

- 两个自定义 Shader 均受支持；四个环境适配材质均保留源贴图
- 高度范围：`-1.755～2.029`；最大坡度：`28.348°`
- 树木/岩石：`18/12`
- 重复生成 Signature：两次均为 `2087925580`
- 六个环境 Prefab 全部使用自定义 Shader，并保留启用的 Collider
- Player 存在且启动后正常落地，地形碰撞保持有效
- 未发现 C# 或 Shader 编译错误
- 已在 Play Mode 对比 Fog Off / `12–40` / `10–32`、两套 Palette、两种光照角度与地面/高处/地平线视角
- 最终天空通过 RenderSettings 绑定；Camera 继续使用 Skybox Clear Mode

## 当前限制

地形 Shader 使用 `RecalculateNormals` 生成的顶点法线，因此地形仍为平滑着色。当前渲染层没有地形纹理层、Triplanar、环境 Normal Map、附加光源循环、物理大气散射、体积雾、云、后处理 Volume、自定义 GI 或平台专项阴影调优。环境适配层刻意采用直接光/环境光分档，而不是完整 URP Lit PBR 特性集。

## 后续渲染方向

后续 Commit 可以研究地形/环境配色统一、远距离可读性与平台专项阴影质量。纹理混合和更高级技术应作为独立、可验证的增量实现。
