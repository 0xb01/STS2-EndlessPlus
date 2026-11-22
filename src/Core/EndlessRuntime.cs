using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace BNKZ.STS2.EndlessPlus;

internal static class EndlessRuntime
{
	private static string? _lastTransitionKey;

	private static string? _lastScalingWarningKey;

	private static int _loopCount;

	private static bool _awaitingFirstCombatAfterLoopLoad;

	private static bool _preserveMultiplayerConnectionDuringGameOver;

	public static bool IsLaunchingFromGameOverScreen { get; private set; }

	public static bool ShouldPreserveMultiplayerConnectionDuringGameOver => _preserveMultiplayerConnectionDuringGameOver;

	/// <summary>
	/// Returns the number of completed loops for the current run, synchronizing with run state metadata or floor count if needed.
	/// </summary>
	public static int GetCompletedLoops(object runState)
	{
		SynchronizeLoopCountFromRunState(runState);
		return _loopCount;
	}

	/// <summary>
	/// Clears all transient in-memory run state (loop counts, transition cache, flags) upon return to main menu or run termination.
	/// </summary>
	public static void ResetTransientState(string reason)
	{
		bool flag = _loopCount != 0 || _lastTransitionKey != null || _awaitingFirstCombatAfterLoopLoad || IsLaunchingFromGameOverScreen || _preserveMultiplayerConnectionDuringGameOver;
		_lastTransitionKey = null;
		_lastScalingWarningKey = null;
		_loopCount = 0;
		_awaitingFirstCombatAfterLoopLoad = false;
		IsLaunchingFromGameOverScreen = false;
		_preserveMultiplayerConnectionDuringGameOver = false;
		if (flag)
		{
			EndlessLog.Info("Reset endless runtime state (" + reason + ").");
		}
	}

	/// <summary>
	/// Gets 1-based current chapter index (Loop 0 = Chapter 1, Loop 1 = Chapter 2, etc.).
	/// </summary>
	public static int GetCurrentChapter(object runState)
	{
		return GetCompletedLoops(runState) + 1;
	}

	/// <summary>
	/// Gets 1-based upcoming chapter index for the next endless loop.
	/// </summary>
	public static int GetUpcomingChapter(object runState)
	{
		return GetCurrentChapter(runState) + 1;
	}

	/// <summary>
	/// Determines if the local client is permitted to trigger the endless loop transition (true for singleplayer, host-only in multiplayer).
	/// </summary>
	public static bool CanLocalPlayerLaunchEndless(object runManager, object runState)
	{
		if (EndlessReflection.GetPlayersCount(runState) <= 1)
		{
			return true;
		}
		return EndlessReflection.IsLocalPlayerHost(runManager);
	}

	/// <summary>
	/// Verifies whether all connected multiplayer peers are marked ready before starting next chapter.
	/// </summary>
	public static bool AreAllPlayersReadyForEndless(object runManager, object runState)
	{
		if (EndlessReflection.GetPlayersCount(runState) <= 1)
		{
			return true;
		}
		if (EndlessReflection.TryShouldAllowRunToBegin(runManager, out var shouldAllow))
		{
			return shouldAllow;
		}
		if (EndlessReflection.TryGetReadyPlayerCount(runManager, out var readyCount))
		{
			return readyCount >= EndlessReflection.GetPlayersCount(runState);
		}
		return CanLocalPlayerLaunchEndless(runManager, runState);
	}

	/// <summary>
	/// Holds or releases the multiplayer peer connection during the game-over screen to prevent lobby disconnects.
	/// </summary>
	public static void SetPreserveMultiplayerConnectionDuringGameOver(bool preserve, string reason)
	{
		if (_preserveMultiplayerConnectionDuringGameOver != preserve)
		{
			_preserveMultiplayerConnectionDuringGameOver = preserve;
			EndlessLog.Info((preserve ? "Preserving" : "Releasing") + " multiplayer game-over connection hold (" + reason + ").");
		}
	}

	/// <summary>
	/// Gets enemy HP scaling multiplier for the current run state.
	/// </summary>
	public static decimal GetEnemyScalingMultiplier(object runState)
	{
		return ScalingCalculator.GetHpMultiplier(runState);
	}

	/// <summary>
	/// Gets enemy damage scaling multiplier for the current run state.
	/// </summary>
	public static decimal GetEnemyDamageScalingMultiplier(object runState)
	{
		return ScalingCalculator.GetDamageMultiplier(runState);
	}

	/// <summary>
	/// Gets flat monster HP bonus for the current run state.
	/// </summary>
	public static int GetFlatHpBonus(object runState)
	{
		return ScalingCalculator.GetFlatHpBonus(runState);
	}

	/// <summary>
	/// Gets flat monster damage bonus for the current run state.
	/// </summary>
	public static int GetFlatDamageBonus(object runState)
	{
		return ScalingCalculator.GetFlatDamageBonus(runState);
	}

	/// <summary>
	/// Formats top HUD banner status text based on active configuration.
	/// </summary>
	public static string GetRunStatusText(object runState)
	{
		if (EndlessConfig.OverrideHud)
		{
			return ScalingCalculator.FormatStatusText(runState);
		}
		int currentChapter = GetCurrentChapter(runState);
		int value = EndlessReflection.GetCurrentActIndex(runState) + 1;
		decimal enemyScalingMultiplier = GetEnemyScalingMultiplier(runState);
		decimal enemyDamageScalingMultiplier = GetEnemyDamageScalingMultiplier(runState);
		string prefix = currentChapter > 1 ? "Endless: " : "";
		return $"{prefix}Chapter {currentChapter}  Act {value}  HP x{enemyScalingMultiplier:0.##}  DMG x{enemyDamageScalingMultiplier:0.##}";
	}

	/// <summary>
	/// Formats top HUD banner BBCode status text based on active configuration.
	/// </summary>
	public static string GetRunStatusBbCode(object runState, bool collapsed)
	{
		if (EndlessConfig.OverrideHud)
		{
			return ScalingCalculator.FormatStatusBbCode(runState, collapsed);
		}
		int currentChapter = GetCurrentChapter(runState);
		int value = EndlessReflection.GetCurrentActIndex(runState) + 1;
		decimal enemyScalingMultiplier = GetEnemyScalingMultiplier(runState);
		decimal enemyDamageScalingMultiplier = GetEnemyDamageScalingMultiplier(runState);
		string prefix = currentChapter > 1 ? "[color=#f1c40f][b]∞ " : "[color=#f1c40f][b]";
		string suffix = "[/b][/color]";
		if (collapsed)
		{
			return $"{prefix}Chapter {currentChapter}{suffix} [color=#6e6557]·[/color] [color=#cfc7b8]Act {value}[/color]";
		}
		return $"{prefix}Chapter {currentChapter}{suffix} [color=#6e6557]·[/color] [color=#cfc7b8]Act {value}[/color]   [color=#5a4d3a]│[/color]   [color=#ff6b6b]♥ x{enemyScalingMultiplier:0.##}[/color]   [color=#5a4d3a]│[/color]   [color=#f39c12]⚔ x{enemyDamageScalingMultiplier:0.##}[/color]";
	}

	/// <summary>
	/// Formats tooltip details for the top HUD banner.
	/// </summary>
	public static string GetRunStatusTooltip(object runState)
	{
		if (EndlessConfig.OverrideHud)
		{
			return ScalingCalculator.FormatStatusTooltip(runState);
		}
		int currentChapter = GetCurrentChapter(runState);
		int act = EndlessReflection.GetCurrentActIndex(runState) + 1;
		decimal hpMult = GetEnemyScalingMultiplier(runState);
		decimal dmgMult = GetEnemyDamageScalingMultiplier(runState);
		return $"Endless Mode: Chapter {currentChapter}, Act {act}\n" +
		       $"Enemy HP: x{hpMult:0.##}\n" +
		       $"Enemy DMG: x{dmgMult:0.##}\n\n" +
		       $"[Drag anywhere to reposition · Double-click to reset · Click toggle to collapse]";
	}

	/// <summary>
	/// Fetches the authoritative HP scale rate per chapter (reads host run state metadata in multiplayer).
	/// </summary>
	internal static decimal GetEnemyHpScalePerChapter(object runState)
	{
		return GetActiveScalingSettings(runState).HpScale;
	}

	/// <summary>
	/// Fetches the authoritative damage scale rate per chapter (reads host run state metadata in multiplayer).
	/// </summary>
	internal static decimal GetEnemyDamageScalePerChapter(object runState)
	{
		return GetActiveScalingSettings(runState).DamageScale;
	}

	/// <summary>
	/// Returns the authoritative endless scaling settings for the run.
	/// In multiplayer, reads host-synchronized metadata to avoid combat math divergence.
	/// </summary>
	public static EndlessScalingSettings GetActiveScalingSettings(object runState)
	{
		int completedLoops = GetCompletedLoops(runState);
		if (EndlessReflection.GetPlayersCount(runState) <= 1)
		{
			return EndlessScalingSettings.FromConfig(completedLoops);
		}
		if (EndlessReflection.TryGetPersistedEndlessSettings(runState, out var hostSettings))
		{
			WarnIfLocalScalingDiffers(hostSettings);
			return hostSettings;
		}
		if (TryGetRunManager(out object runManager) && EndlessReflection.IsLocalPlayerHost(runManager))
		{
			int num = EndlessReflection.GetPersistedEndlessLoopCount(runState) ?? completedLoops;
			EndlessScalingSettings hostConfig = EndlessScalingSettings.FromConfig(num);
			EndlessReflection.SetPersistedEndlessSettings(runState, hostConfig);
			EndlessLog.Info($"Migrated missing multiplayer scaling metadata from host config: loop={num}, interval={hostConfig.Interval}, curve={hostConfig.Curve}, skip_ch={!hostConfig.ScaleChapterOne}, hp_scale={hostConfig.HpScale:0.###}, dmg_scale={hostConfig.DamageScale:0.###}, flat_hp={hostConfig.FlatHp}, flat_dmg={hostConfig.FlatDmg}.");
			return hostConfig;
		}
		WarnOnce("missing-scaling-metadata", "WARNING: Multiplayer endless scaling metadata is missing from the run state. Using local config as fallback.");
		return EndlessScalingSettings.FromConfig(completedLoops);
	}

	private static bool TryGetRunManager(out object runManager)
	{
		try
		{
			runManager = EndlessReflection.GetRunManagerInstance();
			return true;
		}
		catch
		{
			runManager = null;
			return false;
		}
	}

	private static void WarnIfLocalScalingDiffers(EndlessScalingSettings hostSettings)
	{
		bool mismatch = hostSettings.HpScale != EndlessConfig.EnemyHpScalingPerChapter
			|| hostSettings.DamageScale != EndlessConfig.EnemyDamageScalingPerChapter
			|| hostSettings.Interval != EndlessConfig.Interval
			|| hostSettings.Curve != EndlessConfig.Curve
			|| hostSettings.ScaleChapterOne != EndlessConfig.ScaleChapterOne
			|| hostSettings.FlatHp != EndlessConfig.FlatHpPerStep
			|| hostSettings.FlatDmg != EndlessConfig.FlatDmgPerStep;

		if (mismatch)
		{
			WarnOnce($"scaling-mismatch:{hostSettings.LoopCount}:{hostSettings.HpScale:0.###}:{hostSettings.DamageScale:0.###}:{hostSettings.Interval}:{hostSettings.Curve}",
				$"WARNING: Local endless config differs from multiplayer host settings. " +
				$"Using host settings: interval={hostSettings.Interval}, curve={hostSettings.Curve}, skip_ch={!hostSettings.ScaleChapterOne}, hp_scale={hostSettings.HpScale:0.###}, dmg_scale={hostSettings.DamageScale:0.###}, flat_hp={hostSettings.FlatHp}, flat_dmg={hostSettings.FlatDmg}.");
		}
	}

	private static void WarnOnce(string key, string message)
	{
		if (!(_lastScalingWarningKey == key))
		{
			_lastScalingWarningKey = key;
			EndlessLog.Info(message);
		}
	}

	/// <summary>
	/// Intercepts act index increments at end of Act 3 (or final act), resetting index to 0 and triggering new endless loop generation.
	/// </summary>
	/// <param name="runManager">RunManager instance.</param>
	/// <param name="actIndex">Reference to target act index being entered.</param>
	/// <returns>True if endless loop wrapped act index back to 0; otherwise false.</returns>
	public static bool TryWrapActIndex(object runManager, ref int actIndex)
	{
		if (!EndlessConfig.Enabled)
		{
			return false;
		}
		object runState = EndlessReflection.GetRunState(runManager);
		int actsCount = EndlessReflection.GetActsCount(runState);
		if (actIndex < actsCount)
		{
			return false;
		}
		PrepareLoop(runManager, runState);
		actIndex = 0;
		return true;
	}

	/// <summary>
	/// Evaluates whether game completion victory should be intercepted to offer Endless Mode continuation.
	/// </summary>
	public static bool ShouldInterceptWin(object runManager)
	{
		if (!EndlessConfig.Enabled)
		{
			return false;
		}
		object runState = EndlessReflection.GetRunState(runManager);
		int actsCount = EndlessReflection.GetActsCount(runState);
		int currentActIndex = EndlessReflection.GetCurrentActIndex(runState);
		return currentActIndex >= actsCount - 1;
	}

	/// <summary>
	/// Forces the run to immediately advance into the next endless loop and loads Act 1.
	/// </summary>
	public static Task ForceNextLoopAsync(object runManager)
	{
		object runState = EndlessReflection.GetRunState(runManager);
		PrepareLoop(runManager, runState);
		return EndlessReflection.InvokeEnterNextActAsync(runManager);
	}

	/// <summary>
	/// Initiates endless loop regeneration from the Victory / Game Over screen asynchronously.
	/// </summary>
	public static Task StartLoopFromGameOverScreenAsync(Node screen)
	{
		return StartLoopFromGameOverScreenAsync(screen, forceReady: false);
	}

	/// <summary>
	/// Initiates endless loop regeneration from the Victory / Game Over screen asynchronously with ready check override options.
	/// </summary>
	public static async Task StartLoopFromGameOverScreenAsync(Node screen, bool forceReady)
	{
		await StartLoopFromGameOverScreenAsync(screen, forceReady, allowClientRelay: false, broadcastLaunchMarker: true);
	}

	/// <summary>
	/// Reconstructs save state, revives players, clears visited nodes, reseeds RNG, and transitions run into Act 1 of the new endless chapter.
	/// </summary>
	/// <param name="screen">Game over screen Godot node.</param>
	/// <param name="forceReady">Whether to bypass waiting for all multiplayer clients to ready up.</param>
	/// <param name="allowClientRelay">Whether a client is permitted to trigger the loop transition directly.</param>
	/// <param name="broadcastLaunchMarker">Whether to broadcast ready toggle marker to synchronize multiplayer clients.</param>
	public static async Task StartLoopFromGameOverScreenAsync(Node screen, bool forceReady, bool allowClientRelay, bool broadcastLaunchMarker)
	{
		if (IsLaunchingFromGameOverScreen)
		{
			return;
		}
		IsLaunchingFromGameOverScreen = true;
		object runManager = EndlessReflection.GetRunManagerInstance();
		try
		{
			object runState = EndlessReflection.GetRunState(runManager);
			if (!allowClientRelay && !CanLocalPlayerLaunchEndless(runManager, runState))
			{
				EndlessLog.Info("Ignoring endless launch request on a multiplayer client because only the host may start the next chapter.");
				return;
			}
			if (!allowClientRelay && !forceReady && !AreAllPlayersReadyForEndless(runManager, runState))
			{
				EndlessLog.Info("Ignoring endless launch request because not all multiplayer players are marked ready yet.");
				return;
			}
			if (forceReady)
			{
				EndlessLog.Info("Force-starting the next endless chapter without waiting for all multiplayer players to be ready.");
			}
			if (broadcastLaunchMarker && EndlessReflection.GetPlayersCount(runState) > 1 && EndlessReflection.TrySetLocalPlayerReady(runManager, isReady: false))
			{
				EndlessLog.Info("Broadcasted the endless launch marker to multiplayer clients via the shared ready state.");
				await Task.Delay(350);
			}
			SynchronizeLoopCountFromRunState(runState);
			PrepareLoop(runManager, runState);
			object endlessSave = EndlessReflection.CreateSaveSnapshot(runManager);
			PersistEndlessMetadataOnSave(runManager, runState, endlessSave, _loopCount);
			EndlessReflection.SetPreFinishedRoom(endlessSave, null);
			EndlessReflection.SetSerializableRunWinTime(endlessSave, 0L);
			EndlessReflection.ClearSerializableSeenEvents(endlessSave);
			int revivedPlayers = EndlessReflection.ReviveSerializablePlayers(endlessSave);
			object resumedRunState = EndlessReflection.CreateRunStateFromSave(runManager, endlessSave);
			PersistEndlessMetadata(runManager, resumedRunState, _loopCount);
			EndlessReflection.ClearVisitedEventIds(resumedRunState);
			EndlessReflection.RefreshSharedRelicGrabBag(resumedRunState);
			EndlessReflection.SetRunState(runManager, resumedRunState);
			EndlessReflection.InitializeSavedRun(runManager, endlessSave);
			EndlessReflection.PrepareActChangeForFreshRun(runManager, resumedRunState);
			EndlessReflection.ResetEventSynchronizerForFreshRun(runManager, resumedRunState);
			EndlessReflection.PrepareMapSelectionForFreshMap(runManager, resumedRunState);
			EndlessReflection.ResetMultiplayerStateAfterEndlessLoad(runManager);
			EndlessLog.Info("Prepared endless save snapshot and reconstructed a fresh run state.");
			EndlessLog.Info("Reset act-change readiness for the regenerated endless chapter.");
			EndlessLog.Info("Reset event-selection state for the regenerated endless chapter.");
			EndlessLog.Info("Reset map-selection votes for the regenerated act map.");
			EndlessLog.Info("Reset the seen-event history and refreshed the shared relic pool for the new endless chapter.");
			if (revivedPlayers > 0)
			{
				EndlessLog.Info($"Revived {revivedPlayers} player(s) in the endless save snapshot.");
			}
			CanvasItem canvasItem = (CanvasItem)(object)((screen is CanvasItem) ? screen : null);
			if (canvasItem != null)
			{
				canvasItem.Visible = false;
			}
			await ((GodotObject)screen).ToSignal((GodotObject)(object)screen.GetTree(), SceneTree.SignalName.ProcessFrame);
			EndlessReflection.ClearScreens(runManager);
			EndlessLog.Info("Cleared lingering run screens before loading the endless run.");
			screen.QueueFree();
			EndlessLog.Info("Queued the game over screen for disposal.");
			EndlessReflection.QueueFreeLingeringCombatNodes();
			EndlessLog.Info("Loading endless run back into the main game scene.");
			await EndlessReflection.InvokeLoadRunAsync(EndlessReflection.GetGameInstance(), resumedRunState, EndlessReflection.GetPreFinishedRoom(endlessSave));
			EndlessReflection.PrepareActChangeForFreshRun(runManager, resumedRunState);
			EndlessReflection.RefreshMapSelectionForLoadedRun(runManager, resumedRunState);
			EndlessReflection.ResetMultiplayerStateAfterEndlessLoad(runManager);
			EndlessLog.Info("Reset multiplayer action/combat state for the endless map transition.");
			EndlessLog.Info("Endless run load completed.");
			SetPreserveMultiplayerConnectionDuringGameOver(preserve: false, "endless run load completed");
			_awaitingFirstCombatAfterLoopLoad = true;
		}
		finally
		{
			IsLaunchingFromGameOverScreen = false;
		}
	}

	private static void PersistEndlessMetadata(object runManager, object runState, int loopCount)
	{
		if (ShouldWriteAuthoritativeScaling(runManager, runState))
		{
			EndlessScalingSettings settings = EndlessScalingSettings.FromConfig(loopCount);
			EndlessReflection.SetPersistedEndlessSettings(runState, settings);
			EndlessLog.Info($"Persisted endless multiplayer scaling metadata: loop={loopCount}, interval={settings.Interval}, curve={settings.Curve}, skip_ch={!settings.ScaleChapterOne}, hp_scale={settings.HpScale:0.###}, dmg_scale={settings.DamageScale:0.###}, flat_hp={settings.FlatHp}, flat_dmg={settings.FlatDmg}.");
		}
		else if (EndlessReflection.TryGetPersistedEndlessSettings(runState, out var hostSettings))
		{
			hostSettings.LoopCount = loopCount;
			EndlessReflection.SetPersistedEndlessSettings(runState, hostSettings);
			EndlessLog.Info($"Preserved host endless scaling metadata on multiplayer client: loop={loopCount}, hp_scale={hostSettings.HpScale:0.###}, dmg_scale={hostSettings.DamageScale:0.###}.");
		}
		else
		{
			EndlessScalingSettings fallback = EndlessScalingSettings.FromConfig(loopCount);
			EndlessReflection.SetPersistedEndlessSettings(runState, fallback);
			EndlessLog.Info($"Initialized client endless metadata fallback: loop={loopCount}, hp_scale={fallback.HpScale:0.###}, dmg_scale={fallback.DamageScale:0.###}.");
		}
	}

	private static void PersistEndlessMetadataOnSave(object runManager, object runState, object save, int loopCount)
	{
		if (ShouldWriteAuthoritativeScaling(runManager, runState))
		{
			EndlessScalingSettings settings = EndlessScalingSettings.FromConfig(loopCount);
			EndlessReflection.SetPersistedEndlessSettingsOnSave(save, settings);
		}
		else if (EndlessReflection.TryGetPersistedEndlessSettings(runState, out var hostSettings))
		{
			hostSettings.LoopCount = loopCount;
			EndlessReflection.SetPersistedEndlessSettingsOnSave(save, hostSettings);
		}
		else
		{
			EndlessReflection.SetPersistedEndlessLoopCountOnSave(save, loopCount);
		}
	}

	private static bool ShouldWriteAuthoritativeScaling(object runManager, object runState)
	{
		return EndlessReflection.GetPlayersCount(runState) <= 1 || EndlessReflection.IsLocalPlayerHost(runManager);
	}

	/// <summary>
	/// Prepares the run state for a new endless loop: increments loop count, calculates ascension escalation, regenerates acts, randomizes bosses, reseeds deterministic RNG pools, and resets act maps.
	/// </summary>
	private static void PrepareLoop(object runManager, object runState)
	{
		SynchronizeLoopCountFromRunState(runState);
		string text = $"{EndlessReflection.GetCurrentActIndex(runState)}:{EndlessReflection.GetTotalFloor(runState)}:{_loopCount}:{EndlessReflection.GetAscensionLevel(runState)}";
		if (!(text == _lastTransitionKey))
		{
			int num = _loopCount + 1;
			int ascensionLevel = EndlessReflection.GetAscensionLevel(runState);
			int value = (EndlessConfig.AscensionPerLoop >= 0)
				? Math.Clamp(num * EndlessConfig.AscensionPerLoop, 0, EndlessConfig.MaxAscension)
				: Math.Min(ascensionLevel + 1, 10);
			string text2 = $"{EndlessReflection.GetRunSeedString(runState)}|endless-loop|{num}|{text}";
			EndlessReflection.ReseedUnknownMapPointRng(runState, GetDeterministicSeed(text2, 97));
			string[] array = ReseedRewardAndEventRngs(runState, text2);
			string[] array2 = ReseedPlayerRngs(runState, text2);
			IReadOnlyList<ActModel> randomizedActsForLoop = EndlessReflection.GetRandomizedActsForLoop(runState, text2);
			EndlessReflection.SetActs(runState, randomizedActsForLoop);
			string[] array3 = RandomizeActBosses(runState, text, num);
			string[] array4 = DescribeActAncients(runState);
			EndlessReflection.SetAscensionLevel(runState, value);
			EndlessReflection.SetCurrentActIndex(runState, 0);
			EndlessReflection.SetActFloor(runState, 0);
			EndlessReflection.SetMap(runState, EndlessReflection.CreateFreshActMap(runState));
			EndlessReflection.ClearVisitedMapCoords(runState);
			EndlessReflection.ResetSavedMapsToLoad(runManager);
			EndlessReflection.ClearMapDrawingsToLoad(runManager);
			EndlessReflection.ResetWinTime(runManager);
			EndlessReflection.SetShouldSave(runManager, value: true);
			EndlessReflection.SetIsCleaningUp(runManager, value: false);
			EndlessReflection.SetIsAbandoned(runManager, value: false);
			_lastTransitionKey = text;
			_loopCount = num;
			PersistEndlessMetadata(runManager, runState, _loopCount);
			EndlessLog.Info($"Loop {_loopCount}: wrapping back to Act 1, regenerating the map, and raising Ascension {ascensionLevel} -> {value}.");
			EndlessLog.Info($"Rerolled the act lineup for loop {_loopCount} using seed '{text2}'.");
			if (array3.Length != 0)
			{
				EndlessLog.Info("Randomized act bosses: " + string.Join("; ", array3) + ".");
			}
			if (array4.Length != 0)
			{
				EndlessLog.Info("Rolled act ancients: " + string.Join("; ", array4) + ".");
			}
			if (array.Length != 0)
			{
				EndlessLog.Info("Reseeded reward RNGs for the new endless chapter: " + string.Join(", ", array) + ".");
			}
			if (array2.Length != 0)
			{
				EndlessLog.Info("Reinitialized player RNG seeds for the new endless chapter: " + string.Join(", ", array2) + ".");
			}
		}
	}

	private static string[] RandomizeActBosses(object runState, string transitionKey, int loopCount)
	{
		List<ActModel> acts = EndlessReflection.GetActs(runState);
		if (acts.Count == 0)
		{
			return Array.Empty<string>();
		}
		List<ActModel> list = new List<ActModel>(acts.Count);
		List<string> list2 = new List<string>();
		for (int i = 0; i < acts.Count; i++)
		{
			ActModel val = EndlessReflection.CreateMutableAct(acts[i]);
			List<EncounterModel> allBossEncounters = EndlessReflection.GetAllBossEncounters(val);
			if (allBossEncounters.Count == 0)
			{
				EndlessReflection.GenerateRoomsForAct(val, runState);
				list.Add(val);
				continue;
			}
			EncounterModel bossEncounter = EndlessReflection.GetBossEncounter(val);
			EncounterModel val2 = PickBossEncounter(allBossEncounters, bossEncounter, transitionKey, loopCount, i, 0);
			EndlessReflection.SetBossEncounter(val, val2);
			string text = $"Act {i + 1} {FormatBossChange(bossEncounter, val2)}";
			if (EndlessReflection.HasSecondBoss(val))
			{
				EncounterModel secondBossEncounter = EndlessReflection.GetSecondBossEncounter(val);
				EncounterModel val3 = PickBossEncounter(allBossEncounters, secondBossEncounter, transitionKey, loopCount, i, 1, val2);
				EndlessReflection.SetSecondBossEncounter(val, val3);
				text = text + " / second " + FormatBossChange(secondBossEncounter, val3);
			}
			EndlessReflection.GenerateRoomsForAct(val, runState);
			list.Add(val);
			list2.Add(text);
		}
		EndlessReflection.SetActs(runState, list);
		return list2.ToArray();
	}

	private static EncounterModel PickBossEncounter(IReadOnlyList<EncounterModel> bossPool, EncounterModel? currentBoss, string transitionKey, int loopCount, int actIndex, int salt, EncounterModel? disallowed = null)
	{
		List<EncounterModel> list = bossPool.Where((EncounterModel encounter) => encounter != disallowed).ToList();
		if (list.Count == 0)
		{
			list = bossPool.ToList();
		}
		int num = GetDeterministicIndex(transitionKey, loopCount, actIndex, salt, list.Count);
		if (list.Count > 1 && currentBoss != null && list[num] == currentBoss)
		{
			num = (num + 1) % list.Count;
		}
		return list[num];
	}

	private static int GetDeterministicIndex(string transitionKey, int loopCount, int actIndex, int salt, int count)
	{
		ulong num = ComputeDeterministicHash(transitionKey, loopCount, actIndex, salt);
		return (int)(num % (ulong)count);
	}

	private static uint GetDeterministicSeed(string key, int salt)
	{
		return (uint)(ComputeDeterministicHash(key, 0, 0, salt) & 0xFFFFFFFFu);
	}

	private static ulong ComputeDeterministicHash(string key, int loopCount, int actIndex, int salt)
	{
		ulong num = 14695981039346656037uL;
		foreach (char c in key)
		{
			num ^= c;
			num *= 1099511628211L;
		}
		num ^= (uint)loopCount;
		num *= 1099511628211L;
		num ^= (uint)((actIndex + 1) * 131 + salt * 17);
		return num * 1099511628211L;
	}

	private static string FormatBossChange(EncounterModel? previousBoss, EncounterModel nextBoss)
	{
		string text = ((previousBoss == null) ? "<none>" : EndlessReflection.GetModelId(previousBoss));
		string modelId = EndlessReflection.GetModelId(nextBoss);
		return text + "->" + modelId;
	}

	private static string[] DescribeActAncients(object runState)
	{
		List<ActModel> acts = EndlessReflection.GetActs(runState);
		if (acts.Count == 0)
		{
			return Array.Empty<string>();
		}
		return acts.Select((ActModel act, int index) =>
		{
			object ancient = EndlessReflection.GetAncient(act);
			string value = ((ancient == null) ? "<none>" : EndlessReflection.GetModelId(ancient));
			return $"Act {index + 1} {value}";
		}).ToArray();
	}

	private static string[] ReseedRewardAndEventRngs(object runState, string loopSeed)
	{
		(string, int)[] array = new (string, int)[3]
		{
			("UpFront", 211),
			("Niche", 223),
			("TreasureRoomRelics", 229)
		};
		List<string> list = new List<string>();
		(string, int)[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			var (text, salt) = array2[i];
			if (EndlessReflection.TryReseedRunRng(runState, text, GetDeterministicSeed(loopSeed, salt)))
			{
				list.Add(text);
			}
		}
		return list.ToArray();
	}

	private static string[] ReseedPlayerRngs(object runState, string loopSeed)
	{
		IReadOnlyList<object> runStatePlayers = EndlessReflection.GetRunStatePlayers(runState);
		if (runStatePlayers.Count == 0)
		{
			return Array.Empty<string>();
		}
		List<string> list = new List<string>(runStatePlayers.Count);
		for (int i = 0; i < runStatePlayers.Count; i++)
		{
			string seed = $"{loopSeed}|player|{i}";
			EndlessReflection.InitializePlayerSeed(runStatePlayers[i], seed);
			list.Add($"player{i + 1}");
		}
		return list.ToArray();
	}

	/// <summary>
	/// Synchronizes local in-memory loop count from serialized run state metadata or floor count heuristics.
	/// Guards against underflow when transitioning from Act 3 final boss room to Act 1.
	/// </summary>
	private static void SynchronizeLoopCountFromRunState(object runState)
	{
		int? persistedEndlessLoopCount = EndlessReflection.GetPersistedEndlessLoopCount(runState);
		if (persistedEndlessLoopCount.HasValue)
		{
			int valueOrDefault = persistedEndlessLoopCount.GetValueOrDefault();
			if (true)
			{
				_loopCount = Math.Max(_loopCount, valueOrDefault);
				return;
			}
		}
		int num = 0;
		int cycleFloorCount = EndlessReflection.GetCycleFloorCount(runState);
		if (cycleFloorCount > 0)
		{
			int totalFloor = EndlessReflection.GetTotalFloor(runState);
			int currentActIndex = EndlessReflection.GetCurrentActIndex(runState);
			// Underflow defense: when boss of act 3 is defeated (or act 1 begins on loop 1+),
			// totalFloor can be 47 (boss node) while cycleFloorCount is 48.
			// (47 + 3) / 48 correctly resolves to loop 1 instead of 0.
			num = ((currentActIndex <= 0) ? ((totalFloor + 3) / cycleFloorCount) : Math.Max(0, (totalFloor - 1) / cycleFloorCount));
			if (EndlessReflection.IsGameOver(runState) && currentActIndex > 0 && num > 0)
			{
				num--;
			}
		}
		_loopCount = Math.Max(_loopCount, num);
	}

	/// <summary>
	/// Consumes one-shot flag requesting action queue and creature reset on the first combat room entered after an endless loop load.
	/// </summary>
	public static bool TryConsumeFirstCombatResetRequest()
	{
		if (!_awaitingFirstCombatAfterLoopLoad)
		{
			return false;
		}
		_awaitingFirstCombatAfterLoopLoad = false;
		return true;
	}
}
