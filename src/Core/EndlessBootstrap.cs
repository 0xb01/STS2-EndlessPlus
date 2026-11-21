using System;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Runs;

namespace BNKZ.STS2.EndlessPlus;

[ModInitializer("Initialize")]
internal static class EndlessBootstrap
{
	private const string ModVersion = "1.0.3";

	private static readonly Harmony Harmony = new Harmony("BNKZ.STS2.EndlessPlus");

	private static readonly Type NButtonType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.GodotExtensions.NButton") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Nodes.GodotExtensions.NButton");

	private static readonly Type NGameType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.NGame") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Nodes.NGame");

	private static readonly Type GodotFileIoType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Saves.GodotFileIo") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Saves.GodotFileIo");

	private static readonly Type? CloudSaveStoreType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Saves.CloudSaveStore");

	private static readonly Type? SaveManagerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Saves.SaveManager");

	private static readonly Type? NetHostGameServiceType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Multiplayer.NetHostGameService");

	private static readonly Type? NetClientGameServiceType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Multiplayer.NetClientGameService");

	private static readonly Type NRunType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.NRun") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Nodes.NRun");

	private static readonly Type CombatRoomType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Rooms.CombatRoom") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Rooms.CombatRoom");

	private static readonly Type NIntentType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Combat.NIntent") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Nodes.Combat.NIntent");

	private static readonly Type IRunStateType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Runs.IRunState") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Runs.IRunState");

	private static readonly Type NeowType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Models.Events.Neow") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Models.Events.Neow");

	private static bool _initialized;

	/// <summary>
	/// Main mod initialization entry point.
	/// Loads configuration, registers BaseLib Mod Configuration UI, and applies Harmony patches to Slay the Spire 2 core assemblies.
	/// </summary>
	public static void Initialize()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected Obj, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected Obj, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected Obj, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Expected Obj, but got Unknown
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Expected Obj, but got Unknown
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Expected Obj, but got Unknown
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Expected Obj, but got Unknown
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Expected Obj, but got Unknown
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Expected Obj, but got Unknown
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Expected Obj, but got Unknown
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Expected Obj, but got Unknown
		//IL_04f5: Expected Obj, but got Unknown
		//IL_051d: Unknown result type (might be due to invalid IL or missing references)
		//IL_052c: Expected Obj, but got Unknown
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_0561: Unknown result type (might be due to invalid IL or missing references)
		//IL_056b: Expected Obj, but got Unknown
		//IL_056b: Expected Obj, but got Unknown
		//IL_0586: Unknown result type (might be due to invalid IL or missing references)
		//IL_0596: Expected Obj, but got Unknown
		//IL_05b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c1: Expected Obj, but got Unknown
		//IL_05dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Expected Obj, but got Unknown
		//IL_0608: Unknown result type (might be due to invalid IL or missing references)
		//IL_0617: Expected Obj, but got Unknown
		//IL_0455: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Expected Obj, but got Unknown
		//IL_064d: Unknown result type (might be due to invalid IL or missing references)
		//IL_065c: Expected Obj, but got Unknown
		//IL_0677: Unknown result type (might be due to invalid IL or missing references)
		//IL_0687: Expected Obj, but got Unknown
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_06be: Expected Obj, but got Unknown
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Expected Obj, but got Unknown
		//IL_037c: Expected Obj, but got Unknown
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Expected Obj, but got Unknown
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Expected Obj, but got Unknown
		//IL_03d1: Expected Obj, but got Unknown
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		try
		{
			EndlessLog.Info($"Initializing BNKZ EndlessPlus v{ModVersion}.");
			EndlessConfig.Load();
			BaseLibIntegration.TryRegister();
			LinuxNativeBootstrap.PrepareForHarmony();
			int attemptedPatches = 0;
			int successfulPatches = 0;
			bool flag = OS.GetName().Equals("Android", StringComparison.OrdinalIgnoreCase);
			TryPatch("LocManager.Initialize", AccessTools.Method(typeof(MegaCrit.Sts2.Core.Localization.LocManager), "Initialize", (Type[])null, (Type[])null), null, new HarmonyMethod(typeof(BaseLibIntegration), nameof(BaseLibIntegration.OnLocManagerInitialize), (Type[])null), ref attemptedPatches, ref successfulPatches);
			TryPatch("RunManager.SetActInternal", EndlessReflection.RunManagerSetActInternal, new HarmonyMethod(typeof(RunManagerPatches), "BeforeSetActInternal", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
			TryPatch("RunManager.Launch", AccessTools.Method(typeof(RunManager), "Launch", (Type[])null, (Type[])null), null, new HarmonyMethod(typeof(RunManagerPatches), "AfterLaunch", (Type[])null), ref attemptedPatches, ref successfulPatches);
			TryPatch("RunManager.SetUpSavedMultiplayer", AccessTools.Method(typeof(RunManager), "SetUpSavedMultiplayer", (Type[])null, (Type[])null), null, new HarmonyMethod(typeof(GameLifecyclePatches), "AfterSetUpSavedMultiplayer", (Type[])null), ref attemptedPatches, ref successfulPatches);
			TryPatch("RunManager.GetRejoinMessage", AccessTools.Method(typeof(RunManager), "GetRejoinMessage", (Type[])null, (Type[])null), new HarmonyMethod(typeof(GameLifecyclePatches), "BeforeGetRejoinMessage", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
			TryPatch("RunManager.ToSave", AccessTools.Method(typeof(RunManager), "ToSave", (Type[])null, (Type[])null), null, new HarmonyMethod(typeof(GameLifecyclePatches), "AfterToSave", (Type[])null), ref attemptedPatches, ref successfulPatches);
			TryPatch("NButton.OnPress", AccessTools.Method(NButtonType, "OnPress", (Type[])null, (Type[])null), new HarmonyMethod(typeof(GameOverScreenPatches), "BeforeAnyButtonPress", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
			TryPatch("NGame.Quit", AccessTools.Method(NGameType, "Quit", (Type[])null, (Type[])null), new HarmonyMethod(typeof(GameLifecyclePatches), "BeforeQuit", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
			if (flag)
			{
				EndlessLog.Info("Android compatibility mode: skipping GodotFileIo.DeleteFile(string) cloud-save protection patch.");
			}
			else
			{
				TryPatch("GodotFileIo.DeleteFile(string)", AccessTools.Method(GodotFileIoType, "DeleteFile", new Type[1] { typeof(string) }, (Type[])null), new HarmonyMethod(typeof(GameLifecyclePatches), "BeforeDeleteFile", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
				foreach (MethodInfo item in from method in AccessTools.GetDeclaredMethods(GodotFileIoType)
					where string.Equals(method.Name, "WriteFile", StringComparison.Ordinal)
					select method)
				{
					ParameterInfo[] parameters = item.GetParameters();
					if (parameters.Length != 0 && !(parameters[0].ParameterType != typeof(string)))
					{
						string text = string.Join(", ", parameters.Select((ParameterInfo parameter) => parameter.ParameterType.Name));
						TryPatch("GodotFileIo.WriteFile(" + text + ")", item, new HarmonyMethod(typeof(GameLifecyclePatches), "BeforeWriteFile", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
					}
				}
				TryPatch("CloudSaveStore.SyncCloudToLocalInternal", ((object)CloudSaveStoreType == null) ? null : AccessTools.Method(CloudSaveStoreType, "SyncCloudToLocalInternal", (Type[])null, (Type[])null), new HarmonyMethod(typeof(GameLifecyclePatches), "BeforeSyncCloudToLocal", (Type[])null), null, ref attemptedPatches, ref successfulPatches, new HarmonyMethod(typeof(GameLifecyclePatches), "FinalizeSyncCloudToLocal", (Type[])null));
				TryPatch("SaveManager.SyncCloudToLocal", ((object)SaveManagerType == null) ? null : AccessTools.Method(SaveManagerType, "SyncCloudToLocal", (Type[])null, (Type[])null), new HarmonyMethod(typeof(GameLifecyclePatches), "BeforeSyncCloudToLocal", (Type[])null), null, ref attemptedPatches, ref successfulPatches, new HarmonyMethod(typeof(GameLifecyclePatches), "FinalizeSyncCloudToLocal", (Type[])null));
			}
			if ((object)NetHostGameServiceType != null)
			{
				TryPatch("NetHostGameService.Disconnect", AccessTools.Method(NetHostGameServiceType, "Disconnect", (Type[])null, (Type[])null), new HarmonyMethod(typeof(GameLifecyclePatches), "BeforeNetGameDisconnect", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
			}
			if ((object)NetClientGameServiceType != null)
			{
				TryPatch("NetClientGameService.Disconnect", AccessTools.Method(NetClientGameServiceType, "Disconnect", (Type[])null, (Type[])null), new HarmonyMethod(typeof(GameLifecyclePatches), "BeforeNetGameDisconnect", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
			}
			TryPatch("StartRunLobby.IsAboutToBeginGame", AccessTools.Method(typeof(MegaCrit.Sts2.Core.Multiplayer.Game.Lobby.StartRunLobby), "IsAboutToBeginGame"), new HarmonyMethod(typeof(LobbyPatches), "BeforeIsAboutToBeginGame"), null, ref attemptedPatches, ref successfulPatches);
			TryPatch("NRun._Ready", AccessTools.Method(NRunType, "_Ready", (Type[])null, (Type[])null), null, new HarmonyMethod(typeof(RunUiPatches), "AfterReady", (Type[])null), ref attemptedPatches, ref successfulPatches);
			TryPatch("CombatRoom.StartCombat(IRunState)", AccessTools.Method(CombatRoomType, "StartCombat", new Type[1] { IRunStateType }, (Type[])null), new HarmonyMethod(typeof(CombatRoomPatches), "BeforeStartCombat", (Type[])null), null, ref attemptedPatches, ref successfulPatches, new HarmonyMethod(typeof(CombatRoomPatches), "FinalizeStartCombat", (Type[])null));
			TryPatch("Neow.GenerateInitialOptions", AccessTools.Method(NeowType, "GenerateInitialOptions", (Type[])null, (Type[])null), null, new HarmonyMethod(typeof(NeowPatches), "AfterGenerateInitialOptions", (Type[])null), ref attemptedPatches, ref successfulPatches);
			TryPatch("EventSynchronizer.BeginEvent", EndlessReflection.EventSynchronizerBeginEvent, new HarmonyMethod(typeof(EventSynchronizerPatches), "BeforeBeginEvent", (Type[])null), null, ref attemptedPatches, ref successfulPatches, new HarmonyMethod(typeof(EventSynchronizerPatches), "FinalizeBeginEvent", (Type[])null));
			TryPatch("MapSelectionSynchronizer.GetVote", EndlessReflection.MapSelectionSynchronizerGetVote, new HarmonyMethod(typeof(MapSelectionSynchronizerPatches), "BeforeGetVote", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
			TryPatch("MapSelectionSynchronizer.PlayerVotedForMapCoord", EndlessReflection.MapSelectionSynchronizerPlayerVotedForMapCoord, new HarmonyMethod(typeof(MapSelectionSynchronizerPatches), "BeforePlayerVotedForMapCoord", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
			TryPatch("RewardsSetSynchronizer.GetRewardStateForPlayer", EndlessReflection.RewardsSetSynchronizerGetRewardStateForPlayer, new HarmonyMethod(typeof(RewardsSetSynchronizerPatches), "BeforeGetRewardStateForPlayer", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
			TryPatch("NRelicInventory.AnimateRelic", EndlessReflection.NRelicInventoryAnimateRelic, new HarmonyMethod(typeof(RelicInventoryPatches), "BeforeAnimateRelic", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
			TryPatch("CombatState.CreateCreature", EndlessReflection.CombatStateCreateCreature, null, new HarmonyMethod(typeof(CombatScalingPatches), "AfterCreateCreature", (Type[])null), ref attemptedPatches, ref successfulPatches);
			TryPatch("CombatState.AddCreature", EndlessReflection.CombatStateAddCreature, new HarmonyMethod(typeof(CombatScalingPatches), "BeforeAddCreature", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
			if (flag)
			{
				EndlessLog.Info("Android compatibility mode: skipping high-risk combat scaling patches for Creature.SetUniqueMonsterHpValue, AttackCommand.Execute, and NIntent.UpdateIntent.");
			}
			else
			{
				TryPatch("Creature.SetUniqueMonsterHpValue", EndlessReflection.CreatureSetUniqueMonsterHpValue, null, new HarmonyMethod(typeof(CombatScalingPatches), "AfterSetUniqueMonsterHpValue", (Type[])null), ref attemptedPatches, ref successfulPatches);
				TryPatch("Creature.ScaleMonsterHpForMultiplayer", EndlessReflection.CreatureScaleMonsterHpForMultiplayer, null, new HarmonyMethod(typeof(CombatScalingPatches), "AfterScaleMonsterHpForMultiplayer", (Type[])null), ref attemptedPatches, ref successfulPatches);
				TryPatch("AttackCommand.Execute", EndlessReflection.AttackCommandExecute, new HarmonyMethod(typeof(CombatScalingPatches), "BeforeAttackExecute", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
				TryPatch("NIntent.UpdateIntent", AccessTools.Method(NIntentType, "UpdateIntent", (Type[])null, (Type[])null), new HarmonyMethod(typeof(CombatScalingPatches), "BeforeUpdateIntent", (Type[])null), null, ref attemptedPatches, ref successfulPatches);
			}
			EndlessLog.Info($"Finished endless patch bootstrap: {successfulPatches}/{attemptedPatches} patches applied.");
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed to initialize endless mode patches.", ex);
		}
	}

	private static void TryPatch(string label, MethodBase? original, HarmonyMethod? prefix, HarmonyMethod? postfix, ref int attemptedPatches, ref int successfulPatches, HarmonyMethod? finalizer = null)
	{
		attemptedPatches++;
		if ((object)original == null)
		{
			EndlessLog.Error("Skipped patch '" + label + "' because the target method could not be found.", new MissingMethodException(label));
			return;
		}
		try
		{
			Harmony.Patch(original, prefix, postfix, (HarmonyMethod)null, finalizer);
			successfulPatches++;
			EndlessLog.Info($"Applied patch '{label}' to {original.DeclaringType?.FullName}.{original.Name}.");
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed to apply patch '" + label + "'.", ex);
		}
	}

	static EndlessBootstrap()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected Obj, but got Unknown
	}
}
