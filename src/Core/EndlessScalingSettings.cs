using System;

namespace BNKZ.STS2.EndlessPlus;

/// <summary>
/// Encapsulates all monster scaling progression parameters for an Endless run.
/// Serialized into run state metadata for authoritative host-to-peer multiplayer synchronization.
/// </summary>
public struct EndlessScalingSettings
{
	public int LoopCount;
	public ScalingInterval Interval;
	public ScalingCurve Curve;
	public bool ScaleChapterOne;
	public decimal HpScale;
	public decimal DamageScale;
	public int FlatHp;
	public int FlatDmg;

	public static EndlessScalingSettings FromConfig(int loopCount)
	{
		return new EndlessScalingSettings
		{
			LoopCount = Math.Max(0, loopCount),
			Interval = EndlessConfig.Interval,
			Curve = EndlessConfig.Curve,
			ScaleChapterOne = EndlessConfig.ScaleChapterOne,
			HpScale = EndlessConfig.EnemyHpScalingPerChapter,
			DamageScale = EndlessConfig.EnemyDamageScalingPerChapter,
			FlatHp = EndlessConfig.FlatHpPerStep,
			FlatDmg = EndlessConfig.FlatDmgPerStep
		};
	}
}
