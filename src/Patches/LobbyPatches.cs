using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

namespace BNKZ.STS2.EndlessPlus;

internal static class LobbyPatches
{
	private static readonly FieldInfo? ConnectingPlayersField = AccessTools.Field(typeof(StartRunLobby), "_connectingPlayers");

	public static bool BeforeIsAboutToBeginGame(StartRunLobby __instance, ref bool __result)
	{
		try
		{
			if (ConnectingPlayersField?.GetValue(__instance) is IList connecting && connecting.Count > 0)
			{
				__result = false;
				return false;
			}

			if (__instance.Players.Count == 0)
			{
				__result = false;
				return false;
			}

			if (!__instance.Players.TrueForAll(p => p.isReady))
			{
				__result = false;
				return false;
			}

			// Allow starting solo in multiplayer lobby (vanilla blocks Players.Count == 1)
			__result = true;
			return false;
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed in StartRunLobby.IsAboutToBeginGame patch.", ex);
			return true;
		}
	}
}
