# Unity Project Configuration

**English** | [简体中文](./ProjectConfiguration.zh-CN.md)

## Baseline

| Area | Configuration |
|---|---|
| Editor | Unity `2022.3.62f3` LTS |
| Rendering | Universal Render Pipeline |
| Scripting | C# |
| Input | Unity Input System package; current controllers use the configured input path |
| Quality | URP assets under `Assets/Settings` |
| Version control | Visible meta files, Force Text serialization, Unity `.gitignore` |

URP is appropriate because GenesisWorld targets a stylized environment, Shader Graph experimentation, broad hardware compatibility, and manageable rendering complexity. It provides a practical foundation for later graphics work without introducing a high-end-only pipeline.

## Initialization Record

- Created modular `Assets` and `Documentation` directories.
- Configured project identity and Unity text serialization.
- Installed and recorded package dependencies in `Packages/manifest.json`.
- Assigned URP assets to graphics and quality settings.
- Excluded `Library`, `Temp`, `Obj`, `Build`, `Logs`, and `UserSettings` from Git.

Runtime parameters and scene-specific procedural settings are documented separately.
