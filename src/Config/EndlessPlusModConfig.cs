using System;
using System.Collections.Generic;
using BaseLib.Config;
using Godot;
using MegaCrit.Sts2.Core.Localization;

namespace BNKZ.STS2.EndlessPlus;

/// <summary>
/// BaseLib SimpleModConfig implementation for EndlessPlus.
/// Generates the in-game Mod Configuration page UI and synchronizes two-way with the .config file.
/// </summary>
[ConfigHoverTipsByDefault]
public class EndlessPlusModConfig : SimpleModConfig
{
	[ConfigSection("Cadence & Progression")]
	public static ScalingInterval Interval { get; set; } = ScalingInterval.Act;

	public static bool SkipChapterOne { get; set; } = true;

	public static ScalingCurve Curve { get; set; } = ScalingCurve.Exponential;

	[ConfigSlider(-1, 20, 1, Format = "{0}")]
	public static int AscensionPerLoop { get; set; } = -1;

	[ConfigSlider(0, 50, 1, Format = "A{0}")]
	public static int MaxAscension { get; set; } = 10;

	[ConfigSection("Stat Multipliers (Per Step)")]
	[ConfigSlider(1.0, 3.0, 0.01, Format = "{0:0.00}x")]
	public static float HpMultiplier { get; set; } = 1.08f;

	[ConfigSlider(1.0, 2.5, 0.01, Format = "{0:0.00}x")]
	public static float DamageMultiplier { get; set; } = 1.05f;

	[ConfigSection("Multiplier Clamps & Limits")]
	[ConfigSlider(0.0, 5.0, 0.1, Format = "{0:0.0}x")]
	public static float MinHpMultiplier { get; set; } = 1.0f;

	[ConfigSlider(0.0, 50.0, 0.5, Format = "{0:0.0}x")]
	public static float MaxHpMultiplier { get; set; } = 0.0f;

	[ConfigSlider(0.0, 5.0, 0.1, Format = "{0:0.0}x")]
	public static float MinDamageMultiplier { get; set; } = 1.0f;

	[ConfigSlider(0.0, 50.0, 0.5, Format = "{0:0.0}x")]
	public static float MaxDamageMultiplier { get; set; } = 0.0f;

	[ConfigSection("Flat Stat Bonuses (Per Step)")]
	[ConfigSlider(0, 5000, 25, Format = "+{0} HP")]
	public static int FlatHpPerStep { get; set; } = 500;

	[ConfigSlider(0, 50, 1, Format = "+{0} DMG")]
	public static int FlatDamagePerStep { get; set; } = 2;

	[ConfigSection("User Interface")]
	public static bool OverrideHud { get; set; } = true;

	private static bool _syncing;

	public EndlessPlusModConfig()
	{
		ConfigChanged += OnBaseLibConfigChanged;
		OnConfigReloaded += OnBaseLibConfigReloaded;
	}

	/// <summary>
	/// Builds and customizes the Godot UI elements for the Mod Configuration page, injecting the multiplayer synchronization disclaimer.
	/// </summary>
	public override void SetupConfigUI(Control optionContainer)
	{
		RegisterLocalizations();
		base.SetupConfigUI(optionContainer);

		var header = CreateRawLabelControl($"[color=#cfc7b8][b]{EndlessBootstrap.ModTitle}[/b] [color=#806b42]v{EndlessBootstrap.ModVersion}[/color][/color]", 20);
		header.FitContent = true;
		header.CustomMinimumSize = new Vector2(0, 30);
		optionContainer.AddChild(header);
		optionContainer.MoveChild(header, 0);

		var disclaimer = CreateRawLabelControl("[color=#f1c40f][b]Notice:[/b] If playing on multiplayer/co-op, all users must have the same settings to avoid issues![/color]", 18);
		disclaimer.FitContent = true;
		disclaimer.CustomMinimumSize = new Vector2(0, 40);
		optionContainer.AddChild(disclaimer);
		optionContainer.MoveChild(disclaimer, 1);

		var divider = CreateDividerControl();
		optionContainer.AddChild(divider);
		optionContainer.MoveChild(divider, 2);
	}

	private void OnBaseLibConfigChanged(object? sender, EventArgs e)
	{
		if (_syncing) return;
		PushToBaseConfig(saveToFile: true);
	}

	private void OnBaseLibConfigReloaded()
	{
		if (_syncing) return;
		PushToBaseConfig(saveToFile: true);
	}

	/// <summary>
	/// Pushes values from the BaseLib UI sliders into EndlessConfig and writes them to the .config file.
	/// </summary>
	public static void PushToBaseConfig(bool saveToFile)
	{
		_syncing = true;
		try
		{
			EndlessConfig.ApplySettings(
				Interval,
				SkipChapterOne,
				Curve,
				(decimal)HpMultiplier,
				(decimal)DamageMultiplier,
				FlatHpPerStep,
				FlatDamagePerStep,
				AscensionPerLoop,
				MaxAscension,
				OverrideHud,
				(decimal)MinHpMultiplier,
				(decimal)MaxHpMultiplier,
				(decimal)MinDamageMultiplier,
				(decimal)MaxDamageMultiplier,
				saveToFile: saveToFile
			);
		}
		finally
		{
			_syncing = false;
		}
	}

	/// <summary>
	/// Synchronizes BaseLib UI property values from EndlessConfig when the config file is loaded or hot-reloaded.
	/// </summary>
	public static void SyncFromEndlessConfig()
	{
		_syncing = true;
		try
		{
			Interval = EndlessConfig.Interval;
			SkipChapterOne = !EndlessConfig.ScaleChapterOne;
			Curve = EndlessConfig.Curve;
			HpMultiplier = (float)EndlessConfig.EnemyHpScalingPerChapter;
			DamageMultiplier = (float)EndlessConfig.EnemyDamageScalingPerChapter;
			FlatHpPerStep = EndlessConfig.FlatHpPerStep;
			FlatDamagePerStep = EndlessConfig.FlatDmgPerStep;
			MinHpMultiplier = (float)EndlessConfig.MinHpMultiplier;
			MaxHpMultiplier = (float)EndlessConfig.MaxHpMultiplier;
			MinDamageMultiplier = (float)EndlessConfig.MinDmgMultiplier;
			MaxDamageMultiplier = (float)EndlessConfig.MaxDmgMultiplier;
			AscensionPerLoop = EndlessConfig.AscensionPerLoop;
			MaxAscension = EndlessConfig.MaxAscension;
			OverrideHud = EndlessConfig.OverrideHud;
		}
		finally
		{
			_syncing = false;
		}
	}

	/// <summary>
	/// Registers localization string definitions into LocManager for setting titles and hover tooltip descriptions.
	/// </summary>
	public static void RegisterLocalizations()
	{
		try
		{
			if (LocManager.Instance == null) return;
			LocTable table = LocManager.Instance.GetTable("settings_ui");
			if (table == null) return;
			table.MergeWith(new Dictionary<string, string>
			{
				{ "BNKZ.mod_title", $"EndlessPlus v{EndlessBootstrap.ModVersion}" },
				{ "BNKZ-INTERVAL.title", "Scaling Interval" },
				{ "BNKZ-INTERVAL.hover.desc", "Determines how frequently enemy stats scale up: Act transition, Chapter completion, or every Floor." },
				{ "BNKZ-SKIP_CHAPTER_ONE.title", "Start scale on Chapter 2" },
				{ "BNKZ-SKIP_CHAPTER_ONE.hover.desc", "When enabled, Chapter 1 remains at normal vanilla difficulty (1.0x). Scaling begins in Chapter 2." },
				{ "BNKZ-CURVE.title", "Scaling Curve" },
				{ "BNKZ-CURVE.hover.desc", "Math formula for scaling: Exponential (compounds per step), Linear (adds percentage per step), or Fixed." },
				{ "BNKZ-HP_MULTIPLIER.title", "Enemy HP Multiplier" },
				{ "BNKZ-HP_MULTIPLIER.hover.desc", "Monster Max HP percentage multiplier per step (e.g. 1.08 = +8% HP per step)." },
				{ "BNKZ-DAMAGE_MULTIPLIER.title", "Enemy Damage Multiplier" },
				{ "BNKZ-DAMAGE_MULTIPLIER.hover.desc", "Monster Attack Damage percentage multiplier per step (e.g. 1.05 = +5% DMG per step)." },
				{ "BNKZ-MIN_HP_MULTIPLIER.title", "Min HP Multiplier" },
				{ "BNKZ-MIN_HP_MULTIPLIER.hover.desc", "Minimum enemy HP multiplier floor (default 1.0x)." },
				{ "BNKZ-MAX_HP_MULTIPLIER.title", "Max HP Multiplier" },
				{ "BNKZ-MAX_HP_MULTIPLIER.hover.desc", "Maximum ceiling for enemy HP multiplier. Set to 0.0 for uncapped (infinite) scaling." },
				{ "BNKZ-MIN_DAMAGE_MULTIPLIER.title", "Min Damage Multiplier" },
				{ "BNKZ-MIN_DAMAGE_MULTIPLIER.hover.desc", "Minimum enemy attack damage multiplier floor (default 1.0x)." },
				{ "BNKZ-MAX_DAMAGE_MULTIPLIER.title", "Max Damage Multiplier" },
				{ "BNKZ-MAX_DAMAGE_MULTIPLIER.hover.desc", "Maximum ceiling for enemy damage multiplier. Set to 0.0 for uncapped (infinite) scaling." },
				{ "BNKZ-FLAT_HP_PER_STEP.title", "Flat HP Bonus Per Step" },
				{ "BNKZ-FLAT_HP_PER_STEP.hover.desc", "Flat monster HP added per step after multiplier. Useful for scaling in multiplayer parties (e.g. 500)." },
				{ "BNKZ-FLAT_DAMAGE_PER_STEP.title", "Flat Damage Bonus Per Step" },
				{ "BNKZ-FLAT_DAMAGE_PER_STEP.hover.desc", "Flat monster attack damage added per hit per step after multiplier (e.g. 2)." },
				{ "BNKZ-ASCENSION_PER_LOOP.title", "Ascension Per Loop" },
				{ "BNKZ-ASCENSION_PER_LOOP.hover.desc", "Ascension level increase upon completing each full chapter loop (-1 preserves normal vanilla behavior)." },
				{ "BNKZ-MAX_ASCENSION.title", "Max Ascension" },
				{ "BNKZ-MAX_ASCENSION.hover.desc", "Maximum Ascension level ceiling during endless runs (range: 0 to 50)." },
				{ "BNKZ-OVERRIDE_HUD.title", "Show Endless HUD Stats" },
				{ "BNKZ-OVERRIDE_HUD.hover.desc", "Displays current loop, step count, and active stat multipliers in top HUD banner." }
			});
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Could not register mod config localizations: " + ex.Message);
		}
	}
}
