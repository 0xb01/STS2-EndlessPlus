using System;

namespace BNKZ.STS2.EndlessPlus;

internal static class RunManagerPatches
{
	public static bool BeforeSetActInternal(object __instance, ref int actIndex)
	{
		try
		{
			EndlessLog.Info($"SetActInternal requested with actIndex={actIndex}.");
			EndlessRuntime.TryWrapActIndex(__instance, ref actIndex);
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while wrapping SetActInternal.", ex);
		}
		return true;
	}

	public static void AfterLaunch()
	{
		try
		{
			if (!EndlessRuntime.IsLaunchingFromGameOverScreen)
			{
				EndlessRuntime.ResetTransientState("RunManager.Launch");
			}
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while resetting endless runtime state on run launch.", ex);
		}
	}
}
