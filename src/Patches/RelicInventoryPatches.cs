using System;

namespace BNKZ.STS2.EndlessPlus;

internal static class RelicInventoryPatches
{
	public static bool BeforeAnimateRelic(object __instance, object relic, Godot.Vector2? startPosition, Godot.Vector2? startScale)
	{
		try
		{
			if (relic != null)
			{
				RunUiPatches.RecordNewRelic(relic);
			}

			if (EndlessReflection.RelicInventoryHasHolderForRelic(__instance, relic))
			{
				return true;
			}
			int relicInventoryNodeCount = EndlessReflection.GetRelicInventoryNodeCount(__instance);
			EndlessReflection.AddRelicInventoryNode(__instance, relic, startsShown: true, relicInventoryNodeCount);
			EndlessLog.Info($"Inserted missing relic inventory node before animation at index {relicInventoryNodeCount}.");
			return false;
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while repairing the relic inventory animation state.", ex);
			return true;
		}
	}
}
