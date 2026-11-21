using System;
using MegaCrit.Sts2.Core.Runs;

namespace BNKZ.STS2.EndlessPlus;

/// <summary>
/// Core mathematical calculation engine for monster stat scaling in Endless Mode.
/// Evaluates scaling steps across Act, Chapter, or Floor intervals and applies Exponential, Linear, or Fixed curves.
/// </summary>
internal static class ScalingCalculator
{
	/// <summary>
	/// Computes the number of scaling progression steps elapsed based on current run state and configured interval.
	/// Honors Chapter 1 protection (vanilla 1.0x difficulty) when SkipChapterOne is active.
	/// </summary>
	/// <param name="runState">Current active run state object.</param>
	/// <returns>Integer step count representing cumulative scaling intervals completed.</returns>
	public static int CalculateSteps(object runState)
	{
		EndlessConfig.CheckReload();

		int completedLoops = EndlessRuntime.GetCompletedLoops(runState);

		// Chapter 1 protection: if scale_chapter_1 is false, loop 0 never scales
		if (!EndlessConfig.ScaleChapterOne && completedLoops <= 0)
		{
			return 0;
		}

		int actsPerLoop = 3;
		int currentActIndex = 0;
		if (runState is IRunState state)
		{
			actsPerLoop = Math.Max(1, state.Acts?.Count ?? 3);
			currentActIndex = Math.Clamp(state.CurrentActIndex, 0, actsPerLoop - 1);
		}
		else
		{
			currentActIndex = Math.Max(0, EndlessReflection.GetCurrentActIndex(runState));
		}

		switch (EndlessConfig.Interval)
		{
			case ScalingInterval.Act:
				if (!EndlessConfig.ScaleChapterOne)
				{
					return (completedLoops - 1) * actsPerLoop + currentActIndex + 1;
				}
				return completedLoops * actsPerLoop + currentActIndex;

			case ScalingInterval.Chapter:
				if (!EndlessConfig.ScaleChapterOne)
				{
					return Math.Max(0, completedLoops);
				}
				return completedLoops + 1;

			case ScalingInterval.Floor:
				int totalFloor = Math.Max(0, EndlessReflection.GetTotalFloor(runState));
				if (!EndlessConfig.ScaleChapterOne)
				{
					int loopZeroFloors = actsPerLoop * 16;
					return Math.Max(0, totalFloor - loopZeroFloors);
				}
				return totalFloor;

			default:
				return completedLoops;
		}
	}

	/// <summary>
	/// Calculates total monster HP percentage multiplier for the current run state.
	/// Fetches host-authoritative scale rate in multiplayer to prevent client divergence.
	/// </summary>
	/// <param name="runState">Current active run state object.</param>
	/// <returns>Decimal multiplier to scale enemy maximum HP (e.g. 1.25m = +25% HP).</returns>
	public static decimal GetHpMultiplier(object runState)
	{
		int steps = CalculateSteps(runState);
		if (steps <= 0)
		{
			return 1m;
		}
		decimal scale = EndlessRuntime.GetEnemyHpScalePerChapter(runState);
		return ComputeMultiplier(scale, steps, EndlessConfig.MinHpMultiplier, EndlessConfig.MaxHpMultiplier);
	}

	/// <summary>
	/// Calculates total monster attack damage multiplier for the current run state.
	/// Fetches host-authoritative scale rate in multiplayer to prevent client divergence.
	/// </summary>
	/// <param name="runState">Current active run state object.</param>
	/// <returns>Decimal multiplier to scale enemy outgoing attack damage (e.g. 1.15m = +15% DMG).</returns>
	public static decimal GetDamageMultiplier(object runState)
	{
		int steps = CalculateSteps(runState);
		if (steps <= 0)
		{
			return 1m;
		}
		decimal scale = EndlessRuntime.GetEnemyDamageScalePerChapter(runState);
		return ComputeMultiplier(scale, steps, EndlessConfig.MinDmgMultiplier, EndlessConfig.MaxDmgMultiplier);
	}

	/// <summary>
	/// Computes flat monster HP bonus added on top of multiplier scaling per step.
	/// </summary>
	/// <param name="runState">Current active run state object.</param>
	/// <returns>Integer flat HP bonus to append to enemy maximum HP.</returns>
	public static int GetFlatHpBonus(object runState)
	{
		if (EndlessConfig.FlatHpPerStep <= 0)
		{
			return 0;
		}
		int steps = CalculateSteps(runState);
		return steps > 0 ? EndlessConfig.FlatHpPerStep * steps : 0;
	}

	/// <summary>
	/// Computes flat monster attack damage bonus added per hit on top of multiplier scaling.
	/// </summary>
	/// <param name="runState">Current active run state object.</param>
	/// <returns>Integer flat damage bonus to append to monster attack commands.</returns>
	public static int GetFlatDamageBonus(object runState)
	{
		if (EndlessConfig.FlatDmgPerStep <= 0)
		{
			return 0;
		}
		int steps = CalculateSteps(runState);
		return steps > 0 ? EndlessConfig.FlatDmgPerStep * steps : 0;
	}

	/// <summary>
	/// Formats human-readable status text displaying current endless loop, interval cadence, active ascension, and multiplier percentages.
	/// </summary>
	/// <param name="runState">Current active run state object.</param>
	/// <returns>Formatted status string suitable for top HUD banner overlay.</returns>
	public static string FormatStatusText(object runState)
	{
		int completedLoops = EndlessRuntime.GetCompletedLoops(runState);
		int chapter = completedLoops + 1;
		int act = EndlessReflection.GetCurrentActIndex(runState) + 1;
		int steps = CalculateSteps(runState);
		string prefix = completedLoops > 0 ? "Endless: " : "";

		if (steps <= 0)
		{
			return $"{prefix}Chapter {chapter} · Act {act}";
		}

		decimal hpMult = GetHpMultiplier(runState);
		decimal dmgMult = GetDamageMultiplier(runState);
		decimal hpPct = (hpMult - 1m) * 100m;
		decimal dmgPct = (dmgMult - 1m) * 100m;
		string hpSign = hpPct >= 0m ? "+" : "";
		string dmgSign = dmgPct >= 0m ? "+" : "";

		int flatHp = GetFlatHpBonus(runState);
		int flatDmg = GetFlatDamageBonus(runState);
		string flatHpStr = flatHp > 0 ? $" (+{flatHp})" : "";
		string flatDmgStr = flatDmg > 0 ? $" (+{flatDmg})" : "";

		return $"{prefix}Chapter {chapter} · Act {act} | HP {hpSign}{hpPct:0.#}%{flatHpStr} · DMG {dmgSign}{dmgPct:0.#}%{flatDmgStr}";
	}

	/// <summary>
	/// Evaluates mathematical scaling curve (Fixed, Linear, or Exponential) for a given base scale factor across specified step count.
	/// </summary>
	/// <param name="baseScale">Base factor configured per chapter or step (e.g. 1.08).</param>
	/// <param name="steps">Total scaling steps accumulated.</param>
	/// <param name="minMult">Minimum allowed multiplier floor.</param>
	/// <param name="maxMult">Maximum allowed multiplier ceiling (0 or negative means unconstrained).</param>
	/// <returns>Resulting multiplier clamped within [minMult, maxMult].</returns>
	private static decimal ComputeMultiplier(decimal baseScale, int steps, decimal minMult, decimal maxMult)
	{
		if (steps <= 0)
		{
			return 1m;
		}

		if (EndlessConfig.Curve == ScalingCurve.Fixed)
		{
			return Clamp(baseScale, minMult, maxMult);
		}

		decimal result;
		if (EndlessConfig.Curve == ScalingCurve.Linear)
		{
			result = 1m + (baseScale - 1m) * (decimal)steps;
		}
		else // Exponential
		{
			result = 1m;
			for (int i = 0; i < steps; i++)
			{
				result *= baseScale;
			}
		}

		return Clamp(result, minMult, maxMult);
	}

	/// <summary>
	/// Clamps multiplier value between min and max bounds.
	/// </summary>
	private static decimal Clamp(decimal val, decimal minMult, decimal maxMult)
	{
		if (val < minMult)
		{
			val = minMult;
		}
		if (maxMult > 0m && val > maxMult)
		{
			val = maxMult;
		}
		return val;
	}
}
