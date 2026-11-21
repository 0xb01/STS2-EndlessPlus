# EndlessPlus (BNKZ.STS2.EndlessPlus) - Function & Architecture Summary

## 1. Overview
`BNKZ.STS2.EndlessPlus` is an Endless Mode mod for **Slay the Spire 2** featuring configurable multi-interval stat scaling (Act, Chapter, Floor), mathematical scaling curves (Exponential, Linear, Fixed), flat stat bonuses for co-op balance, in-game BaseLib GUI configuration, and deterministic multiplayer synchronization.

---

## 2. Architecture & Data Flow

```
[Defeat Act 3 Boss] 
        │
        ▼
[GameOverScreenPatches.HandleAfterReady] ──> Injects "Endless" & "Force Endless" Buttons
        │
        ▼ (Host clicks "Endless")
[GameOverScreenPatches.OnEndlessButtonPressedAsync]
        │
        ▼
[EndlessRuntime.StartLoopFromGameOverScreenAsync]
        │──> Holds multiplayer connection (SetPreserveMultiplayerConnectionDuringGameOver)
        │──> Increments loop count & calculates Ascension progression (PrepareLoop)
        │──> Reseeds deterministic RNGs (rewards, events, player seeds, map points)
        │──> Randomizes boss encounters & act ancients
        │──> Regenerates Act 1 map & resets act/floor coordinates
        │──> Revives all dead players in save snapshot
        │──> Persists authoritative scaling metadata to run state
        │──> Reconstructs & loads fresh run state into main game scene
        ▼
[Act 1 Exploration & Combat Rooms]
        │
        ▼
[CombatScalingPatches]
        │──> Calculates scaling steps via [ScalingCalculator.CalculateSteps]
        │──> Intercepts Creature creation & multiplayer HP scaling -> applies HP multiplier + flat HP
        │──> Intercepts AttackCommand.Execute -> scales damage per hit + flat damage
        │──> Intercepts NIntent.UpdateIntent -> scales combat intent UI previews
```

---

## 2.1 Project Directory Structure (`/src/`)

```
src/
├── Config/               # Configuration state, file I/O, and BaseLib Mod Configuration UI
│   ├── BaseLibIntegration.cs
│   ├── EndlessConfig.cs
│   └── EndlessPlusModConfig.cs
├── Core/                 # Mod bootstrap, runtime state coordinator, logging, and platform fixes
│   ├── EndlessBootstrap.cs
│   ├── EndlessLog.cs
│   ├── EndlessReflection.cs
│   ├── EndlessRuntime.cs
│   └── LinuxNativeBootstrap.cs
├── Patches/              # Harmony runtime method interceptors
│   ├── CombatRoomPatches.cs
│   ├── CombatScalingPatches.cs
│   ├── EventSynchronizerPatches.cs
│   ├── GameLifecyclePatches.cs
│   ├── GameOverScreenPatches.cs
│   ├── LobbyPatches.cs
│   ├── MapSelectionSynchronizerPatches.cs
│   ├── NeowPatches.cs
│   ├── RelicInventoryPatches.cs
│   ├── RewardsSetSynchronizerPatches.cs
│   └── RunManagerPatches.cs
├── Scaling/              # Mathematical scaling curves, multipliers, and stat calculations
│   └── ScalingCalculator.cs
└── UI/                   # Godot UI controls, status HUD overlay, and transition dialogs
    ├── ArchitectRewardOverlay.cs
    └── RunUiPatches.cs
```

---

## 3. Core Modules & Function Reference

### 3.1 `EndlessRuntime`
Central state coordinator and loop manager.

| Function | Visibility | Description |
| :--- | :--- | :--- |
| `GetCompletedLoops(object runState)` | `public` | Returns completed loop count; synchronizes from serialized run state metadata or floor count heuristics. |
| `GetCurrentChapter(object runState)` | `public` | Returns 1-based current chapter index (`CompletedLoops + 1`). |
| `GetUpcomingChapter(object runState)` | `public` | Returns 1-based upcoming chapter index for the next loop. |
| `CanLocalPlayerLaunchEndless(...)` | `public` | Returns `true` in singleplayer, or checks host authority in multiplayer. |
| `AreAllPlayersReadyForEndless(...)` | `public` | Checks whether all connected multiplayer peers are marked ready before starting next chapter. |
| `SetPreserveMultiplayerConnectionDuringGameOver(...)` | `public` | Locks connection teardown during game over screen to prevent lobby disconnects. |
| `GetEnemyScalingMultiplier(object runState)` | `public` | Gets current enemy HP multiplier from `ScalingCalculator`. |
| `GetEnemyDamageScalingMultiplier(object runState)` | `public` | Gets current enemy attack damage multiplier from `ScalingCalculator`. |
| `GetFlatHpBonus(object runState)` | `public` | Gets current flat HP addition from `ScalingCalculator`. |
| `GetFlatDamageBonus(object runState)` | `public` | Gets current flat attack damage addition from `ScalingCalculator`. |
| `GetRunStatusText(object runState)` | `public` | Formats status text for top HUD banner overlay. |
| `GetEnemyHpScalePerChapter(object runState)` | `internal` | Retrieves authoritative HP scale rate (reads host run state metadata in multiplayer). |
| `GetEnemyDamageScalePerChapter(object runState)` | `internal` | Retrieves authoritative damage scale rate (reads host run state metadata in multiplayer). |
| `TryWrapActIndex(object runManager, ref int actIndex)` | `public` | Intercepts act index increments beyond Act 3, resets index to 0, and prepares next loop. |
| `ShouldInterceptWin(object runManager)` | `public` | Detects victory condition after Act 3 to redirect to Endless loop flow. |
| `ForceNextLoopAsync(object runManager)` | `public` | Advances run immediately into next endless loop Act 1 without showing game over screen. |
| `StartLoopFromGameOverScreenAsync(...)` | `public` | Main async loop reconstruction routine: rebuilds run state, revives players, clears visited coordinates, and loads Act 1. |
| `PrepareLoop(object runManager, object runState)` | `private` | Advances loop counter, escalates Ascension, reseeds deterministic RNGs, randomizes bosses, and resets map. |
| `SynchronizeLoopCountFromRunState(object runState)` | `private` | Calculates loop count from run metadata or floor count with underflow defense (`(totalFloor + 3) / cycleFloorCount`). |
| `ResetTransientState(string reason)` | `public` | Flushes transient in-memory run state upon returning to main menu or aborting runs. |
| `TryConsumeFirstCombatResetRequest()` | `public` | Consumes one-shot flag requesting action queue reset on the first combat room entered after loop load. |

---

### 3.2 `ScalingCalculator`
Pure calculation engine for monster stat scaling.

| Function | Visibility | Description |
| :--- | :--- | :--- |
| `CalculateSteps(object runState)` | `public` | Computes cumulative progression steps elapsed based on active interval (`Act`, `Chapter`, or `Floor`). Honors Chapter 1 protection (`skip_chapter`). |
| `GetHpMultiplier(object runState)` | `public` | Calculates enemy Max HP multiplier using host-synced or local scale factor across elapsed steps. |
| `GetDamageMultiplier(object runState)` | `public` | Calculates enemy attack damage multiplier using host-synced or local scale factor across elapsed steps. |
| `GetFlatHpBonus(object runState)` | `public` | Computes flat monster HP added per step (`flat_hp_per_step * steps`). |
| `GetFlatDamageBonus(object runState)` | `public` | Computes flat attack damage added per hit per step (`flat_dmg_per_step * steps`). |
| `FormatStatusText(object runState)` | `public` | Formats comprehensive HUD banner: `ENDLESS (Exp/Act · A10) \| Ch 2 Act 1 (Step 3): HP +26% (x1.26) · DMG +15.8% (x1.16) [Flat: +1500 HP / +6 DMG] \| Next: +36%/+21.6%`. |
| `ComputeMultiplier(...)` | `private` | Evaluates mathematical scaling curve: Exponential (`scale ^ steps`), Linear (`1 + (scale - 1) * steps`), or Fixed (`scale`). Clamps output within `[min, max]`. |
| `Clamp(...)` | `private` | Constrains multipliers within configured minimum floor and optional maximum cap. |

---

### 3.3 `EndlessConfig`
Configuration file parser, hot-reload watcher, and serializer.

| Function | Visibility | Description |
| :--- | :--- | :--- |
| `Load()` | `public` | Reads and parses `BNKZ.STS2.EndlessPlus.config` from disk, applying defaults for missing keys. |
| `CheckReload()` | `public` | Watches file write timestamps for hot-reloading; blocked during active combat (`CombatManager.Instance?.IsInProgress == true`) to prevent desyncs. |
| `ApplySettings(...)` | `public` | Updates in-memory settings from GUI / Mod Configuration menu and optionally persists to disk. |
| `SaveToConfigFile()` | `public` | Writes updated configuration values back to disk while preserving comments and layout. |
| `EnsureConfigExists()` | `private` | Generates default documented `.config` file if missing on startup. |

---

### 3.4 `EndlessPlusModConfig`
BaseLib integration for in-game Mod Configuration page UI.

| Function | Visibility | Description |
| :--- | :--- | :--- |
| `SetupConfigUI(Control optionContainer)` | `public override` | Injects Mod Configuration UI controls and adds yellow multiplayer synchronization notice banner. |
| `PushToBaseConfig(bool saveToFile)` | `public static` | Propagates GUI slider adjustments to `EndlessConfig` and triggers disk persistence. |
| `SyncFromEndlessConfig()` | `public static` | Synchronizes GUI property bindings whenever `.config` file is loaded or hot-reloaded. |
| `RegisterLocalizations()` | `public static` | Injects localized UI labels and hover tooltip descriptions into `LocManager`. |

---

### 3.5 `CombatScalingPatches`
Harmony combat interceptors for HP, damage, and intent rendering.

| Function | Visibility | Description |
| :--- | :--- | :--- |
| `AfterCreateCreature(object? __result)` | `public` | Postfix on `CombatState.CreateCreature` to apply HP scaling. |
| `AfterSetUniqueMonsterHpValue(object __instance)` | `public` | Postfix on `Creature.SetUniqueMonsterHpValue` to re-scale unique monster HP values. |
| `AfterScaleMonsterHpForMultiplayer(object __instance)` | `public` | Postfix on `Creature.ScaleMonsterHpForMultiplayer` to apply endless scaling after party HP scaling. |
| `BeforeAddCreature(object creature)` | `public` | Prefix on `CombatState.AddCreature` to verify creature HP scaling before room registration. |
| `BeforeAttackExecute(object __instance)` | `public` | Prefix on `AttackCommand.Execute` to scale damage per hit by percentage multiplier and flat damage bonus. |
| `BeforeUpdateIntent(object intent, object owner)` | `public` | Prefix on `NIntent.UpdateIntent` to scale enemy intent damage numbers displayed above heads. |
| `TryScaleCreatureHp(object creature, string source)` | `private` | Core HP recalculation routine; uses `ConditionalWeakTable` caching to prevent double-scaling. |

---

### 3.6 `GameOverScreenPatches`
Harmony patches for the Game Over / Victory screen.

| Function | Visibility | Description |
| :--- | :--- | :--- |
| `HandleAfterReady(object __instance)` | `public` | Postfix on `GameOverScreen._Ready` to inject "Endless" and "Force Endless" action buttons. |
| `OnEndlessButtonPressedAsync(...)` | `private` | Click handler for "Endless" button; disables inputs, synchronizes multiplayer peers, and begins loop load. |
| `OnForceButtonPressedAsync(...)` | `private` | Click handler for "Force Endless" button; host override bypassing unready peers. |

---

### 3.7 `EndlessBootstrap`
Mod lifecycle and entry point.

| Function | Visibility | Description |
| :--- | :--- | :--- |
| `Initialize()` | `public static` | Entry point called by Slay the Spire 2 / BaseLib; applies all Harmony patches and registers config UI. |

---

## 4. Multiplayer Synchronization Guarantees

1. **Host Authoritative Metadata**:
   - Host persists authoritative `hp_scale` and `dmg_scale` directly into the run state metadata.
   - Connected clients read scaling factors from host metadata rather than local files, ensuring zero math divergence.
2. **Floor Math Underflow Defense**:
   - In Act 3 boss rooms (`totalFloor = 47`), `(47 + 3) / 48` evaluates cleanly to Loop 1, preventing clients from dropping to Loop 0.
3. **Mid-Combat Hot-Reload Lockout**:
   - File modification checks are skipped whenever `CombatManager.Instance?.IsInProgress == true`, preventing divergence during combat turns.
4. **Game-Over Connection Hold**:
   - Connection teardown is prevented until the new Act 1 map has fully generated and loaded for all clients.
