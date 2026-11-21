using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace BNKZ.STS2.EndlessPlus;

public enum ScalingInterval
{
	Act,
	Chapter,
	Floor
}

public enum ScalingCurve
{
	Exponential,
	Linear,
	Fixed
}

internal static class EndlessConfig
{
	public static readonly bool Enabled = true;

	public static ScalingInterval Interval { get; private set; } = ScalingInterval.Act;
	public static ScalingCurve Curve { get; private set; } = ScalingCurve.Exponential;

	public static decimal EnemyHpScalingPerChapter { get; private set; } = 1.08m;
	public static decimal EnemyDamageScalingPerChapter { get; private set; } = 1.05m;

	public static bool ScaleChapterOne { get; private set; } = false;

	public static decimal MinHpMultiplier { get; private set; } = 1.0m;
	public static decimal MaxHpMultiplier { get; private set; } = 0.0m;

	public static decimal MinDmgMultiplier { get; private set; } = 1.0m;
	public static decimal MaxDmgMultiplier { get; private set; } = 0.0m;

	public static int FlatHpPerStep { get; private set; } = 500;
	public static int FlatDmgPerStep { get; private set; } = 2;

	public static int AscensionPerLoop { get; private set; } = -1;
	public static int MaxAscension { get; private set; } = 10;

	public static bool OverrideHud { get; private set; } = true;

	private static string ConfigPath
	{
		get
		{
			string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory;
			return Path.Combine(dir, "BNKZ.STS2.EndlessPlus.config");
		}
	}

	private static DateTime _lastWriteTime;

	/// <summary>
	/// Checks if configuration file has been modified externally and triggers dynamic hot-reloading if not currently in active combat.
	/// </summary>
	public static void CheckReload()
	{
		try
		{
			// Block hot-reloading during combat to prevent state divergence across multiplayer peers
			if (MegaCrit.Sts2.Core.Combat.CombatManager.Instance?.IsInProgress == true)
			{
				return;
			}
			if (File.Exists(ConfigPath))
			{
				DateTime writeTime = File.GetLastWriteTimeUtc(ConfigPath);
				if (writeTime != _lastWriteTime)
				{
					Load();
					EndlessLog.Info($"Hot-reloaded endless config: hp_scale={EnemyHpScalingPerChapter}, dmg_scale={EnemyDamageScalingPerChapter}, flat_hp={FlatHpPerStep}, flat_dmg={FlatDmgPerStep}, curve={Curve}, interval={Interval}");
				}
			}
		}
		catch
		{
			// Ignore concurrent file access
		}
	}

	/// <summary>
	/// Loads and parses endless mode configuration values from the .config file on disk, with automatic fallback defaults.
	/// </summary>
	public static void Load()
	{
		try
		{
			EnsureConfigExists();
			if (!File.Exists(ConfigPath))
			{
				EndlessLog.Info($"Config file not found at '{ConfigPath}', using defaults.");
				return;
			}
			_lastWriteTime = File.GetLastWriteTimeUtc(ConfigPath);

			string[] array = File.ReadAllLines(ConfigPath);
			foreach (string text in array)
			{
				string text2 = text.Trim();
				if (text2.Length == 0 || text2.StartsWith('#'))
				{
					continue;
				}
				string[] array2 = text2.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
				if (array2.Length < 2)
				{
					continue;
				}

				string key = array2[0].ToLowerInvariant();
				string val = array2[1];

				switch (key)
				{
					case "interval":
						if (Enum.TryParse<ScalingInterval>(val, true, out var parsedInterval))
						{
							Interval = parsedInterval;
						}
						break;

					case "skip_chapter":
					case "skip_chapter_1":
					case "skip_chapter_one":
					case "skip_first_chapter":
						if (bool.TryParse(val, out var skipCh))
						{
							ScaleChapterOne = !skipCh;
						}
						break;

					case "scale_per_act":
						if (bool.TryParse(val, out var spa))
						{
							ScaleChapterOne = spa;
						}
						break;

					case "curve":
						if (Enum.TryParse<ScalingCurve>(val, true, out var parsedCurve))
						{
							Curve = parsedCurve;
						}
						break;

					case "hp_scale":
					case "hp_scale_per_act":
					case "hp_scale_per_chapter":
						if (decimal.TryParse(val, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var hp))
						{
							EnemyHpScalingPerChapter = Math.Max(0.01m, hp);
						}
						break;

					case "dmg_scale":
					case "dmg_scale_per_act":
					case "dmg_scale_per_chapter":
						if (decimal.TryParse(val, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var dmg))
						{
							EnemyDamageScalingPerChapter = Math.Max(0.01m, dmg);
						}
						break;

					case "scale_chapter_1":
					case "scale_acts_in_chapter_1":
						if (bool.TryParse(val, out var ch1))
						{
							ScaleChapterOne = ch1;
						}
						break;

					case "min_hp_multiplier":
						if (decimal.TryParse(val, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var minHp))
						{
							MinHpMultiplier = Math.Max(0m, minHp);
						}
						break;

					case "max_hp_multiplier":
						if (decimal.TryParse(val, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var maxHp))
						{
							MaxHpMultiplier = Math.Max(0m, maxHp);
						}
						break;

					case "min_dmg_multiplier":
						if (decimal.TryParse(val, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var minDmg))
						{
							MinDmgMultiplier = Math.Max(0m, minDmg);
						}
						break;

					case "max_dmg_multiplier":
						if (decimal.TryParse(val, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var maxDmg))
						{
							MaxDmgMultiplier = Math.Max(0m, maxDmg);
						}
						break;

					case "flat_hp_per_step":
						if (int.TryParse(val, out var flatHp))
						{
							FlatHpPerStep = Math.Max(0, flatHp);
						}
						break;

					case "flat_dmg_per_step":
						if (int.TryParse(val, out var flatDmg))
						{
							FlatDmgPerStep = Math.Max(0, flatDmg);
						}
						break;

					case "ascension_per_loop":
						if (int.TryParse(val, out var asc))
						{
							AscensionPerLoop = asc;
						}
						break;

					case "max_ascension":
						if (int.TryParse(val, out var maxAsc))
						{
							MaxAscension = Math.Clamp(maxAsc, 0, 50);
						}
						break;

					case "override_hud":
						if (bool.TryParse(val, out var hud))
						{
							OverrideHud = hud;
						}
						break;
				}
			}

			EndlessLog.Info($"Loaded endless config from '{ConfigPath}': interval={Interval}, curve={Curve}, hp_scale={EnemyHpScalingPerChapter:0.###}, dmg_scale={EnemyDamageScalingPerChapter:0.###}, flat_hp={FlatHpPerStep}, flat_dmg={FlatDmgPerStep}, skip_chapter={!ScaleChapterOne}.");
			BaseLibIntegration.TrySyncFromEndlessConfig();
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed to load endless config from '" + ConfigPath + "'. Falling back to defaults.", ex);
		}
	}

	/// <summary>
	/// Applies runtime settings submitted from GUI or Mod Configuration menu, optionally persisting them back to the .config file.
	/// </summary>
	public static void ApplySettings(
		ScalingInterval interval,
		bool skipChapterOne,
		ScalingCurve curve,
		decimal hpScale,
		decimal dmgScale,
		int flatHp,
		int flatDmg,
		int ascensionPerLoop,
		int maxAscension,
		bool overrideHud,
		bool saveToFile)
	{
		Interval = interval;
		ScaleChapterOne = !skipChapterOne;
		Curve = curve;
		EnemyHpScalingPerChapter = Math.Max(0.01m, hpScale);
		EnemyDamageScalingPerChapter = Math.Max(0.01m, dmgScale);
		FlatHpPerStep = Math.Max(0, flatHp);
		FlatDmgPerStep = Math.Max(0, flatDmg);
		AscensionPerLoop = ascensionPerLoop;
		MaxAscension = Math.Clamp(maxAscension, 0, 50);
		OverrideHud = overrideHud;

		EndlessLog.Info($"Applied endless config from GUI: interval={Interval}, curve={Curve}, hp_scale={EnemyHpScalingPerChapter:0.###}, dmg_scale={EnemyDamageScalingPerChapter:0.###}, flat_hp={FlatHpPerStep}, flat_dmg={FlatDmgPerStep}, skip_chapter={!ScaleChapterOne}.");

		if (saveToFile)
		{
			SaveToConfigFile();
		}
	}

	/// <summary>
	/// Serializes in-memory configuration values back to the .config file on disk while preserving comments and layout.
	/// </summary>
	public static void SaveToConfigFile()
	{
		try
		{
			EnsureConfigExists();
			if (!File.Exists(ConfigPath)) return;

			string[] lines = File.ReadAllLines(ConfigPath);
			for (int i = 0; i < lines.Length; i++)
			{
				string trimmed = lines[i].Trim();
				if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;

				string[] parts = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 1) continue;

				string key = parts[0].ToLowerInvariant();
				switch (key)
				{
					case "interval":
						lines[i] = $"interval {Interval.ToString().ToLowerInvariant()}";
						break;
					case "skip_chapter":
					case "skip_chapter_1":
					case "skip_chapter_one":
					case "skip_first_chapter":
						lines[i] = $"skip_chapter {(!ScaleChapterOne).ToString().ToLowerInvariant()}";
						break;
					case "curve":
						lines[i] = $"curve {Curve.ToString().ToLowerInvariant()}";
						break;
					case "hp_scale":
					case "hp_scale_per_act":
					case "hp_scale_per_chapter":
						lines[i] = $"hp_scale {EnemyHpScalingPerChapter.ToString("0.00", CultureInfo.InvariantCulture)}";
						break;
					case "dmg_scale":
					case "dmg_scale_per_act":
					case "dmg_scale_per_chapter":
						lines[i] = $"dmg_scale {EnemyDamageScalingPerChapter.ToString("0.00", CultureInfo.InvariantCulture)}";
						break;
					case "flat_hp_per_step":
						lines[i] = $"flat_hp_per_step {FlatHpPerStep}";
						break;
					case "flat_dmg_per_step":
						lines[i] = $"flat_dmg_per_step {FlatDmgPerStep}";
						break;
					case "ascension_per_loop":
						lines[i] = $"ascension_per_loop {AscensionPerLoop}";
						break;
					case "max_ascension":
						lines[i] = $"max_ascension {MaxAscension}";
						break;
					case "override_hud":
						lines[i] = $"override_hud {OverrideHud.ToString().ToLowerInvariant()}";
						break;
				}
			}
			File.WriteAllLines(ConfigPath, lines);
			_lastWriteTime = File.GetLastWriteTimeUtc(ConfigPath);
			EndlessLog.Info("Saved updated endless config to file: " + ConfigPath);
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed to save endless config to file: " + ex.Message, ex);
		}
	}

	private static void EnsureConfigExists()
	{
		string? directoryName = Path.GetDirectoryName(ConfigPath);
		if (!string.IsNullOrWhiteSpace(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		if (!File.Exists(ConfigPath))
		{
			string defaultContent = @"# ==============================================================================
# BNKZ.STS2.EndlessPlus - Configuration
# ==============================================================================
# Slay the Spire 2 Endless Mode with customizable monster stat & damage scaling.
# Hot-reloading is supported: changes to this file take effect immediately in-game.
# Syntax: <setting_name> <value> (separated by spaces or tabs).
# ==============================================================================

# ------------------------------------------------------------------------------
# 1. SCALING CADENCE & TIMING
# ------------------------------------------------------------------------------
# interval: Determines how frequently enemy stats scale up.
#   - act     : Scale every act transition (Recommended for smooth difficulty).
#   - chapter : Scale only upon completing an entire chapter (full loop).
#   - floor   : Scale on every individual floor room entered.
interval act

# skip_chapter: Whether to keep Chapter 1 at normal vanilla difficulty.
#   - true    : Chapter 1 has 1.0x stats. Scaling begins in Chapter 2 Act 1.
#   - false   : Scaling applies immediately starting from Act 2 of Chapter 1.
skip_chapter true

# ------------------------------------------------------------------------------
# 2. SCALING ALGORITHM (CURVE)
# ------------------------------------------------------------------------------
# curve: Math formula used to compute multipliers per step:
#   - exponential : Multiplier grows exponentially: scale ^ step
#                   Example: at 1.08 hp_scale -> Step 1: 1.08x, Step 2: 1.166x, Step 3: 1.26x...
#   - linear      : Multiplier grows linearly: 1.0 + (scale - 1.0) * step
#                   Example: at 1.08 hp_scale -> Step 1: 1.08x, Step 2: 1.16x, Step 3: 1.24x...
#   - fixed       : Multiplier remains fixed at the configured value regardless of step count.
curve exponential

# ------------------------------------------------------------------------------
# 3. PERCENTAGE MULTIPLIERS (PER STEP)
# ------------------------------------------------------------------------------
# hp_scale: Enemy Max HP percentage multiplier per step.
#   - 1.08 = +8% HP per step (1.00 = +0%, 1.10 = +10%, 1.25 = +25%)
hp_scale 1.08

# dmg_scale: Enemy Attack Damage percentage multiplier per step.
#   - 1.05 = +5% DMG per step (1.00 = +0%, 1.05 = +5%, 1.10 = +10%)
dmg_scale 1.05

# ------------------------------------------------------------------------------
# 4. FLAT STAT BONUSES (PER STEP)
# ------------------------------------------------------------------------------
# Flat bonuses are added directly to base stats after percentage multipliers:
# Final HP  = (Base HP * hp_multiplier) + (flat_hp_per_step * step)
# Final DMG = (Base DMG * dmg_multiplier) + (flat_dmg_per_step * step)

# flat_hp_per_step: Flat monster HP added per step (e.g. 500 adds +500 HP each step).
# Recommended for multiplayer (4 players): 500
flat_hp_per_step 500

# flat_dmg_per_step: Flat damage added per hit per step (e.g. 2 adds +2 DMG each step).
# Recommended for multiplayer (4 players): 2
flat_dmg_per_step 2

# ------------------------------------------------------------------------------
# 5. MULTIPLIER CLAMPS (MIN / MAX)
# ------------------------------------------------------------------------------
# Prevent scaling from dropping below min or exceeding max.
# Set max to 0.0 for uncapped (infinite) scaling.
min_hp_multiplier 1.0
max_hp_multiplier 0.0

min_dmg_multiplier 1.0
max_dmg_multiplier 0.0

# ------------------------------------------------------------------------------
# 6. ASCENSION PROGRESSION
# ------------------------------------------------------------------------------
# ascension_per_loop: How much Ascension level increases after completing a loop.
#   - -1 : Keep vanilla Ascension behavior (does not modify Ascension mid-run).
#   -  0 : Lock Ascension at starting level throughout endless run.
#   - 1+ : Increase Ascension by this amount each loop (e.g. 1 = +1 Ascension per loop).
ascension_per_loop -1

# max_ascension: Hard ceiling for Ascension level during endless runs (range: 0 to 50).
max_ascension 10

# ------------------------------------------------------------------------------
# 7. USER INTERFACE & OVERLAY
# ------------------------------------------------------------------------------
# override_hud: Show current Endless scaling stats in the top HUD banner.
#   - true  : Displays [Ch X Act Y (Step Z) HP +A% DMG +B% [Flat: +C HP / +D DMG]].
#   - false : Standard vanilla top HUD bar.
override_hud true
".ReplaceLineEndings(Environment.NewLine);
			File.WriteAllText(ConfigPath, defaultContent);
		}
	}
}
