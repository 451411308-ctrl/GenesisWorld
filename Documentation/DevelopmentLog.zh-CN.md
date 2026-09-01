# GenesisWorld 开发日志

[English](./DevelopmentLog.md) | **简体中文**

## 第一周 — 核心框架

### Commit 1 — `Initialize GenesisWorld project structure`
初始化 Unity 2022 LTS、模块化资产目录、URP/工程设置、Git 规范与文档体系。

### Commit 2 — `Add player controller system`
实现 CharacterController 移动、冲刺、跳跃、地面检测、重力、可配置参数与 Animator 参数接口。

### Commit 3 — `Add third person camera system`
实现目标跟随、鼠标环绕、俯仰限制、平滑移动、缩放、光标管理与摄像机相对移动。

### Commit 4 — `Update documentation and create Week 1 milestone`
发布核心框架总结与注解标签 `v0.1.0`。

## 第二周 — 程序化世界

### Commit 5 — `Add procedural terrain generation foundation`
分离平面网格数据计算与 Unity Mesh 管理，生成顶点、三角形、UV、法线、包围盒与碰撞。

### Commit 6 — `Implement noise-based terrain generation`
加入中心化 Perlin Noise 高度以及频率、振幅与偏移参数。

### Commit 7 — `Add seeded procedural world generation`
使用局部 `System.Random` 将 World Seed 映射为稳定噪声偏移，同时保留手动调试偏移。

### Commit 8 — `Add procedural environment spawning`
通过独立随机流、地形 Raycast、坡度过滤、间距与重新生成事件实现确定性的树木和岩石放置。

### Commit 9 — `Integrate low-poly environment`
加入项目自制 Low-poly 备用 Variant、URP 材质、碰撞体与首张真实 Game View 展示图。

### Commit 9 美术升级 — `Integrate curated third-party environment assets`
集成 Quaternius Stylized Nature MegaKit 的最小 CC0 子集，重建 URP 材质、统一 Prefab、记录许可证，并完成 Seed A/B/A 可复现验证。

### Commit 10 — `Update procedural world documentation and milestone`
将公开文档拆分为英文与简体中文文件，重构 GitHub 项目主页，整理完整程序化流程，并准备 `v0.2.0` 程序化世界里程碑。

## 第三周 — 渲染与 Shader

### Commit 11 — `Add stylized terrain shader foundation`
新增项目首个自定义 URP 地形 Shader，通过世界空间高度、世界空间表面法线、平滑坡度混合、主方向光 Lambert 光照、可调环境亮度、阴影变体与 Fog 兼容，生成可参数化的风格化地形表现。旧地形材质继续作为备用方案保留。

### Commit 12 — `Add stylized environment lighting`
新增自定义 URP 环境 Shader，支持可调明暗分层、包裹式漫反射、环境补光、原贴图与颜色保留、植被透明裁剪，以及兼容透明裁剪的深度/阴影 Pass。项目自有适配材质覆盖全部已集成树木与岩石 Prefab，同时不修改 CC0 源资源。经同机位硬/软阴影运行对比后，最终采用硬方向光阴影。
