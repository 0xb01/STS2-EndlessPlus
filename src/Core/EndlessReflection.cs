using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace BNKZ.STS2.EndlessPlus;

internal static class EndlessReflection
{
	private const int EndlessLoopCountExtraFieldSentinel = -10000000;

	private const int EndlessMetadataV2MaxValue = -1073741825;

	private const int EndlessMetadataV2ScaleBucketSize = 4096;

	private const int EndlessMetadataV2ScaleMask = 4095;

	private const int EndlessMetadataV2LoopShift = 24;

	private const int EndlessMetadataV2HpShift = 12;

	private const int EndlessMetadataV2MaxLoopCount = 63;

	private const int EndlessMetadataV2PreciseScaleMaxCode = 2047;

	private const int EndlessMetadataV2CoarseScaleBaseCode = 2048;

	private const decimal EndlessMetadataV2PreciseScaleMax = 20.47m;

	private static readonly Type UnlockStateType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Unlocks.UnlockState") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Unlocks.UnlockState");

	private static readonly Type RngType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Random.Rng") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Random.Rng");

	private static readonly Type RunManagerType = typeof(RunManager);

	private static readonly Type RunStateType = typeof(RunState);

	private static readonly Type StandardActMapType = typeof(StandardActMap);

	private static readonly Type PlayerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Entities.Players.Player") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Entities.Players.Player");

	private static readonly Type RelicModelType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Models.RelicModel") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Models.RelicModel");

	private static readonly Type EncounterModelType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Models.EncounterModel") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Models.EncounterModel");

	private static readonly Type AbstractModelType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Models.AbstractModel") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Models.AbstractModel");

	private static readonly Type ActModelType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Models.ActModel") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Models.ActModel");

	private static readonly Type RunRngTypeType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Entities.Rngs.RunRngType") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Entities.Rngs.RunRngType");

	private static readonly Type RelicGrabBagType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Runs.RelicGrabBag") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Runs.RelicGrabBag");

	private static readonly Type CreatureType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Entities.Creatures.Creature") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Entities.Creatures.Creature");

	private static readonly Type CombatStateType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Combat.CombatState") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Combat.CombatState");

	private static readonly Type CombatManagerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Combat.CombatManager") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Combat.CombatManager");

	private static readonly Type AttackCommandType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Commands.Builders.AttackCommand");

	private static readonly Type AttackIntentType = AccessTools.TypeByName("MegaCrit.Sts2.Core.MonsterMoves.Intents.AttackIntent") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.MonsterMoves.Intents.AttackIntent");

	private static readonly Type ActionSynchronizerCombatStateType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Entities.Multiplayer.ActionSynchronizerCombatState") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Entities.Multiplayer.ActionSynchronizerCombatState");

	private static readonly Type NGameType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.NGame") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Nodes.NGame");

	private static readonly Type SerializableRunType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Saves.SerializableRun") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Saves.SerializableRun");

	private static readonly Type ExtraRunFieldsType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Runs.ExtraRunFields") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Runs.ExtraRunFields");

	private static readonly Type SerializableExtraRunFieldsType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Saves.Runs.SerializableExtraRunFields") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Saves.Runs.SerializableExtraRunFields");

	private static readonly Type MapSelectionSynchronizerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Multiplayer.Game.MapSelectionSynchronizer") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Multiplayer.Game.MapSelectionSynchronizer");

	private static readonly Type ActChangeSynchronizerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Multiplayer.Game.ActChangeSynchronizer") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Multiplayer.Game.ActChangeSynchronizer");

	private static readonly Type EventSynchronizerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Multiplayer.Game.EventSynchronizer") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Multiplayer.Game.EventSynchronizer");

	private static readonly Type RewardSynchronizerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Multiplayer.Game.RewardSynchronizer") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Multiplayer.Game.RewardSynchronizer");

	private static readonly Type TreasureRoomRelicSynchronizerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Multiplayer.Game.TreasureRoomRelicSynchronizer") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Multiplayer.Game.TreasureRoomRelicSynchronizer");

	private static readonly Type PlayerChoiceSynchronizerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceSynchronizer") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceSynchronizer");

	private static readonly Type RewardsSetSynchronizerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Multiplayer.Game.RewardsSetSynchronizer") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Multiplayer.Game.RewardsSetSynchronizer");

	private static readonly Type? RestSiteSynchronizerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Multiplayer.Game.RestSiteSynchronizer");

	private static readonly Type? OneOffSynchronizerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Multiplayer.Game.OneOffSynchronizer");

	private static readonly Type NRelicInventoryType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Relics.NRelicInventory") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Nodes.Relics.NRelicInventory");

	private static readonly Type NRelicInventoryHolderType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Relics.NRelicInventoryHolder") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Nodes.Relics.NRelicInventoryHolder");

	private static readonly PropertyInfo RunManagerInstance = AccessTools.Property(RunManagerType, "Instance") ?? throw new MissingMemberException(RunManagerType.FullName, "Instance");

	private static readonly PropertyInfo NGameInstance = AccessTools.Property(NGameType, "Instance") ?? throw new MissingMemberException(NGameType.FullName, "Instance");

	private static readonly PropertyInfo CombatManagerInstance = AccessTools.Property(CombatManagerType, "Instance") ?? throw new MissingMemberException(CombatManagerType.FullName, "Instance");

	private static readonly PropertyInfo CombatManagerIsInProgress = AccessTools.Property(CombatManagerType, "IsInProgress") ?? throw new MissingMemberException(CombatManagerType.FullName, "IsInProgress");

	private static readonly PropertyInfo CombatManagerIsPaused = AccessTools.Property(CombatManagerType, "IsPaused") ?? throw new MissingMemberException(CombatManagerType.FullName, "IsPaused");

	private static readonly PropertyInfo? CombatManagerIsPlayPhase = AccessTools.Property(CombatManagerType, "IsPlayPhase");

	private static readonly PropertyInfo CombatManagerIsEnemyTurnStarted = AccessTools.Property(CombatManagerType, "IsEnemyTurnStarted") ?? throw new MissingMemberException(CombatManagerType.FullName, "IsEnemyTurnStarted");

	private static readonly PropertyInfo CombatManagerEndingPlayerTurnPhaseOne = AccessTools.Property(CombatManagerType, "EndingPlayerTurnPhaseOne") ?? throw new MissingMemberException(CombatManagerType.FullName, "EndingPlayerTurnPhaseOne");

	private static readonly PropertyInfo CombatManagerEndingPlayerTurnPhaseTwo = AccessTools.Property(CombatManagerType, "EndingPlayerTurnPhaseTwo") ?? throw new MissingMemberException(CombatManagerType.FullName, "EndingPlayerTurnPhaseTwo");

	private static readonly PropertyInfo CombatManagerPlayerActionsDisabled = AccessTools.Property(CombatManagerType, "PlayerActionsDisabled") ?? throw new MissingMemberException(CombatManagerType.FullName, "PlayerActionsDisabled");

	public static readonly MethodInfo MapSelectionSynchronizerGetVote = AccessTools.Method(MapSelectionSynchronizerType, "GetVote", new Type[1] { PlayerType }, (Type[])null) ?? throw new MissingMethodException(MapSelectionSynchronizerType.FullName, "GetVote");

	public static readonly MethodInfo MapSelectionSynchronizerPlayerVotedForMapCoord = AccessTools.Method(MapSelectionSynchronizerType, "PlayerVotedForMapCoord", (Type[])null, (Type[])null) ?? throw new MissingMethodException(MapSelectionSynchronizerType.FullName, "PlayerVotedForMapCoord");

	public static readonly MethodInfo EventSynchronizerBeginEvent = AccessTools.Method(EventSynchronizerType, "BeginEvent", (Type[])null, (Type[])null) ?? throw new MissingMethodException(EventSynchronizerType.FullName, "BeginEvent");

	public static readonly MethodInfo NRelicInventoryAnimateRelic = AccessTools.Method(NRelicInventoryType, "AnimateRelic", new Type[3]
	{
		RelicModelType,
		typeof(Godot.Vector2?),
		typeof(Godot.Vector2?)
	}, (Type[])null) ?? throw new MissingMethodException(NRelicInventoryType.FullName, "AnimateRelic");

	public static readonly MethodInfo CombatStateCreateCreature = AccessTools.Method(CombatStateType, "CreateCreature", (Type[])null, (Type[])null) ?? throw new MissingMethodException(CombatStateType.FullName, "CreateCreature");

	public static readonly MethodInfo CombatStateAddCreature = AccessTools.Method(CombatStateType, "AddCreature", new Type[1] { CreatureType }, (Type[])null) ?? throw new MissingMethodException(CombatStateType.FullName, "AddCreature");

	public static readonly MethodInfo CreatureScaleMonsterHpForMultiplayer = AccessTools.Method(CreatureType, "ScaleMonsterHpForMultiplayer", (Type[])null, (Type[])null) ?? throw new MissingMethodException(CreatureType.FullName, "ScaleMonsterHpForMultiplayer");

	public static readonly MethodInfo RewardsSetSynchronizerGetRewardStateForPlayer = AccessTools.Method(RewardsSetSynchronizerType, "GetRewardStateForPlayer", new Type[1] { PlayerType }, (Type[])null) ?? throw new MissingMethodException(RewardsSetSynchronizerType.FullName, "GetRewardStateForPlayer");

	public static readonly MethodInfo CreatureSetUniqueMonsterHpValue = AccessTools.Method(CreatureType, "SetUniqueMonsterHpValue", (Type[])null, (Type[])null) ?? throw new MissingMethodException(CreatureType.FullName, "SetUniqueMonsterHpValue");

	public static readonly MethodInfo AttackCommandExecute = AccessTools.Method(AttackCommandType, "Execute", (Type[])null, (Type[])null) ?? throw new MissingMethodException(AttackCommandType.FullName, "Execute");

	public static readonly MethodInfo CombatManagerReset = AccessTools.Method(CombatManagerType, "Reset", new Type[1] { typeof(bool) }, (Type[])null) ?? throw new MissingMethodException(CombatManagerType.FullName, "Reset");

	public static readonly MethodInfo RunManagerSetActInternal = AccessTools.Method(RunManagerType, "SetActInternal", new Type[1] { typeof(int) }, (Type[])null) ?? throw new MissingMethodException(RunManagerType.FullName, "SetActInternal");

	public static readonly MethodInfo RunManagerWinRun = AccessTools.Method(RunManagerType, "WinRun", (Type[])null, (Type[])null) ?? throw new MissingMethodException(RunManagerType.FullName, "WinRun");

	public static readonly MethodInfo RunManagerEnterNextAct = AccessTools.Method(RunManagerType, "EnterNextAct", (Type[])null, (Type[])null) ?? throw new MissingMethodException(RunManagerType.FullName, "EnterNextAct");

	private static readonly MethodInfo RunManagerToSave = AccessTools.Method(RunManagerType, "ToSave", (Type[])null, (Type[])null) ?? throw new MissingMethodException(RunManagerType.FullName, "ToSave");

	private static readonly MethodInfo RunStateFromSerializable = AccessTools.Method(RunStateType, "FromSerializable", new Type[1] { SerializableRunType }, (Type[])null) ?? throw new MissingMethodException(RunStateType.FullName, "FromSerializable");

	private static readonly MethodInfo RunManagerInitializeSavedRun = AccessTools.Method(RunManagerType, "InitializeSavedRun", new Type[1] { SerializableRunType }, (Type[])null) ?? throw new MissingMethodException(RunManagerType.FullName, "InitializeSavedRun");

	private static readonly MethodInfo NGameLoadRun = AccessTools.Method(NGameType, "LoadRun", new Type[2]
	{
		RunStateType,
		AccessTools.TypeByName("MegaCrit.Sts2.Core.Saves.Runs.SerializableRoom")
	}, (Type[])null) ?? throw new MissingMethodException(NGameType.FullName, "LoadRun");

	private static readonly MethodInfo MapSelectionSynchronizerBeforeMapGenerated = AccessTools.Method(MapSelectionSynchronizerType, "BeforeMapGenerated", (Type[])null, (Type[])null) ?? throw new MissingMethodException(MapSelectionSynchronizerType.FullName, "BeforeMapGenerated");

	private static readonly MethodInfo EventSynchronizerClearPlayerVotes = AccessTools.Method(EventSynchronizerType, "ClearPlayerVotes", (Type[])null, (Type[])null) ?? throw new MissingMethodException(EventSynchronizerType.FullName, "ClearPlayerVotes");

	private static readonly MethodInfo NRelicInventoryAdd = AccessTools.Method(NRelicInventoryType, "Add", new Type[3]
	{
		RelicModelType,
		typeof(bool),
		typeof(int)
	}, (Type[])null) ?? throw new MissingMethodException(NRelicInventoryType.FullName, "Add");

	private static readonly MethodInfo CreatureSetCurrentHpInternal = AccessTools.Method(CreatureType, "SetCurrentHpInternal", new Type[1] { typeof(decimal) }, (Type[])null) ?? throw new MissingMethodException(CreatureType.FullName, "SetCurrentHpInternal");

	private static readonly MethodInfo CreatureSetMaxHpInternal = AccessTools.Method(CreatureType, "SetMaxHpInternal", new Type[1] { typeof(decimal) }, (Type[])null) ?? throw new MissingMethodException(CreatureType.FullName, "SetMaxHpInternal");

	private static readonly MethodInfo ActModelGetNumberOfFloors = AccessTools.Method(ActModelType, "GetNumberOfFloors", new Type[1] { typeof(bool) }, (Type[])null) ?? throw new MissingMethodException(ActModelType.FullName, "GetNumberOfFloors");

	private static readonly MethodInfo ActModelToMutable = AccessTools.Method(ActModelType, "ToMutable", (Type[])null, (Type[])null) ?? throw new MissingMethodException(ActModelType.FullName, "ToMutable");

	private static readonly MethodInfo ActModelGetRandomList = AccessTools.Method(ActModelType, "GetRandomList", new Type[3]
	{
		RngType,
		UnlockStateType,
		typeof(bool)
	}, (Type[])null) ?? AccessTools.Method(ActModelType, "GetRandomList", new Type[3]
	{
		typeof(string),
		UnlockStateType,
		typeof(bool)
	}, (Type[])null) ?? throw new MissingMethodException(ActModelType.FullName, "GetRandomList");

	private static readonly MethodInfo ActModelGenerateRooms = AccessTools.Method(ActModelType, "GenerateRooms", new Type[3]
	{
		RngType,
		UnlockStateType,
		typeof(bool)
	}, (Type[])null) ?? throw new MissingMethodException(ActModelType.FullName, "GenerateRooms");

	private static readonly MethodInfo ActModelSetBossEncounter = AccessTools.Method(ActModelType, "SetBossEncounter", new Type[1] { EncounterModelType }, (Type[])null) ?? throw new MissingMethodException(ActModelType.FullName, "SetBossEncounter");

	private static readonly MethodInfo ActModelSetSecondBossEncounter = AccessTools.Method(ActModelType, "SetSecondBossEncounter", new Type[1] { EncounterModelType }, (Type[])null) ?? throw new MissingMethodException(ActModelType.FullName, "SetSecondBossEncounter");

	private static readonly MethodInfo RelicGrabBagPopulateForPlayer = AccessTools.Method(RelicGrabBagType, "Populate", new Type[2] { PlayerType, RngType }, (Type[])null) ?? throw new MissingMethodException(RelicGrabBagType.FullName, "Populate");

	private static readonly MethodInfo PlayerInitializeSeed = AccessTools.Method(PlayerType, "InitializeSeed", new Type[1] { typeof(string) }, (Type[])null) ?? throw new MissingMethodException(PlayerType.FullName, "InitializeSeed");

	private static readonly MethodInfo RunRngSetMockRng = AccessTools.Method(AccessTools.TypeByName("MegaCrit.Sts2.Core.Runs.RunRngSet"), "MockRng", new Type[2]
	{
		RunRngTypeType,
		typeof(uint)
	}, (Type[])null) ?? throw new MissingMethodException("MegaCrit.Sts2.Core.Runs.RunRngSet", "MockRng");

	private static readonly MethodInfo StandardActMapCreateFor = AccessTools.Method(StandardActMapType, "CreateFor", new Type[2]
	{
		RunStateType,
		typeof(bool)
	}, (Type[])null) ?? throw new MissingMethodException(StandardActMapType.FullName, "CreateFor");

	private static readonly PropertyInfo RunManagerState = AccessTools.Property(RunManagerType, "State") ?? throw new MissingMemberException(RunManagerType.FullName, "State");

	private static readonly PropertyInfo RunManagerRunTime = AccessTools.Property(RunManagerType, "RunTime") ?? throw new MissingMemberException(RunManagerType.FullName, "RunTime");

	private static readonly PropertyInfo RunManagerSavedMapsToLoad = AccessTools.Property(RunManagerType, "SavedMapsToLoad") ?? throw new MissingMemberException(RunManagerType.FullName, "SavedMapsToLoad");

	private static readonly PropertyInfo RunManagerMapDrawingsToLoad = AccessTools.Property(RunManagerType, "MapDrawingsToLoad") ?? throw new MissingMemberException(RunManagerType.FullName, "MapDrawingsToLoad");

	private static readonly PropertyInfo RunManagerMapSelectionSynchronizer = AccessTools.Property(RunManagerType, "MapSelectionSynchronizer") ?? throw new MissingMemberException(RunManagerType.FullName, "MapSelectionSynchronizer");

	private static readonly PropertyInfo RunManagerActChangeSynchronizer = AccessTools.Property(RunManagerType, "ActChangeSynchronizer") ?? throw new MissingMemberException(RunManagerType.FullName, "ActChangeSynchronizer");

	private static readonly PropertyInfo RunManagerEventSynchronizer = AccessTools.Property(RunManagerType, "EventSynchronizer") ?? throw new MissingMemberException(RunManagerType.FullName, "EventSynchronizer");

	private static readonly PropertyInfo RunManagerRewardSynchronizer = AccessTools.Property(RunManagerType, "RewardSynchronizer") ?? throw new MissingMemberException(RunManagerType.FullName, "RewardSynchronizer");

	private static readonly PropertyInfo? RunManagerRewardsSetSynchronizer = AccessTools.Property(RunManagerType, "RewardsSetSynchronizer");

	private static readonly PropertyInfo? RunManagerRestSiteSynchronizer = AccessTools.Property(RunManagerType, "RestSiteSynchronizer");

	private static readonly PropertyInfo? RunManagerOneOffSynchronizer = AccessTools.Property(RunManagerType, "OneOffSynchronizer");

	private static readonly PropertyInfo RunManagerTreasureRoomRelicSynchronizer = AccessTools.Property(RunManagerType, "TreasureRoomRelicSynchronizer") ?? throw new MissingMemberException(RunManagerType.FullName, "TreasureRoomRelicSynchronizer");

	private static readonly PropertyInfo RunManagerPlayerChoiceSynchronizer = AccessTools.Property(RunManagerType, "PlayerChoiceSynchronizer") ?? throw new MissingMemberException(RunManagerType.FullName, "PlayerChoiceSynchronizer");

	private static readonly PropertyInfo RunManagerActionQueueSynchronizer = AccessTools.Property(RunManagerType, "ActionQueueSynchronizer") ?? throw new MissingMemberException(RunManagerType.FullName, "ActionQueueSynchronizer");

	private static readonly PropertyInfo RunManagerCombatStateSynchronizer = AccessTools.Property(RunManagerType, "CombatStateSynchronizer") ?? throw new MissingMemberException(RunManagerType.FullName, "CombatStateSynchronizer");

	private static readonly PropertyInfo RunManagerRunLocationTargetedBuffer = AccessTools.Property(RunManagerType, "RunLocationTargetedBuffer") ?? throw new MissingMemberException(RunManagerType.FullName, "RunLocationTargetedBuffer");

	private static readonly PropertyInfo RunManagerRunLobby = AccessTools.Property(RunManagerType, "RunLobby") ?? throw new MissingMemberException(RunManagerType.FullName, "RunLobby");

	private static readonly PropertyInfo RunManagerWinTime = AccessTools.Property(RunManagerType, "WinTime") ?? throw new MissingMemberException(RunManagerType.FullName, "WinTime");

	private static readonly PropertyInfo RunManagerShouldSave = AccessTools.Property(RunManagerType, "ShouldSave") ?? throw new MissingMemberException(RunManagerType.FullName, "ShouldSave");

	private static readonly PropertyInfo RunManagerIsCleaningUp = AccessTools.Property(RunManagerType, "IsCleaningUp") ?? throw new MissingMemberException(RunManagerType.FullName, "IsCleaningUp");

	private static readonly PropertyInfo RunManagerIsAbandoned = AccessTools.Property(RunManagerType, "IsAbandoned") ?? throw new MissingMemberException(RunManagerType.FullName, "IsAbandoned");

	private static readonly MethodInfo RunManagerClearScreens = AccessTools.Method(RunManagerType, "ClearScreens", (Type[])null, (Type[])null) ?? throw new MissingMethodException(RunManagerType.FullName, "ClearScreens");

	private static readonly PropertyInfo RunStateActs = AccessTools.Property(RunStateType, "Acts") ?? throw new MissingMemberException(RunStateType.FullName, "Acts");

	private static readonly PropertyInfo RunStateAscensionLevel = AccessTools.Property(RunStateType, "AscensionLevel") ?? throw new MissingMemberException(RunStateType.FullName, "AscensionLevel");

	private static readonly PropertyInfo RunStateActFloor = AccessTools.Property(RunStateType, "ActFloor") ?? throw new MissingMemberException(RunStateType.FullName, "ActFloor");

	private static readonly PropertyInfo RunStateCurrentActIndex = AccessTools.Property(RunStateType, "CurrentActIndex") ?? throw new MissingMemberException(RunStateType.FullName, "CurrentActIndex");

	private static readonly PropertyInfo RunStateTotalFloor = AccessTools.Property(RunStateType, "TotalFloor") ?? throw new MissingMemberException(RunStateType.FullName, "TotalFloor");

	private static readonly PropertyInfo RunStateIsGameOver = AccessTools.Property(RunStateType, "IsGameOver") ?? throw new MissingMemberException(RunStateType.FullName, "IsGameOver");

	private static readonly PropertyInfo RunStateMap = AccessTools.Property(RunStateType, "Map") ?? throw new MissingMemberException(RunStateType.FullName, "Map");

	private static readonly PropertyInfo RunStatePlayers = AccessTools.Property(RunStateType, "Players") ?? throw new MissingMemberException(RunStateType.FullName, "Players");

	private static readonly PropertyInfo RunStateRng = AccessTools.Property(RunStateType, "Rng") ?? throw new MissingMemberException(RunStateType.FullName, "Rng");

	private static readonly PropertyInfo RunStateSharedRelicGrabBag = AccessTools.Property(RunStateType, "SharedRelicGrabBag") ?? throw new MissingMemberException(RunStateType.FullName, "SharedRelicGrabBag");

	private static readonly PropertyInfo RunStateUnlockState = AccessTools.Property(RunStateType, "UnlockState") ?? throw new MissingMemberException(RunStateType.FullName, "UnlockState");

	private static readonly PropertyInfo RunStateExtraFields = AccessTools.Property(RunStateType, "ExtraFields") ?? throw new MissingMemberException(RunStateType.FullName, "ExtraFields");

	private static readonly PropertyInfo ActModelAllBossEncounters = AccessTools.Property(ActModelType, "AllBossEncounters") ?? throw new MissingMemberException(ActModelType.FullName, "AllBossEncounters");

	private static readonly PropertyInfo ActModelBossEncounter = AccessTools.Property(ActModelType, "BossEncounter") ?? throw new MissingMemberException(ActModelType.FullName, "BossEncounter");

	private static readonly PropertyInfo ActModelAncient = AccessTools.Property(ActModelType, "Ancient") ?? throw new MissingMemberException(ActModelType.FullName, "Ancient");

	private static readonly PropertyInfo ActModelHasSecondBoss = AccessTools.Property(ActModelType, "HasSecondBoss") ?? throw new MissingMemberException(ActModelType.FullName, "HasSecondBoss");

	private static readonly PropertyInfo ActModelSecondBossEncounter = AccessTools.Property(ActModelType, "SecondBossEncounter") ?? throw new MissingMemberException(ActModelType.FullName, "SecondBossEncounter");

	private static readonly PropertyInfo SerializableRunPreFinishedRoom = AccessTools.Property(SerializableRunType, "PreFinishedRoom") ?? throw new MissingMemberException(SerializableRunType.FullName, "PreFinishedRoom");

	private static readonly PropertyInfo SerializableRunPlayers = AccessTools.Property(SerializableRunType, "Players") ?? throw new MissingMemberException(SerializableRunType.FullName, "Players");

	private static readonly PropertyInfo SerializableRunEventsSeen = AccessTools.Property(SerializableRunType, "EventsSeen") ?? throw new MissingMemberException(SerializableRunType.FullName, "EventsSeen");

	private static readonly PropertyInfo SerializableRunWinTime = AccessTools.Property(SerializableRunType, "WinTime") ?? throw new MissingMemberException(SerializableRunType.FullName, "WinTime");

	private static readonly PropertyInfo SerializableRunExtraFields = AccessTools.Property(SerializableRunType, "ExtraFields") ?? throw new MissingMemberException(SerializableRunType.FullName, "ExtraFields");

	private static readonly PropertyInfo ExtraRunFieldsTestSubjectKills = AccessTools.Property(ExtraRunFieldsType, "TestSubjectKills") ?? throw new MissingMemberException(ExtraRunFieldsType.FullName, "TestSubjectKills");

	private static readonly PropertyInfo SerializableExtraRunFieldsTestSubjectKills = AccessTools.Property(SerializableExtraRunFieldsType, "TestSubjectKills") ?? throw new MissingMemberException(SerializableExtraRunFieldsType.FullName, "TestSubjectKills");

	private static readonly Type SerializablePlayerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Saves.Runs.SerializablePlayer") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Saves.Runs.SerializablePlayer");

	private static readonly PropertyInfo SerializablePlayerCurrentHp = AccessTools.Property(SerializablePlayerType, "CurrentHp") ?? throw new MissingMemberException(SerializablePlayerType.FullName, "CurrentHp");

	private static readonly PropertyInfo SerializablePlayerMaxHp = AccessTools.Property(SerializablePlayerType, "MaxHp") ?? throw new MissingMemberException(SerializablePlayerType.FullName, "MaxHp");

	private static readonly PropertyInfo AbstractModelId = AccessTools.Property(AbstractModelType, "Id") ?? throw new MissingMemberException(AbstractModelType.FullName, "Id");

	private static readonly PropertyInfo CreatureIsMonster = AccessTools.Property(CreatureType, "IsMonster") ?? throw new MissingMemberException(CreatureType.FullName, "IsMonster");

	private static readonly PropertyInfo CreatureCurrentHp = AccessTools.Property(CreatureType, "CurrentHp") ?? throw new MissingMemberException(CreatureType.FullName, "CurrentHp");

	private static readonly PropertyInfo CreatureMaxHp = AccessTools.Property(CreatureType, "MaxHp") ?? throw new MissingMemberException(CreatureType.FullName, "MaxHp");

	private static readonly PropertyInfo CreatureMonsterMaxHpBeforeModification = AccessTools.Property(CreatureType, "MonsterMaxHpBeforeModification") ?? throw new MissingMemberException(CreatureType.FullName, "MonsterMaxHpBeforeModification");

	private static readonly PropertyInfo CreatureCombatState = AccessTools.Property(CreatureType, "CombatState") ?? throw new MissingMemberException(CreatureType.FullName, "CombatState");

	private static readonly PropertyInfo CombatStateRunState = AccessTools.Property(CombatStateType, "RunState") ?? throw new MissingMemberException(CombatStateType.FullName, "RunState");

	private static readonly PropertyInfo RunRngSetStringSeed = AccessTools.Property(AccessTools.TypeByName("MegaCrit.Sts2.Core.Runs.RunRngSet"), "StringSeed") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Runs.RunRngSet", "StringSeed");

	private static readonly PropertyInfo RunRngSetUnknownMapPoint = AccessTools.Property(AccessTools.TypeByName("MegaCrit.Sts2.Core.Runs.RunRngSet"), "UnknownMapPoint") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Runs.RunRngSet", "UnknownMapPoint");

	private static readonly PropertyInfo RunRngSetTreasureRoomRelics = AccessTools.Property(AccessTools.TypeByName("MegaCrit.Sts2.Core.Runs.RunRngSet"), "TreasureRoomRelics") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Runs.RunRngSet", "TreasureRoomRelics");

	private static readonly PropertyInfo AttackCommandAttacker = AccessTools.Property(AttackCommandType, "Attacker") ?? throw new MissingMemberException(AttackCommandType.FullName, "Attacker");

	private static readonly PropertyInfo AttackIntentDamageCalc = AccessTools.Property(AttackIntentType, "DamageCalc") ?? throw new MissingMemberException(AttackIntentType.FullName, "DamageCalc");

	private static readonly PropertyInfo RelicModelDisplayAmount = AccessTools.Property(RelicModelType, "DisplayAmount") ?? throw new MissingMemberException(RelicModelType.FullName, "DisplayAmount");

	private static readonly PropertyInfo RelicModelShowCounter = AccessTools.Property(RelicModelType, "ShowCounter") ?? throw new MissingMemberException(RelicModelType.FullName, "ShowCounter");

	private static readonly FieldInfo AttackCommandDamagePerHit = AccessTools.Field(AttackCommandType, "_damagePerHit") ?? throw new MissingFieldException(AttackCommandType.FullName, "_damagePerHit");

	private static readonly MethodInfo RunStateClearVisitedMapCoordsDebug = AccessTools.Method(RunStateType, "ClearVisitedMapCoordsDebug", (Type[])null, (Type[])null) ?? throw new MissingMethodException(RunStateType.FullName, "ClearVisitedMapCoordsDebug");

	private static readonly MethodInfo RunStateGetPlayerSlotIndex = AccessTools.Method(RunStateType, "GetPlayerSlotIndex", new Type[1] { PlayerType }, (Type[])null) ?? throw new MissingMethodException(RunStateType.FullName, "GetPlayerSlotIndex");

	private static readonly FieldInfo MapSelectionSynchronizerRunState = AccessTools.Field(MapSelectionSynchronizerType, "_runState") ?? throw new MissingFieldException(MapSelectionSynchronizerType.FullName, "_runState");

	private static readonly FieldInfo ActChangeSynchronizerReadyPlayers = AccessTools.Field(ActChangeSynchronizerType, "_readyPlayers") ?? throw new MissingFieldException(ActChangeSynchronizerType.FullName, "_readyPlayers");

	private static readonly FieldInfo MapSelectionSynchronizerVotes = AccessTools.Field(MapSelectionSynchronizerType, "_votes") ?? throw new MissingFieldException(MapSelectionSynchronizerType.FullName, "_votes");

	private static readonly FieldInfo EventSynchronizerEvents = AccessTools.Field(EventSynchronizerType, "_events") ?? throw new MissingFieldException(EventSynchronizerType.FullName, "_events");

	private static readonly FieldInfo EventSynchronizerCanonicalEvent = AccessTools.Field(EventSynchronizerType, "_canonicalEvent") ?? throw new MissingFieldException(EventSynchronizerType.FullName, "_canonicalEvent");

	private static readonly FieldInfo EventSynchronizerPageIndex = AccessTools.Field(EventSynchronizerType, "_pageIndex") ?? throw new MissingFieldException(EventSynchronizerType.FullName, "_pageIndex");

	private static readonly FieldInfo EventSynchronizerPlayerVotes = AccessTools.Field(EventSynchronizerType, "_playerVotes") ?? throw new MissingFieldException(EventSynchronizerType.FullName, "_playerVotes");

	private static readonly FieldInfo? EventSynchronizerPlayerCollection = AccessTools.Field(EventSynchronizerType, "_playerCollection");

	private static readonly FieldInfo? EventSynchronizerPendingOptionTasks = AccessTools.Field(EventSynchronizerType, "_pendingOptionTasks");

	private static readonly FieldInfo? RewardSynchronizerPlayerCollection = AccessTools.Field(RewardSynchronizerType, "_playerCollection");

	private static readonly FieldInfo? RewardSynchronizerBufferedMessages = AccessTools.Field(RewardSynchronizerType, "_bufferedMessages");

	private static readonly FieldInfo? RewardsSetSynchronizerPlayerCollection = AccessTools.Field(RewardsSetSynchronizerType, "_playerCollection");

	private static readonly FieldInfo? RewardsSetSynchronizerRewardStates = AccessTools.Field(RewardsSetSynchronizerType, "_rewardStates");

	private static readonly FieldInfo? RestSiteSynchronizerPlayerCollection = (RestSiteSynchronizerType != null) ? AccessTools.Field(RestSiteSynchronizerType, "_playerCollection") : null;

	private static readonly FieldInfo? RestSiteSynchronizerRestSites = (RestSiteSynchronizerType != null) ? AccessTools.Field(RestSiteSynchronizerType, "_restSites") : null;

	private static readonly FieldInfo? OneOffSynchronizerPlayerCollection = (OneOffSynchronizerType != null) ? AccessTools.Field(OneOffSynchronizerType, "_playerCollection") : null;

	private static readonly FieldInfo? ActChangeSynchronizerRunState = AccessTools.Field(ActChangeSynchronizerType, "_runState");

	private static readonly Type? PlayerRewardStateType = AccessTools.Inner(RewardsSetSynchronizerType, "PlayerRewardState");

	private static readonly Type? RewardsSetStateType = AccessTools.Inner(RewardsSetSynchronizerType, "RewardsSetState");

	private static readonly Type? BufferedMessageType = AccessTools.Inner(RewardsSetSynchronizerType, "BufferedMessage");

	private static readonly FieldInfo? PlayerRewardStateRewardsStack = (PlayerRewardStateType != null) ? AccessTools.Field(PlayerRewardStateType, "rewardsStack") : null;

	private static readonly FieldInfo? PlayerRewardStateBufferedMessages = (PlayerRewardStateType != null) ? AccessTools.Field(PlayerRewardStateType, "bufferedMessages") : null;

	private static readonly FieldInfo? PlayerRewardStateNextId = (PlayerRewardStateType != null) ? AccessTools.Field(PlayerRewardStateType, "nextId") : null;

	private static readonly FieldInfo? TreasureRoomRelicSynchronizerPlayerCollection = AccessTools.Field(TreasureRoomRelicSynchronizerType, "_playerCollection");

	private static readonly FieldInfo? TreasureRoomRelicSynchronizerVotes = AccessTools.Field(TreasureRoomRelicSynchronizerType, "_votes");

	private static readonly FieldInfo? TreasureRoomRelicSynchronizerPredictedVote = AccessTools.Field(TreasureRoomRelicSynchronizerType, "_predictedVote");

	private static readonly FieldInfo? TreasureRoomRelicSynchronizerSinglePlayerSkipped = AccessTools.Field(TreasureRoomRelicSynchronizerType, "_singlePlayerSkipped");

	private static readonly FieldInfo? PlayerChoiceSynchronizerPlayers = AccessTools.Field(PlayerChoiceSynchronizerType, "_players");

	private static readonly FieldInfo? PlayerChoiceSynchronizerChoiceIds = AccessTools.Field(PlayerChoiceSynchronizerType, "_choiceIds");

	private static readonly FieldInfo? PlayerChoiceSynchronizerReceivedChoices = AccessTools.Field(PlayerChoiceSynchronizerType, "_receivedChoices");

	private static readonly FieldInfo RunStateVisitedEventIds = AccessTools.Field(RunStateType, "_visitedEventIds") ?? throw new MissingFieldException(RunStateType.FullName, "_visitedEventIds");

	private static readonly FieldInfo CombatManagerState = AccessTools.Field(CombatManagerType, "_state");

	private static readonly FieldInfo CombatManagerPendingLoss = AccessTools.Field(CombatManagerType, "_pendingLoss");

	private static readonly FieldInfo CombatManagerPlayersReadyToBeginEnemyTurn = AccessTools.Field(CombatManagerType, "_playersReadyToBeginEnemyTurn");

	private static readonly FieldInfo CombatManagerPlayersReadyToEndTurn = AccessTools.Field(CombatManagerType, "_playersReadyToEndTurn");

	private static readonly FieldInfo CombatManagerPlayersTakingExtraTurn = AccessTools.Field(CombatManagerType, "_playersTakingExtraTurn");

	private static readonly PropertyInfo NRelicInventoryRelicNodes = AccessTools.Property(NRelicInventoryType, "RelicNodes") ?? throw new MissingMemberException(NRelicInventoryType.FullName, "RelicNodes");

	private static readonly FieldInfo NRelicInventoryRelicNodesField = AccessTools.Field(NRelicInventoryType, "_relicNodes") ?? throw new MissingFieldException(NRelicInventoryType.FullName, "_relicNodes");

	private static readonly FieldInfo NRelicInventoryHolderModel = AccessTools.Field(NRelicInventoryHolderType, "_model") ?? throw new MissingFieldException(NRelicInventoryHolderType.FullName, "_model");

	private static readonly FieldInfo NRelicInventoryHolderAmountLabel = AccessTools.Field(NRelicInventoryHolderType, "_amountLabel") ?? throw new MissingFieldException(NRelicInventoryHolderType.FullName, "_amountLabel");

	private static readonly MethodInfo NRelicInventoryUpdateNavigation = AccessTools.Method(NRelicInventoryType, "UpdateNavigation", (Type[])null, (Type[])null) ?? throw new MissingMethodException(NRelicInventoryType.FullName, "UpdateNavigation");

	public static object GetRunState(object runManager)
	{
		return RunManagerState.GetValue(runManager) ?? throw new InvalidOperationException("RunManager.State returned null.");
	}

	public static object? GetRunManagerState(object? runManager)
	{
		return (runManager != null) ? RunManagerState.GetValue(runManager) : null;
	}

	public static object GetRunManagerInstance()
	{
		return RunManagerInstance.GetValue(null) ?? throw new InvalidOperationException("RunManager.Instance returned null.");
	}

	public static object GetGameInstance()
	{
		return NGameInstance.GetValue(null) ?? throw new InvalidOperationException("NGame.Instance returned null.");
	}

	public static long GetRunTime(object runManager)
	{
		return (long)(RunManagerRunTime.GetValue(runManager) ?? throw new InvalidOperationException("RunManager.RunTime returned null."));
	}

	public static int GetActsCount(object runState)
	{
		ICollection collection = (ICollection)(RunStateActs.GetValue(runState) ?? throw new InvalidOperationException("RunState.Acts returned null."));
		if (collection == null)
		{
			throw new InvalidOperationException("RunState.Acts is not a countable collection.");
		}
		return collection.Count;
	}

	public static List<ActModel> GetActs(object runState)
	{
		return ((IEnumerable<ActModel>)(RunStateActs.GetValue(runState) ?? throw new InvalidOperationException("RunState.Acts returned null."))).ToList();
	}

	public static void SetActs(object runState, IReadOnlyList<ActModel> acts)
	{
		RunStateActs.SetValue(runState, acts);
	}

	public static string GetRunSeedString(object runState)
	{
		object obj = RunStateRng.GetValue(runState) ?? throw new InvalidOperationException("RunState.Rng returned null.");
		return (string)(RunRngSetStringSeed.GetValue(obj) ?? throw new InvalidOperationException("RunRngSet.StringSeed returned null."));
	}

	public static IReadOnlyList<ActModel> GetRandomizedActsForLoop(object runState, string seed)
	{
		object obj = RunStateUnlockState.GetValue(runState) ?? throw new InvalidOperationException("RunState.UnlockState returned null.");
		bool flag = GetPlayersCount(runState) > 1;
		ParameterInfo[] parameters = ActModelGetRandomList.GetParameters();
		object[] parameters2;
		if (parameters.Length != 0 && parameters[0].ParameterType == typeof(string))
		{
			parameters2 = new object[3] { seed, obj, flag };
		}
		else
		{
			object obj2 = RunStateRng.GetValue(runState) ?? throw new InvalidOperationException("RunState.Rng returned null.");
			object obj3 = RunRngSetUnknownMapPoint.GetValue(obj2) ?? throw new InvalidOperationException("RunRngSet.UnknownMapPoint returned null.");
			parameters2 = new object[3] { obj3, obj, flag };
		}
		IEnumerable<ActModel> source = (IEnumerable<ActModel>)(ActModelGetRandomList.Invoke(null, parameters2) ?? throw new InvalidOperationException("ActModel.GetRandomList returned null."));
		return source.Select(CreateMutableAct).Select((ActModel act) =>
		{
			GenerateRoomsForAct(act, runState);
			return act;
		}).ToList();
	}

	public static int GetPlayersCount(object runState)
	{
		object obj = RunStatePlayers.GetValue(runState) ?? throw new InvalidOperationException("RunState.Players returned null.");
		if (!(obj is ICollection { Count: var count }))
		{
			PropertyInfo propertyInfo = AccessTools.Property(obj.GetType(), "Count") ?? throw new InvalidOperationException("RunState.Players is not a countable collection.");
			return (int)(propertyInfo.GetValue(obj) ?? throw new InvalidOperationException("Players.Count returned null."));
		}
		return count;
	}

	public static int GetCurrentActIndex(object runState)
	{
		return (int)(RunStateCurrentActIndex.GetValue(runState) ?? throw new InvalidOperationException("RunState.CurrentActIndex returned null."));
	}

	public static int GetTotalFloor(object runState)
	{
		return (int)(RunStateTotalFloor.GetValue(runState) ?? throw new InvalidOperationException("RunState.TotalFloor returned null."));
	}

	public static int? GetPersistedEndlessLoopCount(object runState)
	{
		object value = RunStateExtraFields.GetValue(runState);
		if (value == null)
		{
			return null;
		}
		int value2 = (int)(ExtraRunFieldsTestSubjectKills.GetValue(value) ?? throw new InvalidOperationException("ExtraRunFields.TestSubjectKills returned null."));
		if (TryDecodePersistedEndlessSettings(value2, out var settings))
		{
			return settings.LoopCount;
		}
		return DecodePersistedEndlessLoopCount(value2);
	}

	public static void SetPersistedEndlessLoopCount(object runState, int loopCount)
	{
		object obj = RunStateExtraFields.GetValue(runState);
		if (obj == null)
		{
			obj = Activator.CreateInstance(RunStateExtraFields.PropertyType) ?? throw new InvalidOperationException("Could not create ExtraRunFields.");
			RunStateExtraFields.SetValue(runState, obj);
		}
		ExtraRunFieldsTestSubjectKills.SetValue(obj, EncodePersistedEndlessLoopCount(loopCount));
	}

	public static void SetPersistedEndlessMetadata(object runState, int loopCount, decimal hpScalePerChapter, decimal damageScalePerChapter)
	{
		EndlessScalingSettings settings = EndlessScalingSettings.FromConfig(loopCount);
		settings.HpScale = hpScalePerChapter;
		settings.DamageScale = damageScalePerChapter;
		SetPersistedEndlessSettings(runState, settings);
	}

	public static void SetPersistedEndlessSettings(object runState, EndlessScalingSettings settings)
	{
		object obj = RunStateExtraFields.GetValue(runState);
		if (obj == null)
		{
			obj = Activator.CreateInstance(RunStateExtraFields.PropertyType) ?? throw new InvalidOperationException("Could not create ExtraRunFields.");
			RunStateExtraFields.SetValue(runState, obj);
		}
		ExtraRunFieldsTestSubjectKills.SetValue(obj, EncodePersistedEndlessSettings(settings));
	}

	public static bool TryGetPersistedEndlessScaling(object runState, out int loopCount, out decimal hpScalePerChapter, out decimal damageScalePerChapter)
	{
		if (TryGetPersistedEndlessSettings(runState, out var settings))
		{
			loopCount = settings.LoopCount;
			hpScalePerChapter = settings.HpScale;
			damageScalePerChapter = settings.DamageScale;
			return true;
		}
		loopCount = 0;
		hpScalePerChapter = 0m;
		damageScalePerChapter = 0m;
		return false;
	}

	public static bool TryGetPersistedEndlessSettings(object runState, out EndlessScalingSettings settings)
	{
		settings = default;
		object value = RunStateExtraFields.GetValue(runState);
		if (value == null)
		{
			return false;
		}
		int value2 = (int)(ExtraRunFieldsTestSubjectKills.GetValue(value) ?? throw new InvalidOperationException("ExtraRunFields.TestSubjectKills returned null."));
		return TryDecodePersistedEndlessSettings(value2, out settings);
	}

	public static int? GetPersistedEndlessLoopCountFromSave(object save)
	{
		object value = SerializableRunExtraFields.GetValue(save);
		if (value == null)
		{
			return null;
		}
		int value2 = (int)(SerializableExtraRunFieldsTestSubjectKills.GetValue(value) ?? throw new InvalidOperationException("SerializableExtraRunFields.TestSubjectKills returned null."));
		if (TryDecodePersistedEndlessSettings(value2, out var settings))
		{
			return settings.LoopCount;
		}
		return DecodePersistedEndlessLoopCount(value2);
	}

	public static bool TryGetPersistedEndlessSettingsFromSave(object save, out EndlessScalingSettings settings)
	{
		settings = default;
		object value = SerializableRunExtraFields.GetValue(save);
		if (value == null)
		{
			return false;
		}
		int value2 = (int)(SerializableExtraRunFieldsTestSubjectKills.GetValue(value) ?? throw new InvalidOperationException("SerializableExtraRunFields.TestSubjectKills returned null."));
		return TryDecodePersistedEndlessSettings(value2, out settings);
	}

	public static void SetPersistedEndlessLoopCountOnSave(object save, int loopCount)
	{
		object obj = SerializableRunExtraFields.GetValue(save);
		if (obj == null)
		{
			obj = Activator.CreateInstance(SerializableRunExtraFields.PropertyType) ?? throw new InvalidOperationException("Could not create SerializableExtraRunFields.");
			SerializableRunExtraFields.SetValue(save, obj);
		}
		SerializableExtraRunFieldsTestSubjectKills.SetValue(obj, EncodePersistedEndlessLoopCount(loopCount));
	}

	public static void SetPersistedEndlessMetadataOnSave(object save, int loopCount, decimal hpScalePerChapter, decimal damageScalePerChapter)
	{
		EndlessScalingSettings settings = EndlessScalingSettings.FromConfig(loopCount);
		settings.HpScale = hpScalePerChapter;
		settings.DamageScale = damageScalePerChapter;
		SetPersistedEndlessSettingsOnSave(save, settings);
	}

	public static void SetPersistedEndlessSettingsOnSave(object save, EndlessScalingSettings settings)
	{
		object obj = SerializableRunExtraFields.GetValue(save);
		if (obj == null)
		{
			obj = Activator.CreateInstance(SerializableRunExtraFields.PropertyType) ?? throw new InvalidOperationException("Could not create SerializableExtraRunFields.");
			SerializableRunExtraFields.SetValue(save, obj);
		}
		SerializableExtraRunFieldsTestSubjectKills.SetValue(obj, EncodePersistedEndlessSettings(settings));
	}

	private static int EncodePersistedEndlessLoopCount(int loopCount)
	{
		return -10000000 - Math.Max(0, loopCount);
	}

	private static int? DecodePersistedEndlessLoopCount(int value)
	{
		if (value > -10000000)
		{
			return null;
		}
		return Math.Max(0, -10000000 - value);
	}

	private const int V2Marker = 0x40000000;
	private static readonly int[] FlatHpTable = new int[] { 0, 100, 250, 500, 750, 1000, 1500, 2000 };
	private static readonly int[] FlatDmgTable = new int[] { 0, 1, 2, 3, 4, 5, 8, 10 };

	private static int EncodePersistedEndlessSettings(EndlessScalingSettings settings)
	{
		int loop = Math.Clamp(settings.LoopCount, 0, 63);
		int intervalCode = Math.Clamp((int)settings.Interval, 0, 2);
		int curveCode = Math.Clamp((int)settings.Curve, 0, 2);
		int skipChCode = settings.ScaleChapterOne ? 0 : 1;
		int hpCode = EncodeHpScaleCode(settings.HpScale);
		int dmgCode = EncodeDamageScaleCode(settings.DamageScale);
		int flatHpCode = EncodeFlatHpCode(settings.FlatHp);
		int flatDmgCode = EncodeFlatDmgCode(settings.FlatDmg);

		int payload = V2Marker
			| (flatDmgCode << 27)
			| (flatHpCode << 24)
			| (dmgCode << 18)
			| (hpCode << 11)
			| (skipChCode << 10)
			| (curveCode << 8)
			| (intervalCode << 6)
			| loop;

		return int.MinValue + payload;
	}

	private static bool TryDecodePersistedEndlessSettings(int value, out EndlessScalingSettings settings)
	{
		settings = default;
		if (value > -1)
		{
			return false;
		}

		int payload = value - int.MinValue;
		if ((payload & V2Marker) != 0)
		{
			int loop = payload & 0x3F;
			int intervalCode = (payload >> 6) & 0x3;
			int curveCode = (payload >> 8) & 0x3;
			int skipChCode = (payload >> 10) & 0x1;
			int hpCode = (payload >> 11) & 0x7F;
			int dmgCode = (payload >> 18) & 0x3F;
			int flatHpCode = (payload >> 24) & 0x7;
			int flatDmgCode = (payload >> 27) & 0x7;

			settings = new EndlessScalingSettings
			{
				LoopCount = loop,
				Interval = (ScalingInterval)Math.Clamp(intervalCode, 0, 2),
				Curve = (ScalingCurve)Math.Clamp(curveCode, 0, 2),
				ScaleChapterOne = (skipChCode == 0),
				HpScale = DecodeHpScaleCode(hpCode),
				DamageScale = DecodeDamageScaleCode(dmgCode),
				FlatHp = DecodeFlatHpCode(flatHpCode),
				FlatDmg = DecodeFlatDmgCode(flatDmgCode)
			};
			return settings.HpScale >= 1.0m && settings.DamageScale >= 1.0m;
		}

		// Backward-compatible V1 decoding
		if (value <= -1073741825)
		{
			int loop = payload >> 24;
			decimal hpScale = DecodeScaleCode((payload >> 12) & 0xFFF);
			decimal dmgScale = DecodeScaleCode(payload & 0xFFF);
			if (hpScale >= 1m && dmgScale >= 1m)
			{
				settings = new EndlessScalingSettings
				{
					LoopCount = loop,
					Interval = EndlessConfig.Interval,
					Curve = EndlessConfig.Curve,
					ScaleChapterOne = EndlessConfig.ScaleChapterOne,
					HpScale = hpScale,
					DamageScale = dmgScale,
					FlatHp = EndlessConfig.FlatHpPerStep,
					FlatDmg = EndlessConfig.FlatDmgPerStep
				};
				return true;
			}
		}

		return false;
	}

	private static int EncodeHpScaleCode(decimal scale)
	{
		decimal s = Math.Clamp(scale, 1.00m, 3.00m);
		if (s <= 1.50m)
		{
			return Math.Clamp((int)Math.Round((s - 1.00m) * 100m, MidpointRounding.AwayFromZero), 0, 50);
		}
		return Math.Clamp(50 + (int)Math.Round((s - 1.50m) * 50m, MidpointRounding.AwayFromZero), 51, 125);
	}

	private static decimal DecodeHpScaleCode(int code)
	{
		if (code <= 50)
		{
			return 1.00m + (decimal)code * 0.01m;
		}
		return 1.50m + (decimal)(code - 50) * 0.02m;
	}

	private static int EncodeDamageScaleCode(decimal scale)
	{
		decimal s = Math.Clamp(scale, 1.00m, 2.50m);
		if (s <= 1.30m)
		{
			return Math.Clamp((int)Math.Round((s - 1.00m) * 100m, MidpointRounding.AwayFromZero), 0, 30);
		}
		return Math.Clamp(30 + (int)Math.Round((s - 1.30m) * 25m, MidpointRounding.AwayFromZero), 31, 60);
	}

	private static decimal DecodeDamageScaleCode(int code)
	{
		if (code <= 30)
		{
			return 1.00m + (decimal)code * 0.01m;
		}
		return 1.30m + (decimal)(code - 30) * 0.04m;
	}

	private static int EncodeFlatHpCode(int flatHp)
	{
		int bestIndex = 0;
		int bestDiff = Math.Abs(flatHp - FlatHpTable[0]);
		for (int i = 1; i < FlatHpTable.Length; i++)
		{
			int diff = Math.Abs(flatHp - FlatHpTable[i]);
			if (diff < bestDiff)
			{
				bestDiff = diff;
				bestIndex = i;
			}
		}
		return bestIndex;
	}

	private static int DecodeFlatHpCode(int code)
	{
		int index = Math.Clamp(code, 0, FlatHpTable.Length - 1);
		return FlatHpTable[index];
	}

	private static int EncodeFlatDmgCode(int flatDmg)
	{
		int bestIndex = 0;
		int bestDiff = Math.Abs(flatDmg - FlatDmgTable[0]);
		for (int i = 1; i < FlatDmgTable.Length; i++)
		{
			int diff = Math.Abs(flatDmg - FlatDmgTable[i]);
			if (diff < bestDiff)
			{
				bestDiff = diff;
				bestIndex = i;
			}
		}
		return bestIndex;
	}

	private static int DecodeFlatDmgCode(int code)
	{
		int index = Math.Clamp(code, 0, FlatDmgTable.Length - 1);
		return FlatDmgTable[index];
	}

	private static int EncodeScaleCode(decimal scale)
	{
		decimal num = Math.Max(1m, scale);
		if (num <= 20.47m)
		{
			return Math.Clamp((int)Math.Round(num * 100m, MidpointRounding.AwayFromZero), 100, 2047);
		}
		int num2 = (int)Math.Round(num, MidpointRounding.AwayFromZero);
		return Math.Clamp(2048 + num2 - 21, 2048, 4095);
	}

	private static decimal DecodeScaleCode(int code)
	{
		if (code <= 2047)
		{
			return (decimal)code / 100m;
		}
		return 21m + (decimal)code - 2048m;
	}

	public static bool IsGameOver(object runState)
	{
		return (bool)(RunStateIsGameOver.GetValue(runState) ?? throw new InvalidOperationException("RunState.IsGameOver returned null."));
	}

	public static int GetAscensionLevel(object runState)
	{
		return (int)(RunStateAscensionLevel.GetValue(runState) ?? throw new InvalidOperationException("RunState.AscensionLevel returned null."));
	}

	public static void SetAscensionLevel(object runState, int value)
	{
		RunStateAscensionLevel.SetValue(runState, value);
	}

	public static void SetActFloor(object runState, int value)
	{
		RunStateActFloor.SetValue(runState, value);
	}

	public static void SetCurrentActIndex(object runState, int value)
	{
		RunStateCurrentActIndex.SetValue(runState, value);
	}

	public static void SetMap(object runState, object map)
	{
		RunStateMap.SetValue(runState, map);
	}

	public static void SetRunState(object runManager, object runState)
	{
		RunManagerState.SetValue(runManager, runState);
	}

	public static int GetCycleFloorCount(object runState)
	{
		List<ActModel> acts = GetActs(runState);
		bool isMultiplayer = GetPlayersCount(runState) > 1;
		return acts.Sum((ActModel act) => (int)(ActModelGetNumberOfFloors.Invoke(act, new object[1] { isMultiplayer }) ?? throw new InvalidOperationException("ActModel.GetNumberOfFloors returned null.")));
	}

	public static object CreateFreshActMap(object runState, bool replaceTreasureWithElites = false)
	{
		return StandardActMapCreateFor.Invoke(null, new object[2] { runState, replaceTreasureWithElites }) ?? throw new InvalidOperationException("StandardActMap.CreateFor returned null.");
	}

	public static ActModel CreateMutableAct(ActModel act)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected Obj, but got Unknown
		try
		{
			return (ActModel)(ActModelToMutable.Invoke(act, Array.Empty<object>()) ?? throw new InvalidOperationException("ActModel.ToMutable returned null."));
		}
		catch (TargetInvocationException ex) when (ex.InnerException?.GetType().Name == "MutableModelException")
		{
			return act;
		}
	}

	public static List<EncounterModel> GetAllBossEncounters(ActModel act)
	{
		return ((IEnumerable<EncounterModel>)(ActModelAllBossEncounters.GetValue(act) ?? throw new InvalidOperationException("ActModel.AllBossEncounters returned null."))).ToList();
	}

	public static EncounterModel? GetBossEncounter(ActModel act)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected Obj, but got Unknown
		try
		{
			return (EncounterModel)ActModelBossEncounter.GetValue(act);
		}
		catch (Exception exception) when (IsRoomSetNotGeneratedException(exception))
		{
			return null;
		}
	}

	public static object? GetAncient(ActModel act)
	{
		try
		{
			return ActModelAncient.GetValue(act);
		}
		catch (Exception exception) when (IsRoomSetNotGeneratedException(exception))
		{
			return null;
		}
	}

	public static bool HasSecondBoss(ActModel act)
	{
		try
		{
			return (bool)(ActModelHasSecondBoss.GetValue(act) ?? throw new InvalidOperationException("ActModel.HasSecondBoss returned null."));
		}
		catch (Exception exception) when (IsRoomSetNotGeneratedException(exception))
		{
			return false;
		}
	}

	public static EncounterModel? GetSecondBossEncounter(ActModel act)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected Obj, but got Unknown
		try
		{
			return (EncounterModel)ActModelSecondBossEncounter.GetValue(act);
		}
		catch (Exception exception) when (IsRoomSetNotGeneratedException(exception))
		{
			return null;
		}
	}

	public static void SetBossEncounter(ActModel act, EncounterModel encounter)
	{
		ActModelSetBossEncounter.Invoke(act, new object[1] { encounter });
	}

	public static void SetSecondBossEncounter(ActModel act, EncounterModel encounter)
	{
		ActModelSetSecondBossEncounter.Invoke(act, new object[1] { encounter });
	}

	public static void GenerateRoomsForAct(ActModel act, object runState)
	{
		object obj = RunStateUnlockState.GetValue(runState) ?? throw new InvalidOperationException("RunState.UnlockState returned null.");
		object obj2 = RunStateRng.GetValue(runState) ?? throw new InvalidOperationException("RunState.Rng returned null.");
		object obj3 = RunRngSetUnknownMapPoint.GetValue(obj2) ?? throw new InvalidOperationException("RunRngSet.UnknownMapPoint returned null.");
		bool flag = GetPlayersCount(runState) > 1;
		ActModelGenerateRooms.Invoke(act, new object[3] { obj3, obj, flag });
	}

	public static void ReseedUnknownMapPointRng(object runState, uint seed)
	{
		object obj = RunStateRng.GetValue(runState) ?? throw new InvalidOperationException("RunState.Rng returned null.");
		object obj2 = Enum.Parse(RunRngTypeType, "UnknownMapPoint");
		RunRngSetMockRng.Invoke(obj, new object[2] { obj2, seed });
	}

	public static bool TryReseedRunRng(object runState, string rngName, uint seed)
	{
		try
		{
			object obj = RunStateRng.GetValue(runState) ?? throw new InvalidOperationException("RunState.Rng returned null.");
			object obj2 = Enum.Parse(RunRngTypeType, rngName);
			RunRngSetMockRng.Invoke(obj, new object[2] { obj2, seed });
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static void InitializePlayerSeed(object player, string seed)
	{
		PlayerInitializeSeed.Invoke(player, new object[1] { seed });
	}

	public static string GetModelId(object model)
	{
		return (AbstractModelId.GetValue(model) ?? throw new InvalidOperationException("AbstractModel.Id returned null.")).ToString() ?? "<null>";
	}

	public static void ResetSavedMapsToLoad(object runManager)
	{
		object value = Activator.CreateInstance(RunManagerSavedMapsToLoad.PropertyType) ?? throw new InvalidOperationException("Could not create empty SavedMapsToLoad dictionary.");
		RunManagerSavedMapsToLoad.SetValue(runManager, value);
	}

	public static void ClearMapDrawingsToLoad(object runManager)
	{
		RunManagerMapDrawingsToLoad.SetValue(runManager, null);
	}

	public static void ResetWinTime(object runManager)
	{
		RunManagerWinTime.SetValue(runManager, 0L);
	}

	public static void PrepareMapSelectionForFreshMap(object runManager, object runState)
	{
		object obj = RunManagerMapSelectionSynchronizer.GetValue(runManager) ?? throw new InvalidOperationException("RunManager.MapSelectionSynchronizer returned null.");
		int playersCount = GetPlayersCount(runState);
		MapSelectionSynchronizerRunState.SetValue(obj, runState);
		ResetMapSelectionVoteCollections(obj, playersCount);
		TryInvokeMethod(obj, "ClearPlayerVotes", Array.Empty<object>(), out object _);
		MapSelectionSynchronizerBeforeMapGenerated.Invoke(obj, Array.Empty<object>());
		ResetMapSelectionVoteCollections(obj, playersCount);
		EnsureMapSelectionVotesMatchPlayers(obj);
	}

	public static void RefreshMapSelectionForLoadedRun(object runManager, object runState)
	{
		object value = RunManagerMapSelectionSynchronizer.GetValue(runManager);
		if (value != null)
		{
			MapSelectionSynchronizerRunState.SetValue(value, runState);
			EndlessLog.Info(EnsureMapSelectionVotesMatchPlayers(value) ? "Refreshed map-selection run state after load and repaired vote collection size without clearing valid votes." : "Refreshed map-selection run state after load without clearing map votes.");
		}
	}

	public static void PrepareActChangeForFreshRun(object runManager, object runState)
	{
		object synchronizer = RunManagerActChangeSynchronizer.GetValue(runManager);
		if (synchronizer != null)
		{
			TrySetSynchronizerRunState(synchronizer, runState);
			int playersCount = GetPlayersCount(runState);
			ReplaceCollectionMemberWithBooleans(() => ActChangeSynchronizerReadyPlayers.GetValue(synchronizer), (object replacement) =>
			{
				ActChangeSynchronizerReadyPlayers.SetValue(synchronizer, replacement);
			}, ActChangeSynchronizerReadyPlayers.FieldType, playersCount, defaultValue: false);
		}
	}

	public static void ResetEventSynchronizerForFreshRun(object runManager, object runState)
	{
		object synchronizer = RunManagerEventSynchronizer.GetValue(runManager);
		if (synchronizer != null)
		{
			EndlessLog.Info("EventSynchronizer before endless reset: " + DescribeEventSynchronizerState(runManager, runState));
			int playersCount = GetPlayersCount(runState);
			TrySetSynchronizerPlayerCollection(synchronizer, runState);
			EventSynchronizerClearPlayerVotes.Invoke(synchronizer, Array.Empty<object>());
			ClearCollectionMember(() => EventSynchronizerEvents.GetValue(synchronizer), (object replacement) =>
			{
				EventSynchronizerEvents.SetValue(synchronizer, replacement);
			}, EventSynchronizerEvents.FieldType);
			ClearPendingEventOptionTasks(synchronizer);
			ResetCollectionMemberWithNulls(() => EventSynchronizerPlayerVotes.GetValue(synchronizer), (object replacement) =>
			{
				EventSynchronizerPlayerVotes.SetValue(synchronizer, replacement);
			}, EventSynchronizerPlayerVotes.FieldType, playersCount);
			EventSynchronizerCanonicalEvent.SetValue(synchronizer, null);
			EventSynchronizerPageIndex.SetValue(synchronizer, 0u);
			EndlessLog.Info("EventSynchronizer after endless reset: " + DescribeEventSynchronizerState(runManager, runState));
		}
	}

	public static bool TryPrimeEventSynchronizerForBeginEvent(object runManager, object runState, object canonicalEvent, out string stateSummary)
	{
		stateSummary = "synchronizer=null";
		object synchronizer = RunManagerEventSynchronizer.GetValue(runManager);
		if (synchronizer == null)
		{
			return false;
		}
		TrySetSynchronizerRunState(synchronizer, runState);
		TrySetSynchronizerPlayerCollection(synchronizer, runState);
		int playersCount = GetPlayersCount(runState);
		IList list = EventSynchronizerEvents.GetValue(synchronizer) as IList;
		object value = EventSynchronizerCanonicalEvent.GetValue(synchronizer);
		if (value != null && list != null && list.Count <= 0)
		{
			stateSummary = DescribeEventSynchronizerState(runManager, runState);
			return false;
		}
		EventSynchronizerClearPlayerVotes.Invoke(synchronizer, Array.Empty<object>());
		ClearCollectionMember(() => EventSynchronizerEvents.GetValue(synchronizer), (object replacement) =>
		{
			EventSynchronizerEvents.SetValue(synchronizer, replacement);
		}, EventSynchronizerEvents.FieldType);
		ClearPendingEventOptionTasks(synchronizer);
		ResetCollectionMemberWithNulls(() => EventSynchronizerPlayerVotes.GetValue(synchronizer), (object replacement) =>
		{
			EventSynchronizerPlayerVotes.SetValue(synchronizer, replacement);
		}, EventSynchronizerPlayerVotes.FieldType, playersCount);
		EventSynchronizerCanonicalEvent.SetValue(synchronizer, canonicalEvent);
		EventSynchronizerPageIndex.SetValue(synchronizer, 0u);
		stateSummary = DescribeEventSynchronizerState(runManager, runState);
		return true;
	}

	public static string DescribeEventSynchronizerState(object runManager, object? runState = null)
	{
		try
		{
			object value = RunManagerEventSynchronizer.GetValue(runManager);
			if (value == null)
			{
				return "synchronizer=null";
			}
			if (runState == null)
			{
				runState = RunManagerState.GetValue(runManager);
			}
			string value2 = DescribeIdentifier(EventSynchronizerCanonicalEvent.GetValue(value));
			string value3 = EventSynchronizerPageIndex.GetValue(value)?.ToString() ?? "<null>";
			string value4 = DescribeListContents(EventSynchronizerEvents.GetValue(value) as IList);
			string value5 = DescribeListContents(EventSynchronizerPlayerVotes.GetValue(value) as IList);
			string value6 = ((runState == null) ? "?" : GetCurrentActIndex(runState).ToString(CultureInfo.InvariantCulture));
			string value7 = ((runState == null) ? "?" : (Convert.ToString(RunStateActFloor.GetValue(runState), CultureInfo.InvariantCulture) ?? "<null>"));
			string value8 = ((runState == null) ? "?" : GetPlayersCount(runState).ToString(CultureInfo.InvariantCulture));
			return $"act={value6} floor={value7} players={value8} canonical={value2} page={value3} events={value4} votes={value5}";
		}
		catch (Exception ex)
		{
			return "failed-to-describe-event-sync:" + ex.GetType().Name;
		}
	}

	public static void ResetMultiplayerStateAfterEndlessLoad(object runManager)
	{
		ForceResetCombatStateForEndlessTransition();
		TryResetPlayerChoiceSynchronizer(runManager);
		TryResetRewardSynchronizer(runManager);
		TryResetTreasureRoomRelicSynchronizer(runManager);
		TryResetRewardsSetSynchronizer(runManager);
		TryResetRestSiteSynchronizer(runManager);
		TryResetOneOffSynchronizer(runManager);
		TryResetActChangeSynchronizer(runManager);
		TryResetActionQueueSynchronizer(runManager);
		TryResetCombatStateSynchronizer(runManager);
		TryRefreshRunLocationBuffer(runManager);
		TryResetRunLobbyReadyState(runManager);
	}

	private static void TryResetPlayerChoiceSynchronizer(object runManager)
	{
		object synchronizer = RunManagerPlayerChoiceSynchronizer.GetValue(runManager);
		if (synchronizer == null)
		{
			return;
		}
		object value = RunManagerState.GetValue(runManager);
		if (value != null)
		{
			TrySetSynchronizerPlayerCollection(synchronizer, value, PlayerChoiceSynchronizerPlayers);
			int playersCount = GetPlayersCount(value);
			List<uint> list = new List<uint>();
			for (int i = 0; i < playersCount; i++)
			{
				list.Add(0u);
			}
			if ((object)PlayerChoiceSynchronizerChoiceIds != null)
			{
				PlayerChoiceSynchronizerChoiceIds.SetValue(synchronizer, list);
			}
			else
			{
				TrySetMemberValue(synchronizer, "_choiceIds", list);
			}
			ClearCollectionMember(() => PlayerChoiceSynchronizerReceivedChoices?.GetValue(synchronizer), (object replacement) =>
			{
				PlayerChoiceSynchronizerReceivedChoices?.SetValue(synchronizer, replacement);
			}, PlayerChoiceSynchronizerReceivedChoices?.FieldType ?? typeof(List<object>));
			EndlessLog.Info($"Reset player-choice synchronizer for the endless map transition: players={playersCount} choiceIds={DescribeListContents(list)}.");
		}
	}

	private static void TryResetRewardSynchronizer(object runManager)
	{
		object value = RunManagerRewardSynchronizer.GetValue(runManager);
		if (value == null)
		{
			return;
		}
		object value2 = RunManagerState.GetValue(runManager);
		if (value2 != null)
		{
			TrySetSynchronizerPlayerCollection(value, value2, RewardSynchronizerPlayerCollection);
			if (RewardSynchronizerBufferedMessages?.GetValue(value) is IList { IsReadOnly: false } list)
			{
				list.Clear();
			}
			EndlessLog.Info("Reset reward synchronizer for the endless map transition.");
		}
	}

	private static void TryResetTreasureRoomRelicSynchronizer(object runManager)
	{
		object synchronizer = RunManagerTreasureRoomRelicSynchronizer.GetValue(runManager);
		if (synchronizer == null)
		{
			return;
		}
		object value = RunManagerState.GetValue(runManager);
		if (value == null)
		{
			return;
		}
		TrySetSynchronizerPlayerCollection(synchronizer, value, TreasureRoomRelicSynchronizerPlayerCollection);
		int playersCount = GetPlayersCount(value);
		if ((object)TreasureRoomRelicSynchronizerVotes != null)
		{
			ResetCollectionMemberWithNulls(() => TreasureRoomRelicSynchronizerVotes.GetValue(synchronizer), (object replacement) =>
			{
				TreasureRoomRelicSynchronizerVotes.SetValue(synchronizer, replacement);
			}, TreasureRoomRelicSynchronizerVotes.FieldType, playersCount);
		}
		TreasureRoomRelicSynchronizerPredictedVote?.SetValue(synchronizer, null);
		TreasureRoomRelicSynchronizerSinglePlayerSkipped?.SetValue(synchronizer, false);
		EndlessLog.Info($"Reset treasure-room relic synchronizer for the endless map transition: players={playersCount} votes={DescribeListContents(TreasureRoomRelicSynchronizerVotes?.GetValue(synchronizer) as IList)}.");
	}

	private static void TryResetRewardsSetSynchronizer(object runManager)
	{
		object? synchronizer = RunManagerRewardsSetSynchronizer?.GetValue(runManager);
		if (synchronizer == null)
		{
			return;
		}
		object? runState = RunManagerState.GetValue(runManager);
		if (runState == null)
		{
			return;
		}
		TrySetSynchronizerPlayerCollection(synchronizer, runState, RewardsSetSynchronizerPlayerCollection);
		if (RewardsSetSynchronizerRewardStates?.GetValue(synchronizer) is IList rewardStates)
		{
			rewardStates.Clear();
			int playersCount = GetPlayersCount(runState);
			for (int i = 0; i < playersCount; i++)
			{
				object? state = CreatePlayerRewardState();
				if (state != null)
				{
					rewardStates.Add(state);
				}
			}
			EndlessLog.Info($"Reset rewards-set synchronizer for the endless transition: players={playersCount} rewardStates={rewardStates.Count}.");
		}
	}

	private static void TryResetRestSiteSynchronizer(object runManager)
	{
		object? synchronizer = RunManagerRestSiteSynchronizer?.GetValue(runManager);
		if (synchronizer == null)
		{
			return;
		}
		object? runState = RunManagerState.GetValue(runManager);
		if (runState == null)
		{
			return;
		}
		TrySetSynchronizerPlayerCollection(synchronizer, runState, RestSiteSynchronizerPlayerCollection);
		if (RestSiteSynchronizerRestSites?.GetValue(synchronizer) is IList restSites)
		{
			restSites.Clear();
		}
		EndlessLog.Info("Reset rest-site synchronizer for the endless transition.");
	}

	private static void TryResetOneOffSynchronizer(object runManager)
	{
		object? synchronizer = RunManagerOneOffSynchronizer?.GetValue(runManager);
		if (synchronizer == null)
		{
			return;
		}
		object? runState = RunManagerState.GetValue(runManager);
		if (runState == null)
		{
			return;
		}
		TrySetSynchronizerPlayerCollection(synchronizer, runState, OneOffSynchronizerPlayerCollection);
		EndlessLog.Info("Reset one-off synchronizer for the endless transition.");
	}

	private static void TryResetActChangeSynchronizer(object runManager)
	{
		object? synchronizer = RunManagerActChangeSynchronizer?.GetValue(runManager);
		if (synchronizer == null)
		{
			return;
		}
		object? runState = RunManagerState.GetValue(runManager);
		if (runState == null)
		{
			return;
		}
		if (ActChangeSynchronizerRunState != null)
		{
			ActChangeSynchronizerRunState.SetValue(synchronizer, runState);
		}
		int playersCount = GetPlayersCount(runState);
		if (ActChangeSynchronizerReadyPlayers != null)
		{
			ReplaceCollectionMemberWithBooleans(() => ActChangeSynchronizerReadyPlayers.GetValue(synchronizer), (object replacement) =>
			{
				ActChangeSynchronizerReadyPlayers.SetValue(synchronizer, replacement);
			}, ActChangeSynchronizerReadyPlayers.FieldType, playersCount, defaultValue: false);
		}
		EndlessLog.Info($"Reset act-change synchronizer for the endless transition: players={playersCount}.");
	}

	public static object? GetRewardsSetSynchronizerPlayerCollection(object synchronizer)
	{
		return RewardsSetSynchronizerPlayerCollection?.GetValue(synchronizer);
	}

	public static void SetRewardsSetSynchronizerPlayerCollection(object synchronizer, object playerCollection)
	{
		TrySetSynchronizerPlayerCollection(synchronizer, playerCollection, RewardsSetSynchronizerPlayerCollection);
	}

	public static IList? GetRewardsSetSynchronizerRewardStates(object synchronizer)
	{
		return RewardsSetSynchronizerRewardStates?.GetValue(synchronizer) as IList;
	}

	public static object? CreatePlayerRewardState()
	{
		if (PlayerRewardStateType == null)
		{
			return null;
		}
		object state = Activator.CreateInstance(PlayerRewardStateType)!;
		if (RewardsSetStateType != null && PlayerRewardStateRewardsStack != null)
		{
			Type listType = typeof(List<>).MakeGenericType(RewardsSetStateType);
			PlayerRewardStateRewardsStack.SetValue(state, Activator.CreateInstance(listType));
		}
		if (BufferedMessageType != null && PlayerRewardStateBufferedMessages != null)
		{
			Type listType = typeof(List<>).MakeGenericType(BufferedMessageType);
			PlayerRewardStateBufferedMessages.SetValue(state, Activator.CreateInstance(listType));
		}
		if (PlayerRewardStateNextId != null)
		{
			PlayerRewardStateNextId.SetValue(state, 0);
		}
		return state;
	}

	public static void ForceResetCombatStateForEndlessTransition()
	{
		object value = CombatManagerInstance.GetValue(null);
		if (value != null)
		{
			CombatManagerReset.Invoke(value, new object[1] { false });
		}
	}

	public static bool IsLocalPlayerHost(object runManager)
	{
		object runLobby = GetRunLobby(runManager);
		if (runLobby == null)
		{
			return false;
		}
		if (TryGetBooleanMember(runLobby, "IsHost", out var value))
		{
			return value;
		}
		if (TryGetStringIdentifier(runLobby, out string identifier, "GetLobbyOwner", "LobbyOwnerId", "HostNetId", "OwnerId", "Owner") && TryGetStringIdentifier(runLobby, out string identifier2, "GetLocalPlayerId", "LocalPlayerId", "LocalPlayer", "NetClient"))
		{
			return string.Equals(identifier2, identifier, StringComparison.Ordinal);
		}
		if (TryIsLocalPlayerFirstRunPlayer(runManager, runLobby, out var isFirstRunPlayer))
		{
			return isFirstRunPlayer;
		}
		return false;
	}

	public static object? GetRunLobby(object runManager)
	{
		object value = RunManagerRunLobby.GetValue(runManager);
		if (value != null)
		{
			return value;
		}
		object gameInstance = GetGameInstance();
		string[] array = new string[6] { "RunLobby", "LoadRunLobby", "StartRunLobby", "_runLobby", "_loadRunLobby", "_startRunLobby" };
		foreach (string memberName in array)
		{
			if (TryGetMemberValue(gameInstance, memberName, out object value2) && value2 != null)
			{
				return value2;
			}
		}
		return null;
	}

	public static bool TrySetLocalPlayerReady(object runManager, bool isReady)
	{
		object runLobby = GetRunLobby(runManager);
		if (runLobby == null)
		{
			return false;
		}
		if (isReady && TryInvokeMethod(runLobby, "SetLocalPlayerReady", Array.Empty<object>(), out object result))
		{
			return true;
		}
		object lobbyPlayer;
		return TryInvokeMethod(runLobby, "SetLocalPlayerReady", new object[1] { isReady }, out result) || TryInvokeMethod(runLobby, "SetReady", new object[1] { isReady }, out result) || (TryGetLobbyLocalPlayer(runLobby, out lobbyPlayer) && TrySetLobbyPlayerReady(lobbyPlayer, isReady));
	}

	public static bool TryShouldAllowRunToBegin(object runManager, out bool shouldAllow)
	{
		shouldAllow = false;
		object runLobby = GetRunLobby(runManager);
		if (runLobby == null)
		{
			return false;
		}
		if (TryInvokeMethod(runLobby, "ShouldAllowRunToBegin", Array.Empty<object>(), out object result) && result is bool flag)
		{
			shouldAllow = flag;
			return true;
		}
		return false;
	}

	public static bool TryIsLocalPlayerReady(object runManager, out bool isReady)
	{
		isReady = false;
		object runLobby = GetRunLobby(runManager);
		if (runLobby == null)
		{
			return false;
		}
		if (TryGetLocalPlayerIdentifierValue(runLobby, out object identifierValue) && TryInvokeMethod(runLobby, "IsPlayerReady", new object[1] { identifierValue }, out object result) && result is bool flag)
		{
			isReady = flag;
			return true;
		}
		if (TryGetReadyPlayerIdentifiers(runLobby, out List<string> identifiers))
		{
			if (!TryGetLocalPlayerIdentifier(runLobby, out string identifier))
			{
				return false;
			}
			isReady = identifiers.Contains(identifier, StringComparer.Ordinal);
			return true;
		}
		if (TryGetLobbyLocalPlayer(runLobby, out object lobbyPlayer) && TryGetLobbyPlayerReady(lobbyPlayer, out var isReady2))
		{
			isReady = isReady2;
			return true;
		}
		return false;
	}

	private static bool TryIsLocalPlayerFirstRunPlayer(object runManager, object runLobby, out bool isFirstRunPlayer)
	{
		isFirstRunPlayer = false;
		try
		{
			if (!TryGetLobbyLocalPlayer(runLobby, out object lobbyPlayer) || !TryGetStringIdentifier(lobbyPlayer, out string identifier, "OwnerId", "PlayerId", "NetId", "Id"))
			{
				return false;
			}
			object value = RunManagerState.GetValue(runManager);
			if (value == null)
			{
				return false;
			}
			IReadOnlyList<object> runStatePlayers = GetRunStatePlayers(value);
			if (runStatePlayers.Count == 0 || !TryGetStringIdentifier(runStatePlayers[0], out string identifier2, "OwnerId", "PlayerId", "NetId", "Id"))
			{
				return false;
			}
			isFirstRunPlayer = string.Equals(identifier, identifier2, StringComparison.Ordinal);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool TryGetReadyPlayerCount(object runManager, out int readyCount)
	{
		readyCount = 0;
		object runLobby = GetRunLobby(runManager);
		if (runLobby == null)
		{
			return false;
		}
		if (!TryGetReadyPlayerIdentifiers(runLobby, out List<string> identifiers))
		{
			if (!TryGetLobbyPlayers(runLobby, out List<object> players))
			{
				return false;
			}
			readyCount = Enumerable.Count(players, (object player) => TryGetLobbyPlayerReady(player, out var isReady) & isReady);
			return true;
		}
		readyCount = identifiers.Count;
		return true;
	}

	public static bool TryIsHostReady(object runManager, out bool isReady)
	{
		isReady = false;
		object runLobby = GetRunLobby(runManager);
		if (runLobby == null)
		{
			return false;
		}
		if (TryGetStringIdentifier(runLobby, out string identifier, "GetLobbyOwner", "LobbyOwnerId", "HostNetId", "OwnerId", "Owner"))
		{
			if (TryGetReadyPlayerIdentifiers(runLobby, out List<string> identifiers))
			{
				isReady = identifiers.Contains(identifier, StringComparer.Ordinal);
				return true;
			}
			if (TryGetLobbyPlayers(runLobby, out List<object> players))
			{
				foreach (object item in players)
				{
					if (!TryGetStringIdentifier(item, out string identifier2, "OwnerId", "PlayerId", "NetId", "Id") || !string.Equals(identifier2, identifier, StringComparison.Ordinal) || !TryGetLobbyPlayerReady(item, out isReady))
					{
						continue;
					}
					return true;
				}
			}
		}
		return false;
	}

	public static bool EnsureMapSelectionVotesMatchPlayers(object synchronizer)
	{
		object value = MapSelectionSynchronizerRunState.GetValue(synchronizer);
		if (value == null)
		{
			return false;
		}
		int playersCount = GetPlayersCount(value);
		bool flag = EnsureCollectionMemberSize(synchronizer, () => MapSelectionSynchronizerVotes.GetValue(synchronizer), (object replacement) =>
		{
			MapSelectionSynchronizerVotes.SetValue(synchronizer, replacement);
		}, MapSelectionSynchronizerVotes.FieldType, playersCount);
		flag |= TryEnsureCollectionMemberSize(synchronizer, "_playerVotes", playersCount);
		flag |= TryEnsureCollectionMemberSize(synchronizer, "playerVotes", playersCount);
		return flag | TryEnsureCollectionMemberSize(synchronizer, "PlayerVotes", playersCount);
	}

	public static object GetMapSelectionRunState(object synchronizer)
	{
		return MapSelectionSynchronizerRunState.GetValue(synchronizer) ?? throw new InvalidOperationException("MapSelectionSynchronizer._runState returned null.");
	}

	public static IReadOnlyList<object> GetRunStatePlayers(object runState)
	{
		IEnumerable source = (RunStatePlayers.GetValue(runState) as IEnumerable) ?? throw new InvalidOperationException("RunState.Players returned null.");
		return (from object player in source
			where player != null
			select player).ToList();
	}

	public static int GetPlayerSlotIndex(object runState, object player)
	{
		return (int)(RunStateGetPlayerSlotIndex.Invoke(runState, new object[1] { player }) ?? throw new InvalidOperationException("RunState.GetPlayerSlotIndex returned null."));
	}

	public static bool TryRemapPlayerToCurrentRun(object runState, object player, out object remappedPlayer)
	{
		foreach (object runStatePlayer in GetRunStatePlayers(runState))
		{
			if (runStatePlayer == player)
			{
				remappedPlayer = runStatePlayer;
				return true;
			}
		}
		if (TryGetStringIdentifier(player, out string identifier, "OwnerId", "PlayerId", "NetId", "Id"))
		{
			foreach (object runStatePlayer2 in GetRunStatePlayers(runState))
			{
				if (TryGetStringIdentifier(runStatePlayer2, out string identifier2, "OwnerId", "PlayerId", "NetId", "Id") && string.Equals(identifier2, identifier, StringComparison.Ordinal))
				{
					remappedPlayer = runStatePlayer2;
					return true;
				}
			}
		}
		remappedPlayer = player;
		return false;
	}

	public static bool RelicInventoryHasHolderForRelic(object inventory, object relic)
	{
		IEnumerable enumerable = (IEnumerable)(NRelicInventoryRelicNodes.GetValue(inventory) ?? throw new InvalidOperationException("NRelicInventory.RelicNodes returned null."));
		if (enumerable == null)
		{
			return false;
		}
		foreach (object item in enumerable)
		{
			if (item == null || NRelicInventoryHolderModel.GetValue(item) != relic)
			{
				continue;
			}
			return true;
		}
		return false;
	}

	public static int GetRelicInventoryNodeCount(object inventory)
	{
		IEnumerable enumerable = (IEnumerable)(NRelicInventoryRelicNodes.GetValue(inventory) ?? throw new InvalidOperationException("NRelicInventory.RelicNodes returned null."));
		if (!(enumerable is ICollection { Count: var count }))
		{
			int num = 0;
			if (enumerable != null)
			{
				foreach (object item in enumerable)
				{
					num++;
				}
			}
			return num;
		}
		return count;
	}

	public static void AddRelicInventoryNode(object inventory, object relic, bool startsShown, int index)
	{
		NRelicInventoryAdd.Invoke(inventory, new object[3] { relic, startsShown, index });
	}

	public static IReadOnlyList<Control> GetRelicInventoryHolders(object inventory)
	{
		IEnumerable enumerable = (IEnumerable)(NRelicInventoryRelicNodesField.GetValue(inventory) ?? NRelicInventoryRelicNodes.GetValue(inventory));
		if (enumerable == null)
		{
			throw new InvalidOperationException("NRelicInventory relic node storage returned null.");
		}
		return (from holder in enumerable.Cast<object>().OfType<Control>()
			where GodotObject.IsInstanceValid((GodotObject)(object)holder)
			select holder).ToList();
	}

	public static object? GetRelicInventoryHolderModel(object holder)
	{
		return NRelicInventoryHolderModel.GetValue(holder);
	}

	public static string GetRelicInventoryHolderAmountText(object holder)
	{
		object value = NRelicInventoryHolderAmountLabel.GetValue(holder);
		if (value == null)
		{
			return string.Empty;
		}
		CanvasItem val = (CanvasItem)((value is CanvasItem) ? value : null);
		if (val != null && !val.Visible)
		{
			return string.Empty;
		}
		if (!(AccessTools.Property(value.GetType(), "Text")?.GetValue(value) is string text))
		{
			return string.Empty;
		}
		return text.Trim();
	}

	public static void SetRelicInventoryHolderAmountText(object holder, string text)
	{
		object value = NRelicInventoryHolderAmountLabel.GetValue(holder);
		if (value != null)
		{
			PropertyInfo propertyInfo = AccessTools.Property(value.GetType(), "Text");
			if ((object)propertyInfo != null && propertyInfo.CanWrite)
			{
				propertyInfo.SetValue(value, text);
			}
		}
	}

	public static void RefreshRelicCounterDisplay(object holder)
	{
		if (TryGetFormattedRelicCounterText(holder, out string formattedText))
		{
			SetRelicInventoryHolderAmountText(holder, formattedText);
		}
	}

	public static bool TryGetFormattedRelicCounterText(object holder, out string formattedText)
	{
		formattedText = string.Empty;
		object value = NRelicInventoryHolderModel.GetValue(holder);
		if (value == null || !TryGetRelicDisplayAmount(value, out var amount) || !TryGetRelicCounterMaximum(value, out var maximum) || maximum <= 0)
		{
			return false;
		}
		formattedText = $"{amount}/{maximum}";
		return true;
	}

	public static void SetRelicInventoryHolderOrder(object inventory, IReadOnlyList<Control> orderedHolders)
	{
		IList list = (IList)(NRelicInventoryRelicNodesField.GetValue(inventory) ?? NRelicInventoryRelicNodes.GetValue(inventory) ?? throw new InvalidOperationException("NRelicInventory relic node storage returned null."));
		if (list == null)
		{
			throw new InvalidOperationException("NRelicInventory relic node storage is not mutable.");
		}
		list.Clear();
		foreach (Control orderedHolder in orderedHolders)
		{
			list.Add(orderedHolder);
		}
		Node val = (Node)((inventory is Node) ? inventory : null);
		if (val != null)
		{
			for (int i = 0; i < orderedHolders.Count; i++)
			{
				Control val2 = orderedHolders[i];
				if (GodotObject.IsInstanceValid((GodotObject)(object)val2) && ((Node)val2).GetParent() == val)
				{
					val.MoveChild((Node)(object)val2, i);
				}
			}
		}
		UpdateRelicInventoryNavigation(inventory);
	}

	public static void UpdateRelicInventoryNavigation(object inventory)
	{
		NRelicInventoryUpdateNavigation.Invoke(inventory, Array.Empty<object>());
	}

	public static bool IsMonsterCreature(object creature)
	{
		return (bool)(CreatureIsMonster.GetValue(creature) ?? throw new InvalidOperationException("Creature.IsMonster returned null."));
	}

	public static int GetCreatureCurrentHp(object creature)
	{
		return (int)(CreatureCurrentHp.GetValue(creature) ?? throw new InvalidOperationException("Creature.CurrentHp returned null."));
	}

	public static int GetCreatureMaxHp(object creature)
	{
		return (int)(CreatureMaxHp.GetValue(creature) ?? throw new InvalidOperationException("Creature.MaxHp returned null."));
	}

	public static int? GetCreatureMonsterMaxHpBeforeModification(object creature)
	{
		return (int?)CreatureMonsterMaxHpBeforeModification.GetValue(creature);
	}

	public static void SetCreatureMonsterMaxHpBeforeModification(object creature, int? value)
	{
		CreatureMonsterMaxHpBeforeModification.SetValue(creature, value);
	}

	public static void SetCreatureCurrentHpInternal(object creature, int value)
	{
		CreatureSetCurrentHpInternal.Invoke(creature, new object[1] { (decimal)value });
	}

	public static void SetCreatureMaxHpInternal(object creature, int value)
	{
		CreatureSetMaxHpInternal.Invoke(creature, new object[1] { (decimal)value });
	}

	public static object GetCombatStateRunState(object combatState)
	{
		return CombatStateRunState.GetValue(combatState) ?? throw new InvalidOperationException("CombatState.RunState returned null.");
	}

	public static object? GetCreatureCombatState(object creature)
	{
		return CreatureCombatState.GetValue(creature);
	}

	public static object? GetAttackCommandAttacker(object attackCommand)
	{
		return AttackCommandAttacker.GetValue(attackCommand);
	}

	public static decimal GetAttackCommandDamagePerHit(object attackCommand)
	{
		return (decimal)(AttackCommandDamagePerHit.GetValue(attackCommand) ?? throw new InvalidOperationException("AttackCommand._damagePerHit returned null."));
	}

	public static void SetAttackCommandDamagePerHit(object attackCommand, decimal amount)
	{
		AttackCommandDamagePerHit.SetValue(attackCommand, amount);
	}

	public static bool IsAttackIntent(object intent)
	{
		return AttackIntentType.IsInstanceOfType(intent);
	}

	public static Func<decimal>? GetAttackIntentDamageCalc(object intent)
	{
		return AttackIntentDamageCalc.GetValue(intent) as Func<decimal>;
	}

	public static void SetAttackIntentDamageCalc(object intent, Func<decimal> damageCalc)
	{
		AttackIntentDamageCalc.SetValue(intent, damageCalc);
	}

	public static object CreateSaveSnapshot(object runManager)
	{
		return RunManagerToSave.Invoke(runManager, new object[1]) ?? throw new InvalidOperationException("RunManager.ToSave returned null.");
	}

	public static object CreateRunStateFromSave(object runManager, object save)
	{
		return RunStateFromSerializable.Invoke(null, new object[1] { save }) ?? throw new InvalidOperationException("RunState.FromSerializable returned null.");
	}

	public static void InitializeSavedRun(object runManager, object save)
	{
		RunManagerInitializeSavedRun.Invoke(runManager, new object[1] { save });
	}

	public static void ClearSerializableSeenEvents(object save)
	{
		object value = Activator.CreateInstance(SerializableRunEventsSeen.PropertyType) ?? throw new InvalidOperationException("Could not create replacement EventsSeen collection.");
		SerializableRunEventsSeen.SetValue(save, value);
	}

	public static void ClearVisitedEventIds(object runState)
	{
		object value = Activator.CreateInstance(RunStateVisitedEventIds.FieldType) ?? throw new InvalidOperationException("Could not create replacement visited-event set.");
		RunStateVisitedEventIds.SetValue(runState, value);
	}

	public static void RefreshSharedRelicGrabBag(object runState)
	{
		IReadOnlyList<object> runStatePlayers = GetRunStatePlayers(runState);
		if (runStatePlayers.Count != 0)
		{
			object obj = RunStateRng.GetValue(runState) ?? throw new InvalidOperationException("RunState.Rng returned null.");
			object obj2 = RunRngSetTreasureRoomRelics.GetValue(obj) ?? throw new InvalidOperationException("RunRngSet.TreasureRoomRelics returned null.");
			object obj3 = Activator.CreateInstance(RelicGrabBagType) ?? throw new InvalidOperationException("Could not create a fresh RelicGrabBag.");
			RelicGrabBagPopulateForPlayer.Invoke(obj3, new object[2]
			{
				runStatePlayers[0],
				obj2
			});
			RunStateSharedRelicGrabBag.SetValue(runState, obj3);
		}
	}

	public static object? GetPreFinishedRoom(object save)
	{
		return SerializableRunPreFinishedRoom.GetValue(save);
	}

	public static void SetPreFinishedRoom(object save, object? preFinishedRoom)
	{
		SerializableRunPreFinishedRoom.SetValue(save, preFinishedRoom);
	}

	public static void SetSerializableRunWinTime(object save, long value)
	{
		SerializableRunWinTime.SetValue(save, value);
	}

	public static int ReviveSerializablePlayers(object save)
	{
		IEnumerable enumerable = (SerializableRunPlayers.GetValue(save) as IEnumerable) ?? throw new InvalidOperationException("SerializableRun.Players returned null.");
		int num = 0;
		foreach (object item in enumerable)
		{
			if (item != null)
			{
				int num2 = (int)(SerializablePlayerCurrentHp.GetValue(item) ?? throw new InvalidOperationException("SerializablePlayer.CurrentHp returned null."));
				int val = (int)(SerializablePlayerMaxHp.GetValue(item) ?? throw new InvalidOperationException("SerializablePlayer.MaxHp returned null."));
				if (num2 <= 0)
				{
					SerializablePlayerCurrentHp.SetValue(item, Math.Max(1, val));
					num++;
				}
			}
		}
		return num;
	}

	public static void SetShouldSave(object runManager, bool value)
	{
		RunManagerShouldSave.SetValue(runManager, value);
	}

	public static void SetIsCleaningUp(object runManager, bool value)
	{
		RunManagerIsCleaningUp.SetValue(runManager, value);
	}

	public static void SetIsAbandoned(object runManager, bool value)
	{
		RunManagerIsAbandoned.SetValue(runManager, value);
	}

	public static void ClearScreens(object runManager)
	{
		RunManagerClearScreens.Invoke(runManager, Array.Empty<object>());
	}

	public static void QueueFreeLingeringCombatNodes()
	{
		object gameInstance = GetGameInstance();
		Node val = (Node)((gameInstance is Node) ? gameInstance : null);
		if (val == null)
		{
			return;
		}
		List<Node> list = EnumerateDescendants(val).Where(IsLingeringCombatNode).Distinct().ToList();
		foreach (Node item in list)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)item) && !((GodotObject)item).IsQueuedForDeletion())
			{
				item.QueueFree();
			}
		}
		if (list.Count > 0)
		{
			EndlessLog.Info($"Queued {list.Count} lingering combat scene node(s) for disposal before loading the endless run.");
		}
	}

	public static void ClearVisitedMapCoords(object runState)
	{
		RunStateClearVisitedMapCoordsDebug.Invoke(runState, Array.Empty<object>());
	}

	public static Task InvokeSetActInternalAsync(object runManager, int actIndex)
	{
		return (Task)(RunManagerSetActInternal.Invoke(runManager, new object[1] { actIndex }) ?? throw new InvalidOperationException("RunManager.SetActInternal returned null."));
	}

	public static Task InvokeEnterNextActAsync(object runManager)
	{
		return (Task)(RunManagerEnterNextAct.Invoke(runManager, Array.Empty<object>()) ?? throw new InvalidOperationException("RunManager.EnterNextAct returned null."));
	}

	public static Task InvokeLoadRunAsync(object game, object runState, object? preFinishedRoom)
	{
		return (Task)(NGameLoadRun.Invoke(game, new object[2] { runState, preFinishedRoom }) ?? throw new InvalidOperationException("NGame.LoadRun returned null."));
	}

	private static IEnumerable<Node> EnumerateDescendants(Node node)
	{
		foreach (Node child in ((IEnumerable)node.GetChildren(false)).OfType<Node>())
		{
			yield return child;
			foreach (Node item in EnumerateDescendants(child))
			{
				yield return item;
			}
		}
	}

	private static bool IsLingeringCombatNode(Node node)
	{
		string text = ((object)node).GetType().FullName ?? ((object)node).GetType().Name;
		string text2 = ((object)node.Name).ToString();
		return text.Equals("MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom", StringComparison.Ordinal) || text.Equals("MegaCrit.Sts2.Core.Nodes.Combat.NCardPlayQueue", StringComparison.Ordinal) || text2.Equals("NCombatRoom", StringComparison.Ordinal) || text2.Equals("NCardPlayQueue", StringComparison.Ordinal);
	}

	private static bool TryGetBooleanMember(object target, string memberName, out bool value)
	{
		value = false;
		PropertyInfo propertyInfo = AccessTools.Property(target.GetType(), memberName);
		if ((object)propertyInfo != null && propertyInfo.GetValue(target) is bool flag)
		{
			value = flag;
			return true;
		}
		MethodInfo methodInfo = AccessTools.Method(target.GetType(), memberName, Type.EmptyTypes, (Type[])null);
		if ((object)methodInfo != null && methodInfo.Invoke(target, Array.Empty<object>()) is bool flag2)
		{
			value = flag2;
			return true;
		}
		FieldInfo fieldInfo = AccessTools.Field(target.GetType(), memberName);
		if ((object)fieldInfo != null && fieldInfo.GetValue(target) is bool flag3)
		{
			value = flag3;
			return true;
		}
		return false;
	}

	private static bool IsRoomSetNotGeneratedException(Exception exception)
	{
		while (exception is TargetInvocationException { InnerException: not null } ex)
		{
			exception = ex.InnerException;
		}
		return exception is InvalidOperationException ex2 && ex2.Message.Contains("RoomSet.", StringComparison.Ordinal) && ex2.Message.Contains("not set!", StringComparison.Ordinal) && ex2.Message.Contains("GenerateRooms", StringComparison.Ordinal);
	}

	private static bool TryGetStringIdentifier(object target, out string identifier, params string[] memberNames)
	{
		foreach (string memberName in memberNames)
		{
			if (TryGetMemberValue(target, memberName, out object value) && value != null && TryConvertIdentifier(value, out identifier))
			{
				return true;
			}
		}
		identifier = string.Empty;
		return false;
	}

	private static bool TryGetLocalPlayerIdentifier(object runLobby, out string identifier)
	{
		return TryGetStringIdentifier(runLobby, out identifier, "GetLocalPlayerId", "LocalPlayerId", "LocalPlayer");
	}

	private static bool TryGetLobbyLocalPlayer(object runLobby, out object lobbyPlayer)
	{
		string[] array = new string[4] { "GetLocalPlayer", "LocalPlayer", "_localPlayer", "_displayLocalPlayer" };
		foreach (string memberName in array)
		{
			if (TryGetMemberValue(runLobby, memberName, out object value) && value != null)
			{
				lobbyPlayer = value;
				return true;
			}
		}
		if (!TryGetLocalPlayerIdentifier(runLobby, out string identifier))
		{
			lobbyPlayer = null;
			return false;
		}
		if (TryGetLobbyPlayers(runLobby, out List<object> players))
		{
			foreach (object item in players)
			{
				if (TryGetStringIdentifier(item, out string identifier2, "OwnerId", "PlayerId", "NetId", "Id") && string.Equals(identifier2, identifier, StringComparison.Ordinal))
				{
					lobbyPlayer = item;
					return true;
				}
			}
		}
		lobbyPlayer = null;
		return false;
	}

	private static bool TryGetLobbyPlayers(object runLobby, out List<object> players)
	{
		players = new List<object>();
		string[] array = new string[4] { "Players", "_players", "AllPlayers", "_allPlayers" };
		foreach (string memberName in array)
		{
			if (!TryGetMemberValue(runLobby, memberName, out object value) || !(value is IEnumerable enumerable))
			{
				continue;
			}
			foreach (object item in enumerable)
			{
				if (item != null)
				{
					players.Add(item);
				}
			}
			if (players.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	private static bool TryGetLobbyPlayerReady(object player, out bool isReady)
	{
		if (TryGetBooleanMember(player, "IsReady", out isReady))
		{
			return true;
		}
		if (TryGetBooleanMember(player, "_isReady", out isReady))
		{
			return true;
		}
		return false;
	}

	private static bool TrySetLobbyPlayerReady(object player, bool isReady)
	{
		object result;
		return TryInvokeMethod(player, "SetReady", new object[1] { isReady }, out result) || TryInvokeMethod(player, "SetIsReady", new object[1] { isReady }, out result) || TrySetMemberValue(player, "IsReady", isReady) || TrySetMemberValue(player, "_isReady", isReady);
	}

	private static bool TryGetLocalPlayerIdentifierValue(object runLobby, out object? identifierValue)
	{
		string[] array = new string[3] { "GetLocalPlayerId", "LocalPlayerId", "LocalPlayer" };
		foreach (string memberName in array)
		{
			if (TryGetMemberValue(runLobby, memberName, out identifierValue) && identifierValue != null)
			{
				return true;
			}
		}
		identifierValue = null;
		return false;
	}

	private static bool TryGetReadyPlayerIdentifiers(object runLobby, out List<string> identifiers)
	{
		identifiers = new List<string>();
		string[] array = new string[3] { "_readyPlayers", "readyPlayers", "ReadyPlayers" };
		foreach (string memberName in array)
		{
			if (TryGetMemberValue(runLobby, memberName, out object value) && value != null && TryExtractIdentifiersFromCollection(value, identifiers))
			{
				identifiers = identifiers.Where((string id) => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).ToList();
				return true;
			}
		}
		return false;
	}

	private static bool TryGetMemberValue(object target, string memberName, out object? value)
	{
		PropertyInfo propertyInfo = AccessTools.Property(target.GetType(), memberName);
		if ((object)propertyInfo != null)
		{
			value = propertyInfo.GetValue(target);
			return true;
		}
		MethodInfo methodInfo = AccessTools.Method(target.GetType(), memberName, Type.EmptyTypes, (Type[])null);
		if ((object)methodInfo != null)
		{
			value = methodInfo.Invoke(target, Array.Empty<object>());
			return true;
		}
		FieldInfo fieldInfo = AccessTools.Field(target.GetType(), memberName);
		if ((object)fieldInfo != null)
		{
			value = fieldInfo.GetValue(target);
			return true;
		}
		value = null;
		return false;
	}

	private static bool TryInvokeMethod(object target, string methodName, object?[] args, out object? result)
	{
		MethodInfo[] array = (from method in target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where string.Equals(method.Name, methodName, StringComparison.Ordinal)
			select method).ToArray();
		MethodInfo[] array2 = array;
		foreach (MethodInfo methodInfo in array2)
		{
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length != args.Length)
			{
				continue;
			}
			object[] array3 = new object[args.Length];
			bool flag = true;
			for (int num2 = 0; num2 < args.Length; num2++)
			{
				if (!TryPrepareArgument(parameters[num2].ParameterType, args[num2], out array3[num2]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				result = methodInfo.Invoke(target, array3);
				return true;
			}
		}
		result = null;
		return false;
	}

	private static bool TryPrepareArgument(Type parameterType, object? value, out object? preparedValue)
	{
		if (value == null)
		{
			preparedValue = null;
			return !parameterType.IsValueType || (object)Nullable.GetUnderlyingType(parameterType) != null;
		}
		if (parameterType.IsInstanceOfType(value))
		{
			preparedValue = value;
			return true;
		}
		Type type = Nullable.GetUnderlyingType(parameterType) ?? parameterType;
		if (type.IsEnum && value is string value2)
		{
			try
			{
				preparedValue = Enum.Parse(type, value2, ignoreCase: false);
				return true;
			}
			catch
			{
				try
				{
					preparedValue = Enum.Parse(type, value2, ignoreCase: true);
					return true;
				}
				catch
				{
					preparedValue = null;
					return false;
				}
			}
		}
		try
		{
			preparedValue = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
			return true;
		}
		catch
		{
			preparedValue = null;
			return false;
		}
	}

	private static bool TryExtractIdentifiersFromCollection(object collectionObject, List<string> identifiers)
	{
		if (!(collectionObject is IDictionary dictionary))
		{
			if (collectionObject is IEnumerable enumerable)
			{
				foreach (object item in enumerable)
				{
					if (item != null && TryConvertIdentifier(item, out string identifier))
					{
						identifiers.Add(identifier);
					}
				}
				return true;
			}
			return false;
		}
		foreach (DictionaryEntry item2 in dictionary)
		{
			string identifier3;
			if (TryConvertIdentifier(item2.Key, out string identifier2))
			{
				identifiers.Add(identifier2);
			}
			else if (TryConvertIdentifier(item2.Value, out identifier3))
			{
				identifiers.Add(identifier3);
			}
		}
		return true;
	}

	private static bool TryConvertIdentifier(object value, out string identifier)
	{
		if (value != null)
		{
			if (!(value is string text))
			{
				if (value is char c)
				{
					identifier = c.ToString();
					return true;
				}
				if (value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong)
				{
					identifier = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
					return identifier.Length > 0;
				}
			}
			else if (!string.IsNullOrWhiteSpace(text))
			{
				identifier = text;
				return true;
			}
			string[] array = new string[4] { "OwnerId", "PlayerId", "NetId", "Id" };
			foreach (string memberName in array)
			{
				if (TryGetMemberValue(value, memberName, out object value2) && value2 != null && TryConvertIdentifier(value2, out identifier))
				{
					return true;
				}
			}
			identifier = value.ToString() ?? string.Empty;
			return identifier.Length > 0;
		}
		identifier = string.Empty;
		return false;
	}

	private static bool TryGetRelicDisplayAmount(object relic, out int amount)
	{
		amount = 0;
		if (relic == null || !(RelicModelShowCounter.GetValue(relic) is bool flag) || !flag)
		{
			return false;
		}
		amount = (int)(RelicModelDisplayAmount.GetValue(relic) ?? throw new InvalidOperationException("RelicModel.DisplayAmount returned null."));
		return true;
	}

	private static bool TryGetRelicCounterMaximum(object relic, out int maximum)
	{
		maximum = 0;
		string[] array = new string[7] { "turnsThreshold", "_turnsThreshold", "_attacksThreshold", "attacksThreshold", "_threshold", "threshold", "Threshold" };
		foreach (string memberName in array)
		{
			if (TryGetIntMember(relic, memberName, out maximum) && maximum > 0)
			{
				return true;
			}
		}
		maximum = GetKnownRelicCounterMaximum(relic.GetType().Name);
		return maximum > 0;
	}

	private static int GetKnownRelicCounterMaximum(string relicTypeName)
	{
		return relicTypeName switch
		{
			"HappyFlower" => 3, 
			"FakeHappyFlower" => 3, 
			"PenNib" => 10, 
			"Nunchaku" => 10, 
			"InkBottle" => 10, 
			"Sundial" => 3, 
			"IncenseBurner" => 6, 
			_ => 0, 
		};
	}

	private static void ResetMapSelectionVoteCollections(object synchronizer, int playerCount)
	{
		ReplaceCollectionMemberWithNulls(() => MapSelectionSynchronizerVotes.GetValue(synchronizer), (object replacement) =>
		{
			MapSelectionSynchronizerVotes.SetValue(synchronizer, replacement);
		}, MapSelectionSynchronizerVotes.FieldType, playerCount);
		TryReplaceCollectionMemberWithNulls(synchronizer, "_playerVotes", playerCount);
		TryReplaceCollectionMemberWithNulls(synchronizer, "playerVotes", playerCount);
		TryReplaceCollectionMemberWithNulls(synchronizer, "PlayerVotes", playerCount);
	}

	private static void TryResetRunLobbyReadyState(object runManager)
	{
		object runLobby = GetRunLobby(runManager);
		if (runLobby == null)
		{
			return;
		}
		TryInvokeMethod(runLobby, "ClearReadyPlayers", Array.Empty<object>(), out object result);
		TryInvokeMethod(runLobby, "ResetReadyPlayers", Array.Empty<object>(), out result);
		TryResetCollectionMemberToEmpty(runLobby, "_readyPlayers");
		TryResetCollectionMemberToEmpty(runLobby, "readyPlayers");
		TryResetCollectionMemberToEmpty(runLobby, "ReadyPlayers");
		if (!TryGetLobbyPlayers(runLobby, out List<object> players))
		{
			return;
		}
		foreach (object item in players)
		{
			TrySetLobbyPlayerReady(item, isReady: false);
		}
	}

	private static void TryResetActionQueueSynchronizer(object runManager)
	{
		object value = RunManagerActionQueueSynchronizer.GetValue(runManager);
		if (value != null)
		{
			TryInvokeMethod(value, "ResetCombatState", Array.Empty<object>(), out object result);
			TrySetSynchronizerCombatState(value, "NotInCombat");
			TryInvokeMethod(value, "UnpauseAllPlayerQueues", Array.Empty<object>(), out result);
		}
	}

	private static void TryResetCombatStateSynchronizer(object runManager)
	{
		object value = RunManagerCombatStateSynchronizer.GetValue(runManager);
		if (value != null)
		{
			TryInvokeMethod(value, "ResetCombatState", Array.Empty<object>(), out object _);
			TrySetSynchronizerCombatState(value, "NotInCombat");
		}
	}

	private static void TrySetSynchronizerCombatState(object synchronizer, string stateName)
	{
		object result;
		try
		{
			object obj = Enum.Parse(ActionSynchronizerCombatStateType, stateName);
			TryInvokeMethod(synchronizer, "SetCombatState", new object[1] { obj }, out result);
		}
		catch
		{
			TryInvokeMethod(synchronizer, "SetCombatState", new object[1] { stateName }, out result);
		}
	}

	private static void ReplaceCombatManagerCollection(object combatManager, FieldInfo field)
	{
		object obj = Activator.CreateInstance(field.FieldType);
		object result;
		if (obj != null)
		{
			field.SetValue(combatManager, obj);
		}
		else if (field.GetValue(combatManager) is IList list)
		{
			list.Clear();
		}
		else if (field.GetValue(combatManager) != null && !TryInvokeMethod(field.GetValue(combatManager), "Clear", Array.Empty<object>(), out result))
		{
		}
	}

	private static void TryResetCombatManagerPendingLoss(object combatManager)
	{
		try
		{
			object obj = Activator.CreateInstance(CombatManagerPendingLoss.FieldType);
			if (obj != null)
			{
				CombatManagerPendingLoss.SetValue(combatManager, obj);
				return;
			}
		}
		catch
		{
		}
		object value = CombatManagerPendingLoss.GetValue(combatManager);
		if (value != null && !TryInvokeMethod(value, "Reset", Array.Empty<object>(), out object result) && !TryInvokeMethod(value, "Clear", Array.Empty<object>(), out result))
		{
			TrySetMemberValue(value, "Encounter", null);
			TrySetMemberValue(value, "Killer", null);
			TrySetMemberValue(value, "Player", null);
			TrySetMemberValue(value, "Character", null);
			TrySetMemberValue(value, "IsPending", false);
			TrySetMemberValue(value, "Pending", false);
			TrySetMemberValue(value, "ShouldLose", false);
			TrySetMemberValue(value, "_isPending", false);
			TrySetMemberValue(value, "_pending", false);
		}
	}

	private static void TryRefreshRunLocationBuffer(object runManager)
	{
		object value = RunManagerRunLocationTargetedBuffer.GetValue(runManager);
		if (value != null && TryGetMemberValue(runManager, "RunLocation", out object value2) && value2 != null && !TryInvokeMethod(value, "OnRunLocationChanged", new object[1] { value2 }, out object result) && !TryInvokeMethod(value, "AfterLocationChanged", new object[1] { value2 }, out result))
		{
			TrySetMemberValue(value, "CurrentLocation", value2);
			TrySetMemberValue(value, "Location", value2);
		}
	}

	private static bool TrySetMemberValue(object target, string memberName, object? value)
	{
		PropertyInfo propertyInfo = AccessTools.Property(target.GetType(), memberName);
		if ((object)propertyInfo != null)
		{
			if (!TryPrepareArgument(propertyInfo.PropertyType, value, out object preparedValue))
			{
				return false;
			}
			propertyInfo.SetValue(target, preparedValue);
			return true;
		}
		FieldInfo fieldInfo = AccessTools.Field(target.GetType(), memberName);
		if ((object)fieldInfo != null)
		{
			if (!TryPrepareArgument(fieldInfo.FieldType, value, out object preparedValue2))
			{
				return false;
			}
			fieldInfo.SetValue(target, preparedValue2);
			return true;
		}
		return false;
	}

	private static void TrySetSynchronizerRunState(object synchronizer, object runState)
	{
		TrySetMemberValue(synchronizer, "_runState", runState);
		TrySetMemberValue(synchronizer, "runState", runState);
		TrySetMemberValue(synchronizer, "RunState", runState);
	}

	private static void TrySetSynchronizerPlayerCollection(object synchronizer, object runState)
	{
		TrySetSynchronizerPlayerCollection(synchronizer, runState, EventSynchronizerPlayerCollection);
	}

	private static void TrySetSynchronizerPlayerCollection(object synchronizer, object runState, FieldInfo? playerCollectionField)
	{
		if ((object)playerCollectionField == null)
		{
			return;
		}
		Type fieldType = playerCollectionField.FieldType;
		if (fieldType.IsInstanceOfType(runState))
		{
			playerCollectionField.SetValue(synchronizer, runState);
			return;
		}
		object value = RunStatePlayers.GetValue(runState);
		if (value != null && fieldType.IsInstanceOfType(value))
		{
			playerCollectionField.SetValue(synchronizer, value);
		}
	}

	private static bool TryGetIntMember(object target, string memberName, out int value)
	{
		value = 0;
		if (!TryGetMemberValue(target, memberName, out object value2) || value2 == null)
		{
			return false;
		}
		try
		{
			value = Convert.ToInt32(value2, CultureInfo.InvariantCulture);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static void ReplaceCollectionMemberWithNulls(Func<object?> valueAccessor, Action<object> valueSetter, Type collectionType, int expectedCount)
	{
		IList list = (IList)(Activator.CreateInstance(collectionType) ?? throw new InvalidOperationException("Could not create replacement collection for " + collectionType.FullName + "."));
		for (int i = 0; i < expectedCount; i++)
		{
			list.Add(null);
		}
		valueSetter(list);
	}

	private static void ClearCollectionMember(Func<object?> valueAccessor, Action<object> valueSetter, Type collectionType)
	{
		if (valueAccessor() is IList { IsReadOnly: false } list)
		{
			list.Clear();
			return;
		}
		IList obj = (IList)(Activator.CreateInstance(collectionType) ?? throw new InvalidOperationException("Could not create replacement collection for " + collectionType.FullName + "."));
		valueSetter(obj);
	}

	private static void ClearPendingEventOptionTasks(object synchronizer)
	{
		if (EventSynchronizerPendingOptionTasks?.GetValue(synchronizer) is IList { IsReadOnly: false } list)
		{
			list.Clear();
		}
	}

	private static void ResetCollectionMemberWithNulls(Func<object?> valueAccessor, Action<object> valueSetter, Type collectionType, int expectedCount)
	{
		if (valueAccessor() is IList { IsReadOnly: false } list && TryResizeList(list, expectedCount))
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = null;
			}
		}
		else
		{
			ReplaceCollectionMemberWithNulls(valueAccessor, valueSetter, collectionType, expectedCount);
		}
	}

	private static void ReplaceCollectionMemberWithBooleans(Func<object?> valueAccessor, Action<object> valueSetter, Type collectionType, int expectedCount, bool defaultValue)
	{
		IList list = (IList)(Activator.CreateInstance(collectionType) ?? throw new InvalidOperationException("Could not create replacement collection for " + collectionType.FullName + "."));
		for (int i = 0; i < expectedCount; i++)
		{
			list.Add(defaultValue);
		}
		valueSetter(list);
	}

	private static bool TryReplaceCollectionMemberWithNulls(object target, string memberName, int expectedCount)
	{
		if (!TryGetMemberValue(target, memberName, out object value) || value == null)
		{
			return false;
		}
		Type type = AccessTools.Property(target.GetType(), memberName)?.PropertyType ?? AccessTools.Field(target.GetType(), memberName)?.FieldType;
		if ((object)type == null)
		{
			return false;
		}
		IList list = (IList)(Activator.CreateInstance(type) ?? throw new InvalidOperationException("Could not create replacement collection for member " + memberName + "."));
		for (int i = 0; i < expectedCount; i++)
		{
			list.Add(null);
		}
		return TrySetMemberValue(target, memberName, list);
	}

	private static bool TryResizeList(IList list, int expectedCount)
	{
		try
		{
			while (list.Count > expectedCount)
			{
				list.RemoveAt(list.Count - 1);
			}
			while (list.Count < expectedCount)
			{
				list.Add(null);
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static string DescribeListContents(IList? list)
	{
		if (list == null)
		{
			return "<null>";
		}
		List<string> list2 = new List<string>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			list2.Add(DescribeIdentifier(list[i]));
		}
		return "[" + string.Join(", ", list2) + "]";
	}

	private static string DescribeIdentifier(object? value)
	{
		if (value == null)
		{
			return "null";
		}
		if (TryConvertIdentifier(value, out string identifier))
		{
			return identifier;
		}
		return value.ToString() ?? value.GetType().Name;
	}

	private static bool TryResetCollectionMemberToEmpty(object target, string memberName)
	{
		if (!TryGetMemberValue(target, memberName, out object value) || value == null)
		{
			return false;
		}
		Type type = AccessTools.Property(target.GetType(), memberName)?.PropertyType ?? AccessTools.Field(target.GetType(), memberName)?.FieldType;
		if ((object)type != null)
		{
			try
			{
				object obj = Activator.CreateInstance(type);
				if (obj != null && TrySetMemberValue(target, memberName, obj))
				{
					return true;
				}
			}
			catch
			{
			}
		}
		if (TryInvokeMethod(value, "Clear", Array.Empty<object>(), out object _))
		{
			return true;
		}
		if (value is IList list)
		{
			list.Clear();
			return true;
		}
		return false;
	}

	private static bool EnsureCollectionMemberSize(object target, Func<object?> valueAccessor, Action<object> valueSetter, Type collectionType, int expectedCount)
	{
		if (valueAccessor() is IList list && list.Count == expectedCount)
		{
			return false;
		}
		IList list2 = (IList)(Activator.CreateInstance(collectionType) ?? throw new InvalidOperationException("Could not create replacement collection for " + collectionType.FullName + "."));
		for (int i = 0; i < expectedCount; i++)
		{
			list2.Add(null);
		}
		valueSetter(list2);
		return true;
	}

	private static bool TryEnsureCollectionMemberSize(object target, string memberName, int expectedCount)
	{
		if (!TryGetMemberValue(target, memberName, out object value) || value == null)
		{
			return false;
		}
		if (value is IList list && list.Count == expectedCount)
		{
			return false;
		}
		Type type = AccessTools.Property(target.GetType(), memberName)?.PropertyType ?? AccessTools.Field(target.GetType(), memberName)?.FieldType;
		if ((object)type == null)
		{
			return false;
		}
		IList list2 = (IList)(Activator.CreateInstance(type) ?? throw new InvalidOperationException("Could not create replacement collection for member " + memberName + "."));
		for (int i = 0; i < expectedCount; i++)
		{
			list2.Add(null);
		}
		return TrySetMemberValue(target, memberName, list2);
	}
}
