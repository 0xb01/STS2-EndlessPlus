# Dependency Assemblies (`lib/`)

This directory is used for portable compilation and GitHub Actions CI runner builds when Slay the Spire 2 is not installed in the default Steam path.

### Required Game Assemblies:
- `GodotSharp.dll` (from `<STS2_INSTALL>/data_sts2_windows_x86_64/GodotSharp.dll`)
- `sts2.dll` (from `<STS2_INSTALL>/data_sts2_windows_x86_64/sts2.dll`)
- `0Harmony.dll` (from `<STS2_INSTALL>/data_sts2_windows_x86_64/0Harmony.dll`)
- `BaseLib.dll` (from `<STS2_INSTALL>/mods/BaseLib/BaseLib.dll`)

> [!NOTE]
> Proprietary game assemblies are omitted from git tracking. Place assemblies in this directory or provide the `STS2_LIBS_BASE64` secret in GitHub repository settings.
