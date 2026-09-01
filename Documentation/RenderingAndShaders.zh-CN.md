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
- 复用 URP Lit 的 `ShadowCaster` 与 `DepthOnly` Pass
- 通过 `ComputeFogFactor` 和 `MixFog` 兼容 Fog
- 材质参数位于 `UnityPerMaterial` CBUFFER，兼容 SRP Batcher

默认场景仍关闭 Directional Light Shadow，因为当前导入植被在低分辨率下会产生干扰展示的阴影轮廓。Soft Shadow 运行对比已确认 Terrain 能接收阴影，随后恢复原场景默认值。Shader 兼容 Fog，但本 Commit 没有新增 Fog System。

## 运行验证

- Shader Supported：true；8 个公开参数全部存在
- 高度范围：`-1.755～2.029`；最大坡度：`28.348°`
- 树木/岩石：`18/12`
- 重复生成 Signature：两次均为 `2087925580`
- 启动后 Player 正常落地，地形碰撞保持有效
- 未发现 C# 或 Shader 编译错误

## 当前限制

Shader 使用 `RecalculateNormals` 生成的顶点法线，因此地形仍为平滑着色。当前没有纹理层、Triplanar、Normal Map、附加光源循环、自定义 GI、Cel 分级或地形专用阴影过滤。环境资产继续使用原有 URP 材质。

## 后续渲染方向

后续 Commit 可以研究风格化环境光照与可控阴影质量。纹理混合和更高级技术应作为独立、可验证的增量实现。
