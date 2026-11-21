using System;

namespace BNKZ.STS2.EndlessPlus;

internal static class CombatRoomPatches
{
	public static void BeforeStartCombat(object __instance, object? __0)
	{
		try
		{
			object runManagerInstance = EndlessReflection.GetRunManagerInstance();
			object runState = __0 ?? EndlessReflection.GetRunState(runManagerInstance);
			int completedLoops = EndlessRuntime.GetCompletedLoops(runState);
			if (completedLoops > 0 || EndlessRuntime.IsLaunchingFromGameOverScreen)
			{
				EndlessLog.Info($"About to start combat after endless transition. loop={completedLoops} totalFloor={EndlessReflection.GetTotalFloor(runState)} eventSync={EndlessReflection.DescribeEventSynchronizerState(runManagerInstance, runState)}");
			}
			if (EndlessRuntime.TryConsumeFirstCombatResetRequest())
			{
				EndlessLog.Info("Reached the first endless combat room; keeping multiplayer action queues intact. EventSync now: " + EndlessReflection.DescribeEventSynchronizerState(runManagerInstance, runState));
			}
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while preparing combat diagnostics for the first endless combat room.", ex);
		}
	}

	public static Exception? FinalizeStartCombat(Exception? __exception, object __instance, object? __0)
	{
		if (__exception == null)
		{
			return null;
		}
		try
		{
			object runManagerInstance = EndlessReflection.GetRunManagerInstance();
			object runState = __0 ?? EndlessReflection.GetRunState(runManagerInstance);
			EndlessLog.Error($"CombatRoom.StartCombat failed after endless transition. loop={EndlessRuntime.GetCompletedLoops(runState)} totalFloor={EndlessReflection.GetTotalFloor(runState)} eventSync={EndlessReflection.DescribeEventSynchronizerState(runManagerInstance, runState)}", __exception);
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while logging CombatRoom.StartCombat exception state.", ex);
		}
		return __exception;
	}
}
