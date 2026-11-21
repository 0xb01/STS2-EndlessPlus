using System;

namespace BNKZ.STS2.EndlessPlus;

internal static class EventSynchronizerPatches
{
	public static void BeforeBeginEvent(object __instance, object canonicalEvent, bool isPrefinished)
	{
		try
		{
			object runManagerInstance = EndlessReflection.GetRunManagerInstance();
			object runState = EndlessReflection.GetRunState(runManagerInstance);
			int completedLoops = EndlessRuntime.GetCompletedLoops(runState);
			if (completedLoops > 0 || EndlessRuntime.IsLaunchingFromGameOverScreen)
			{
				if (EndlessReflection.TryPrimeEventSynchronizerForBeginEvent(runManagerInstance, runState, canonicalEvent, out string stateSummary))
				{
					EndlessLog.Info("Primed EventSynchronizer immediately before BeginEvent after endless transition. State: " + stateSummary);
				}
				EndlessLog.Info($"About to begin multiplayer event (prefinished={isPrefinished}) after endless transition. State: {EndlessReflection.DescribeEventSynchronizerState(runManagerInstance, runState)}");
			}
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while logging EventSynchronizer.BeginEvent pre-state.", ex);
		}
	}

	public static Exception? FinalizeBeginEvent(Exception? __exception, object __instance, object canonicalEvent, bool isPrefinished)
	{
		if (__exception == null)
		{
			return null;
		}
		try
		{
			object runManagerInstance = EndlessReflection.GetRunManagerInstance();
			object runState = EndlessReflection.GetRunState(runManagerInstance);
			EndlessLog.Error($"EventSynchronizer.BeginEvent failed (prefinished={isPrefinished}). State: {EndlessReflection.DescribeEventSynchronizerState(runManagerInstance, runState)}", __exception);
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while logging EventSynchronizer.BeginEvent exception state.", ex);
		}
		return __exception;
	}
}
