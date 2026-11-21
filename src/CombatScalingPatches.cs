using System;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Runs;

namespace BNKZ.STS2.EndlessPlus;

/// <summary>
/// Harmony interceptors and state trackers for monster stat scaling during combat.
/// Scales monster Max HP, current HP, attack intent previews, and outgoing attack damage per hit.
/// </summary>
internal static class CombatScalingPatches
{
	private sealed class HpScalingState
	{
		public required int LastBaseMaxHp { get; init; }

		public required int LastScaledMaxHp { get; init; }

		public required decimal LastMultiplier { get; init; }

		public required int LastFlatHp { get; init; }
	}

	private sealed class IntentScalingState
	{
		public required Func<decimal> OriginalDamageCalc { get; init; }

		public required Func<decimal> WrappedDamageCalc { get; init; }

		public required decimal LastMultiplier { get; init; }

		public required int LastFlatDmg { get; init; }
	}

	private static readonly ConditionalWeakTable<object, HpScalingState> HpScaledCreatures = new ConditionalWeakTable<object, HpScalingState>();

	private static readonly ConditionalWeakTable<object, IntentScalingState> ScaledAttackIntents = new ConditionalWeakTable<object, IntentScalingState>();

	/// <summary>
	/// Postfix patch on Creature creation to apply Endless HP multiplier and flat HP bonuses.
	/// </summary>
	public static void AfterCreateCreature(object? __result)
	{
		if (__result != null)
		{
			TryScaleCreatureHp(__result, "CreateCreature");
		}
	}

	/// <summary>
	/// Postfix patch on monster unique HP initialization.
	/// </summary>
	public static void AfterSetUniqueMonsterHpValue(object __instance)
	{
		TryScaleCreatureHp(__instance, "SetUniqueMonsterHpValue");
	}

	/// <summary>
	/// Postfix patch after multiplayer HP scaling runs to ensure Endless scaling compounds cleanly.
	/// </summary>
	public static void AfterScaleMonsterHpForMultiplayer(object __instance)
	{
		TryScaleCreatureHp(__instance, "ScaleMonsterHpForMultiplayer");
	}

	/// <summary>
	/// Prefix patch before creature registration into combat room.
	/// </summary>
	public static void BeforeAddCreature(object creature)
	{
		TryScaleCreatureHp(creature, "AddCreature");
	}

	/// <summary>
	/// Prefix patch on monster attack command execution to scale damage per hit by endless multiplier and flat damage bonus.
	/// </summary>
	public static void BeforeAttackExecute(object __instance)
	{
		try
		{
			object attackCommandAttacker = EndlessReflection.GetAttackCommandAttacker(__instance);
			if (attackCommandAttacker == null || !EndlessReflection.IsMonsterCreature(attackCommandAttacker))
			{
				return;
			}
			object? creatureCombatState = EndlessReflection.GetCreatureCombatState(attackCommandAttacker);
			object? combatStateRunState = null;
			if (creatureCombatState != null)
			{
				combatStateRunState = EndlessReflection.GetCombatStateRunState(creatureCombatState);
			}
			if (combatStateRunState == null)
			{
				combatStateRunState = EndlessReflection.GetRunManagerState(RunManager.Instance);
			}
			if (combatStateRunState == null)
			{
				return;
			}
			decimal enemyDamageScalingMultiplier = EndlessRuntime.GetEnemyDamageScalingMultiplier(combatStateRunState);
			int flatDmg = EndlessRuntime.GetFlatDamageBonus(combatStateRunState);
			if (enemyDamageScalingMultiplier <= 1m && flatDmg <= 0)
			{
				return;
			}
			decimal attackCommandDamagePerHit = EndlessReflection.GetAttackCommandDamagePerHit(__instance);
			if (attackCommandDamagePerHit > 0m)
			{
				decimal scaledDmg = decimal.Ceiling(attackCommandDamagePerHit * enemyDamageScalingMultiplier) + (decimal)flatDmg;
				EndlessReflection.SetAttackCommandDamagePerHit(__instance, ClampToIntRange(scaledDmg));
			}
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while scaling monster damage for endless mode.", ex);
		}
	}

	/// <summary>
	/// Prefix patch before monster attack intent is rendered, scaling the damage preview shown over enemy heads.
	/// </summary>
	public static void BeforeUpdateIntent(object intent, object owner)
	{
		try
		{
			if (!EndlessReflection.IsAttackIntent(intent) || !EndlessReflection.IsMonsterCreature(owner))
			{
				return;
			}
			object? creatureCombatState = EndlessReflection.GetCreatureCombatState(owner);
			object? combatStateRunState = null;
			if (creatureCombatState != null)
			{
				combatStateRunState = EndlessReflection.GetCombatStateRunState(creatureCombatState);
			}
			if (combatStateRunState == null)
			{
				combatStateRunState = EndlessReflection.GetRunManagerState(RunManager.Instance);
			}
			if (combatStateRunState == null)
			{
				return;
			}
			decimal multiplier = EndlessRuntime.GetEnemyDamageScalingMultiplier(combatStateRunState);
			int flatDmg = EndlessRuntime.GetFlatDamageBonus(combatStateRunState);
			if (multiplier <= 1m && flatDmg <= 0)
			{
				return;
			}
			Func<decimal> func = EndlessReflection.GetAttackIntentDamageCalc(intent);
			if (func == null)
			{
				return;
			}
			if (ScaledAttackIntents.TryGetValue(intent, out IntentScalingState value))
			{
				if ((object)func == value.WrappedDamageCalc && value.LastMultiplier == multiplier && value.LastFlatDmg == flatDmg)
				{
					return;
				}
				if ((object)func == value.WrappedDamageCalc)
				{
					func = value.OriginalDamageCalc;
				}
			}
			Func<decimal> originalDamageCalc = func;
			Func<decimal> func2 = () => ClampToIntRange(decimal.Ceiling(originalDamageCalc() * multiplier) + (decimal)flatDmg);
			EndlessReflection.SetAttackIntentDamageCalc(intent, func2);
			ScaledAttackIntents.Remove(intent);
			ScaledAttackIntents.Add(intent, new IntentScalingState
			{
				OriginalDamageCalc = originalDamageCalc,
				WrappedDamageCalc = func2,
				LastMultiplier = multiplier,
				LastFlatDmg = flatDmg
			});
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while scaling monster intent damage for endless mode.", ex);
		}
	}

	/// <summary>
	/// Core routine that calculates and updates monster Max HP and current HP.
	/// Uses conditional weak table caching to prevent double-scaling the same creature across multiple hooks.
	/// </summary>
	/// <param name="creature">Creature instance being checked.</param>
	/// <param name="source">Name of invoking hook for diagnostic logging.</param>
	private static void TryScaleCreatureHp(object creature, string source)
	{
		try
		{
			if (!EndlessReflection.IsMonsterCreature(creature))
			{
				return;
			}
			object? creatureCombatState = EndlessReflection.GetCreatureCombatState(creature);
			object? combatStateRunState = null;
			if (creatureCombatState != null)
			{
				combatStateRunState = EndlessReflection.GetCombatStateRunState(creatureCombatState);
			}
			if (combatStateRunState == null)
			{
				combatStateRunState = EndlessReflection.GetRunManagerState(RunManager.Instance);
			}
			if (combatStateRunState == null)
			{
				return;
			}
			int playersCount = EndlessReflection.GetPlayersCount(combatStateRunState);
			if (source == "SetUniqueMonsterHpValue" && playersCount > 1)
			{
				return;
			}
			decimal enemyScalingMultiplier = EndlessRuntime.GetEnemyScalingMultiplier(combatStateRunState);
			int flatHp = EndlessRuntime.GetFlatHpBonus(combatStateRunState);
			if (enemyScalingMultiplier <= 1m && flatHp <= 0)
			{
				return;
			}
			int creatureMaxHp = EndlessReflection.GetCreatureMaxHp(creature);
			if (creatureMaxHp <= 0 || (HpScaledCreatures.TryGetValue(creature, out HpScalingState value) && 
			    value.LastMultiplier == enemyScalingMultiplier && 
			    value.LastFlatHp == flatHp && 
			    creatureMaxHp == value.LastScaledMaxHp))
			{
				return;
			}
			decimal num = decimal.Ceiling((decimal)creatureMaxHp * enemyScalingMultiplier) + (decimal)flatHp;
			int num2 = Math.Max(creatureMaxHp, ClampToIntRange(num));
			if (num2 == creatureMaxHp)
			{
				ReplaceScalingState(creature, new HpScalingState
				{
					LastBaseMaxHp = creatureMaxHp,
					LastScaledMaxHp = num2,
					LastMultiplier = enemyScalingMultiplier,
					LastFlatHp = flatHp
				});
				return;
			}
			int creatureCurrentHp = EndlessReflection.GetCreatureCurrentHp(creature);
			decimal num3 = ((creatureMaxHp <= 0) ? 1m : ((decimal)creatureCurrentHp / (decimal)creatureMaxHp));
			int value2 = Math.Max(1, ClampToIntRange(decimal.Ceiling((decimal)num2 * num3)));
			EndlessReflection.SetCreatureMaxHpInternal(creature, num2);
			EndlessReflection.SetCreatureCurrentHpInternal(creature, value2);
			int? creatureMonsterMaxHpBeforeModification = EndlessReflection.GetCreatureMonsterMaxHpBeforeModification(creature);
			if (creatureMonsterMaxHpBeforeModification.HasValue)
			{
				EndlessReflection.SetCreatureMonsterMaxHpBeforeModification(creature, Math.Max(creatureMonsterMaxHpBeforeModification.Value, ClampToIntRange(decimal.Ceiling((decimal)creatureMonsterMaxHpBeforeModification.Value * enemyScalingMultiplier) + (decimal)flatHp)));
			}
			ReplaceScalingState(creature, new HpScalingState
			{
				LastBaseMaxHp = creatureMaxHp,
				LastScaledMaxHp = num2,
				LastMultiplier = enemyScalingMultiplier,
				LastFlatHp = flatHp
			});
			EndlessLog.Info($"Scaled monster HP for endless. source={source} creatureType={creature.GetType().Name} baseMaxHp={creatureMaxHp} scaledMaxHp={num2} scaledCurrentHp={value2} multiplier={enemyScalingMultiplier:0.###} flatHp={flatHp} loop={EndlessRuntime.GetCompletedLoops(combatStateRunState)}");
			if (num > 2147483647m)
			{
				EndlessLog.Info($"Capped endless monster HP at {int.MaxValue} because raw scaled HP {num:0} exceeds the game's integer HP limit.");
			}
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while scaling monster health for endless mode (source=" + source + ").", ex);
		}
	}

	private static void ReplaceScalingState(object creature, HpScalingState state)
	{
		HpScaledCreatures.Remove(creature);
		HpScaledCreatures.Add(creature, state);
	}

	private static int ClampToIntRange(decimal value)
	{
		if (value >= 2147483647m)
		{
			return int.MaxValue;
		}
		if (value <= -2147483648m)
		{
			return int.MinValue;
		}
		return (int)value;
	}
}
