using System;
using System.Runtime.CompilerServices;

namespace BNKZ.STS2.EndlessPlus;

/// <summary>
/// Handles optional integration with BaseLib for in-game Mod Configuration UI.
/// If BaseLib is not present, all methods safely no-op and file-based config is used.
/// </summary>
public static class BaseLibIntegration
{
	private static bool? _isAvailable;

	/// <summary>
	/// Gets whether the BaseLib assembly is currently loaded in the application domain.
	/// </summary>
	public static bool IsAvailable
	{
		get
		{
			if (!_isAvailable.HasValue)
			{
				_isAvailable = CheckBaseLibLoaded();
			}
			return _isAvailable.Value;
		}
	}

	private static bool CheckBaseLibLoaded()
	{
		try
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (string.Equals(assembly.GetName().Name, "BaseLib", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		catch
		{
			// Ignore reflection errors during early assembly inspection
		}
		return false;
	}

	/// <summary>
	/// Registers EndlessPlus with BaseLib ModConfig registry if BaseLib is installed.
	/// </summary>
	public static void TryRegister()
	{
		if (!IsAvailable)
		{
			EndlessLog.Info("BaseLib not detected. In-game config UI disabled (file-based config remains fully functional).");
			return;
		}

		try
		{
			RegisterInternal();
		}
		catch (Exception ex)
		{
			EndlessLog.Warn("Failed to register with BaseLib ModConfig: " + ex.Message);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void RegisterInternal()
	{
		BaseLib.Config.ModConfigRegistry.Register("BNKZ.STS2.EndlessPlus", new EndlessPlusModConfig());
		EndlessPlusModConfig.SyncFromEndlessConfig();
		EndlessLog.Info("Registered EndlessPlus with BaseLib Mod Configuration.");
	}

	/// <summary>
	/// Synchronizes BaseLib UI property values from EndlessConfig when config reloads.
	/// </summary>
	public static void TrySyncFromEndlessConfig()
	{
		if (!IsAvailable) return;

		try
		{
			SyncInternal();
		}
		catch
		{
			// Safe fallback if BaseLib UI throws
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void SyncInternal()
	{
		EndlessPlusModConfig.SyncFromEndlessConfig();
	}

	/// <summary>
	/// Harmony postfix for LocManager.Initialize to register setting titles and descriptions.
	/// </summary>
	public static void OnLocManagerInitialize()
	{
		if (!IsAvailable) return;

		try
		{
			RegisterLocalizationsInternal();
		}
		catch
		{
			// Safe fallback
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void RegisterLocalizationsInternal()
	{
		EndlessPlusModConfig.RegisterLocalizations();
	}
}
