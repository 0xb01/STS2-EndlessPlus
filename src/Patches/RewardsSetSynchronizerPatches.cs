using System;
using System.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace BNKZ.STS2.EndlessPlus;

internal static class RewardsSetSynchronizerPatches
{
	public static bool BeforeGetRewardStateForPlayer(object __instance, ref object? __result, object player)
	{
		try
		{
			object? currentRunState = EndlessReflection.GetRunManagerState(RunManager.Instance);
			if (currentRunState != null)
			{
				object? currentCollection = EndlessReflection.GetRewardsSetSynchronizerPlayerCollection(__instance);
				if (currentCollection != currentRunState)
				{
					EndlessReflection.SetRewardsSetSynchronizerPlayerCollection(__instance, currentRunState);
				}

				if (EndlessReflection.TryRemapPlayerToCurrentRun(currentRunState, player, out object remappedPlayer) && remappedPlayer != null)
				{
					player = remappedPlayer;
				}

				int slotIndex = EndlessReflection.GetPlayerSlotIndex(currentRunState, player);
				if (slotIndex < 0)
				{
					slotIndex = 0;
				}

				IList? rewardStates = EndlessReflection.GetRewardsSetSynchronizerRewardStates(__instance);
				if (rewardStates != null)
				{
					while (rewardStates.Count <= slotIndex)
					{
						object? newEntry = EndlessReflection.CreatePlayerRewardState();
						if (newEntry != null)
						{
							rewardStates.Add(newEntry);
						}
						else
						{
							break;
						}
					}

					if (slotIndex >= 0 && slotIndex < rewardStates.Count)
					{
						__result = rewardStates[slotIndex];
						return false;
					}
				}
			}
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed in BeforeGetRewardStateForPlayer safety patch.", ex);
		}

		return true;
	}
}
