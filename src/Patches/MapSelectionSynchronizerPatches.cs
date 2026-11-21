using System;

namespace BNKZ.STS2.EndlessPlus;

internal static class MapSelectionSynchronizerPatches
{
	public static void BeforeGetVote(object __instance, ref object player)
	{
		try
		{
			RemapPlayerToCurrentRun(__instance, ref player, "GetVote");
			if (EndlessReflection.EnsureMapSelectionVotesMatchPlayers(__instance))
			{
				object mapSelectionRunState = EndlessReflection.GetMapSelectionRunState(__instance);
				int playerSlotIndex = EndlessReflection.GetPlayerSlotIndex(mapSelectionRunState, player);
				int playersCount = EndlessReflection.GetPlayersCount(mapSelectionRunState);
				EndlessLog.Info($"Repaired map-selection vote list before GetVote for player slot {playerSlotIndex} (players={playersCount}).");
			}
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while repairing map-selection votes before GetVote.", ex);
		}
	}

	public static void BeforePlayerVotedForMapCoord(object __instance, ref object player)
	{
		try
		{
			RemapPlayerToCurrentRun(__instance, ref player, "PlayerVotedForMapCoord");
			if (EndlessReflection.EnsureMapSelectionVotesMatchPlayers(__instance))
			{
				object mapSelectionRunState = EndlessReflection.GetMapSelectionRunState(__instance);
				int playersCount = EndlessReflection.GetPlayersCount(mapSelectionRunState);
				EndlessLog.Info($"Repaired map-selection vote state before PlayerVotedForMapCoord (players={playersCount}).");
			}
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while repairing map-selection votes before PlayerVotedForMapCoord.", ex);
		}
	}

	private static void RemapPlayerToCurrentRun(object synchronizer, ref object player, string source)
	{
		object mapSelectionRunState = EndlessReflection.GetMapSelectionRunState(synchronizer);
		if (EndlessReflection.TryRemapPlayerToCurrentRun(mapSelectionRunState, player, out object remappedPlayer) && remappedPlayer != player)
		{
			player = remappedPlayer;
			int playerSlotIndex = EndlessReflection.GetPlayerSlotIndex(mapSelectionRunState, remappedPlayer);
			EndlessLog.Info($"Remapped stale multiplayer player reference before {source} to slot {playerSlotIndex}.");
		}
	}
}
