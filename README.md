# STS2-EndlessPlus

A Slay the Spire 2 mod that let's us loop the game endlessly.

Customizable Endless Mode for **Slay the Spire 2** (v0.111.1+). Features multi-interval stat scaling (Act, Chapter, Floor), configurable mathematical curves (Exponential, Linear, Fixed), flat stat bonuses for co-op balance, in-game BaseLib Mod Configuration UI integration, and deterministic host-authoritative multiplayer synchronization.

---

## Project Structure

```
STS2-EndlessPlus/
├── BNKZ.STS2.EndlessPlus.csproj   # .NET 9.0 project build file
├── BNKZ.STS2.EndlessPlus.json     # STS2 mod manifest
├── README.md                      # Project root readme
├── docu/                          # Documentation folder
│   └── FUNCTIONS_SUMMARY.md       # Architecture & function reference guide
├── src/                           # C# source files
│   ├── ArchitectRewardOverlay.cs
│   ├── CombatRoomPatches.cs
│   ├── CombatScalingPatches.cs
│   ├── EndlessBootstrap.cs
│   ├── EndlessConfig.cs
│   ├── EndlessLog.cs
│   ├── EndlessPlusModConfig.cs
│   ├── EndlessReflection.cs
│   ├── EndlessRuntime.cs
│   ├── EventSynchronizerPatches.cs
│   ├── GameLifecyclePatches.cs
│   ├── GameOverScreenPatches.cs
│   ├── LinuxNativeBootstrap.cs
│   ├── LobbyPatches.cs
│   ├── MapSelectionSynchronizerPatches.cs
│   ├── NeowPatches.cs
│   ├── RelicInventoryPatches.cs
│   ├── RewardsSetSynchronizerPatches.cs
│   ├── RunManagerPatches.cs
│   ├── RunUiPatches.cs
│   └── ScalingCalculator.cs
└── Properties/
    └── AssemblyInfo.cs
```

---

## Compilation & Installation

```bash
dotnet build -c Release
```
Compiles `.dll` to `bin/Release/` and auto-deploys to `Slay the Spire 2/mods/EndlessPlus`.
