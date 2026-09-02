# 渲染与 Shader

[English](./RenderingAndShaders.md) | **简体中文**

## 概述

GenesisWorld v0.3.0 渲染基础使用 Unity `2022.3.62f3c1`、Universal Render Pipeline `14.0.12`、ShaderLab 与手写 HLSL。三个紧凑的自定义 Shader 分别呈现生成地形、带贴图的 Low-poly 资产和天空。设计重点是让图形学基础保持清晰，而不是覆盖完整 PBR 功能集。

面向作品集展示的学习路线见 [v0.3.0 渲染与 Shader 里程碑](./RenderingAndShaders_Milestone.zh-CN.md)。

## 渲染架构

```text
CPU 世界生成                                  GPU 世界呈现
MeshGenerator ── 几何 ────────────────────→ StylizedTerrain
TerrainGenerator ── Mesh/Collider 生命周期 ↗    ↑ 主光 / 阴影 / 雾
EnvironmentSpawner ── 放置实例 ───────────→ StylizedEnvironment
摄像机观察方向 ───────────────────────────→ StylizedSkybox
RenderSettings ── Skybox + Linear Fog ────→ 最终风格化场景
```

程序化 C# 代码决定**几何和实例在哪里**，GPU 决定**可见表面如何呈现**。Shader 不重新采样 Perlin Noise，不改变 World Seed，也不管理环境放置。

## Shader 流程

```text
顶点位置 + 法线
        ↓ Vertex Stage
对象空间 → 世界空间 → 裁剪空间
        ↓ 光栅化 / 插值
世界位置 + 世界法线 + 主光
        ↓ Fragment Stage
表面颜色 → 光照 → 阴影衰减 → 雾
        ↓
最终 Pixel 颜色
```

世界空间为地形坡度、主光方向与天空方向提供稳定的共同坐标系。

## 风格化地形

[`StylizedTerrain.shader`](../Assets/Shaders/Terrain/StylizedTerrain.shader) 无需地形贴图集即可为生成地面着色。其 `UniversalForward` Pass 接收世界位置、世界法线、主光阴影坐标和 Fog Factor；URP Lit 的 `ShadowCaster` 与 `DepthOnly` Pass 提供深度和投影支持。

### 基于高度的颜色

```text
heightFactor = saturate((worldY - HeightMin) / max(HeightMax - HeightMin, 0.0001))
heightColor = lerp(LowColor, HighColor, heightFactor)
```

Epsilon 防止除零。当前材质范围为 `-2.5～2.5`，对应以零为中心、Height Scale 为 `5` 的地形。

### 坡度检测

```text
upAlignment = saturate(dot(normalWS, float3(0, 1, 0)))
slope = 1 - upAlignment
slopeFactor = smoothstep(SlopeStart, SlopeEnd, slope)
baseColor = lerp(heightColor, SlopeColor, slopeFactor)
```

平地法线与 World Up 对齐，坡度颜色较少；倾斜法线会降低点积，并平滑引入土石颜色。

### 地形光照

```text
NdotL = saturate(dot(normalWS, lightDirectionWS))
direct = NdotL * distanceAttenuation * shadowAttenuation
lighting = AmbientStrength + mainLightColor * direct
```

这是带标量环境亮度下限的轻量 Lambert 漫反射，不是完整 PBR BRDF 或采样 GI 方案。

## 风格化环境

[`StylizedEnvironment.shader`](../Assets/Shaders/Environment/StylizedEnvironment.shader) 由项目自有树木/岩石适配材质使用。它保留源贴图与 Tint，同时通过包裹式、量化的直接光提升 Low-poly 切面可读性。

### BaseMap 与 BaseColor

```text
surface = sample(BaseMap, uv) * BaseColor
```

项目不修改导入的 CC0 源材质；适配材质引用其贴图并应用自定义 Shader。

### 世界空间法线与光照量化

完整 `N·L` 解释已在上方地形光照中给出。环境着色额外加入包裹与分档：

```text
wrapped = saturate((saturate(dot(N, L)) + LightWrap) / (1 + LightWrap))
steps = max(round(LightSteps), 2)
banded = round(wrapped * (steps - 1)) / (steps - 1)
```

当前默认选择三档。包裹项让略微背光的切面仍可读；量化把连续漫反射变成受控的 Low-poly/Cell 风格明暗语言。

### 透明裁剪

启用 `_ALPHATEST_ON` 时，低于 `_Cutoff` 的 Fragment 会被丢弃。Forward、`ShadowCaster` 与 `DepthOnly` Pass 调用同一个表面采样函数，因此植被颜色、阴影轮廓和深度轮廓保持一致。

### 阴影与深度

自定义 `ShadowCaster` 使用 URP Shadow Bias，并支持方向光/点光投影变体；`DepthOnly` 只写深度。经同机位 Hard/Soft 对比后采用 Hard Shadow，因为其轮廓在当前尺度下更清晰。

## 风格化天空盒

[`StylizedSkybox.shader`](../Assets/Shaders/Sky/StylizedSkybox.shader) 是无贴图的观察方向渐变：

| 参数 | 当前值 | 作用 |
|---|---:|---|
| Zenith Color | `(0.18, 0.42, 0.72)` | 上层天空蓝色 |
| Horizon Color | `(0.72, 0.84, 0.82)` | 大气过渡与雾目标色 |
| Lower Color | `(0.32, 0.38, 0.28)` | 自然下半球颜色 |
| Horizon Exponent | `0.65` | 渐变过渡形状 |

### 观察方向与地平线渐变

Vertex Stage 将立方体方向转换到世界空间；Fragment Stage 归一化后读取 Y：正值从地平线混合到天顶，负值从地平线混合到下半球。

```text
upper = pow(smoothstep(0, 1, saturate(viewDirection.y)), HorizonExponent)
lower = pow(smoothstep(0, 1, saturate(-viewDirection.y)), HorizonExponent)
```

两个半球在同一个 Horizon Color 汇合，避免人为接缝。

## 雾与大气深度

场景使用 `12–40` Unity Linear Fog。地形与环境 Forward Pass 编译 Fog Variant，计算 `ComputeFogFactor`，并在光照之后调用 `MixFog`。

```text
FinalColor = lerp(SurfaceColor, FogColor, FogFactor)
```

Fog Color 与 Skybox Horizon Color 完全一致：`(0.72, 0.84, 0.82)`。运行对比覆盖 Fog Off、`12–40` 与 `10–32`；`12–40` 能保留局部颜色并提供有效距离层次。

## 光照与质量配置

- Directional Light：Rotation `(48, -32, 0)`，暖色 `(1.00, 0.94, 0.84)`，Intensity `1.15`
- Ambient Source：Skybox，Intensity `1.0`；自定义材质标量 Ambient 约为 `0.32–0.35`
- High Fidelity 阴影：Hard、`2048` 主光贴图、`40` 距离、两级 Cascade
- Light Bias `0.05`；Normal Bias `0.4`；Near Plane `0.2`
- Shadow Distance 与 Fog End 都是 `40`

此基础不需要 Global Volume 或自定义 Atmosphere Manager。

## 运行验证

- 场景：`Assets/Scenes/Test_Player_Controller.unity`
- 地形：`20 × 20`，`50 × 50` 分段，Height Scale `5`
- 环境：`18` 棵树、`12` 块岩石；适配材质保留源贴图
- Seed `12345`：重复 Layout Signature `2087925580`
- 地形、环境、天空盒、硬阴影与 Linear Fog 正常渲染，无粉色材质
- 里程碑验证期间项目 C# 与 Shader Error：`0`

## 当前限制

- 仅处理主方向光，没有自定义 Additional Lights 循环
- 自定义地形/环境 Shader 不包含 PBR 工作流
- 地形使用平滑顶点法线，没有 Triplanar Texture Layer 或 Normal Map
- 没有水体、植被风动、天气、昼夜循环、云或体积雾
- 没有屏幕空间效果或后处理框架
- 仅有小型单块程序化地图，没有 Biome、Chunk、Streaming 或 LOD

## 后续渲染方向

未来可选择研究水体、风动、Additional Lights、后处理或 LOD，但都需要独立设计与验证，不属于 v0.3.0，也不属于规划中的 v0.4.0 AI NPC 里程碑。
