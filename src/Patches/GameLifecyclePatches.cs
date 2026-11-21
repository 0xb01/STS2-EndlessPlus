using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace BNKZ.STS2.EndlessPlus;

internal static class GameLifecyclePatches
{
	private sealed record SaveFileSnapshot(string Path, byte[] Bytes, DateTime LastWriteTimeUtc);

	private static readonly object CloudSyncSnapshotLock = new object();

	private static List<SaveFileSnapshot>? _cloudSyncSnapshot;

	private static int _cloudSyncDepth;

	public static bool BeforeQuit()
	{
		if (!EndlessRuntime.IsLaunchingFromGameOverScreen)
		{
			return true;
		}
		EndlessLog.Info("Blocked NGame.Quit while endless loop start is in progress.");
		return false;
	}

	public static void BeforeGetRejoinMessage(object __instance)
	{
		try
		{
			object runState = EndlessReflection.GetRunState(__instance);
			MigrateLoadedMultiplayerScalingMetadata(__instance, runState, requireHost: false, "before sending multiplayer rejoin message");
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while preparing endless scaling metadata for multiplayer rejoin message.", ex);
		}
	}

	public static void AfterToSave(object __instance, object? __result)
	{
		if (__result == null)
		{
			return;
		}
		try
		{
			object runState = EndlessReflection.GetRunState(__instance);
			PersistScalingMetadataToSave(__instance, runState, __result, "RunManager.ToSave");
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while writing endless scaling metadata into the serialized run save.", ex);
		}
	}

	public static void AfterSetUpSavedMultiplayer(object __instance, object state)
	{
		try
		{
			MigrateLoadedMultiplayerScalingMetadata(__instance, state, requireHost: true, "after loading saved multiplayer run");
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while migrating loaded multiplayer save to endless scaling metadata.", ex);
		}
	}

	private static void MigrateLoadedMultiplayerScalingMetadata(object runManager, object runState, bool requireHost, string source)
	{
		if (EndlessReflection.GetPlayersCount(runState) > 1 && !EndlessReflection.TryGetPersistedEndlessSettings(runState, out var _))
		{
			if (requireHost && !EndlessReflection.IsLocalPlayerHost(runManager))
			{
				EndlessLog.Info("Loaded an older multiplayer endless save without scaling metadata on a client; waiting for host-authored metadata.");
				return;
			}
			int valueOrDefault = EndlessReflection.GetPersistedEndlessLoopCount(runState).GetValueOrDefault();
			EndlessScalingSettings hostConfig = EndlessScalingSettings.FromConfig(valueOrDefault);
			EndlessReflection.SetPersistedEndlessSettings(runState, hostConfig);
			EndlessLog.Info($"Migrated older multiplayer endless save to host scaling metadata {source}: loop={valueOrDefault}, interval={hostConfig.Interval}, curve={hostConfig.Curve}, hp_scale={hostConfig.HpScale:0.###}, dmg_scale={hostConfig.DamageScale:0.###}.");
		}
	}

	private static void PersistScalingMetadataToSave(object runManager, object runState, object save, string source)
	{
		if (EndlessReflection.GetPlayersCount(runState) > 1)
		{
			int valueOrDefault = EndlessReflection.GetPersistedEndlessLoopCount(runState).GetValueOrDefault();
			if (EndlessReflection.TryGetPersistedEndlessSettings(runState, out var settings))
			{
				settings.LoopCount = valueOrDefault;
				EndlessReflection.SetPersistedEndlessSettingsOnSave(save, settings);
				EndlessLog.Info($"Persisted existing endless multiplayer scaling metadata into save via {source}: loop={valueOrDefault}, hp_scale={settings.HpScale:0.###}, dmg_scale={settings.DamageScale:0.###}.");
			}
			else if (!EndlessReflection.IsLocalPlayerHost(runManager))
			{
				EndlessReflection.SetPersistedEndlessLoopCountOnSave(save, valueOrDefault);
				EndlessLog.Info("Saved multiplayer endless loop count without scaling metadata on a client via " + source + "; host metadata was not available.");
			}
			else
			{
				EndlessScalingSettings hostConfig = EndlessScalingSettings.FromConfig(valueOrDefault);
				EndlessReflection.SetPersistedEndlessSettings(runState, hostConfig);
				EndlessReflection.SetPersistedEndlessSettingsOnSave(save, hostConfig);
				EndlessLog.Info($"Persisted host endless multiplayer scaling metadata into save via {source}: loop={valueOrDefault}, interval={hostConfig.Interval}, curve={hostConfig.Curve}, hp_scale={hostConfig.HpScale:0.###}, dmg_scale={hostConfig.DamageScale:0.###}.");
			}
		}
	}

	public static void BeforeSyncCloudToLocal()
	{
		lock (CloudSyncSnapshotLock)
		{
			_cloudSyncDepth++;
			if (_cloudSyncDepth > 1)
			{
				return;
			}
			try
			{
				_cloudSyncSnapshot = CaptureProtectedSavesSnapshot();
			}
			catch (Exception ex)
			{
				_cloudSyncSnapshot = null;
				EndlessLog.Error("Failed while snapshotting local saves before cloud sync.", ex);
			}
		}
	}

	public static Exception? FinalizeSyncCloudToLocal(Exception? __exception)
	{
		lock (CloudSyncSnapshotLock)
		{
			_cloudSyncDepth--;
			if (_cloudSyncDepth > 0)
			{
				return __exception;
			}
			if (_cloudSyncDepth < 0)
			{
				_cloudSyncDepth = 0;
			}
			try
			{
				RestoreProtectedSavesSnapshot(_cloudSyncSnapshot);
			}
			catch (Exception ex)
			{
				EndlessLog.Error("Failed while restoring local saves after cloud sync.", ex);
			}
			finally
			{
				_cloudSyncSnapshot = null;
			}
		}
		return __exception;
	}

	public static bool BeforeWriteFile(string path)
	{
		try
		{
			if (!ShouldPreserveCurrentRunDuringCloudSync(path))
			{
				return true;
			}
			EndlessLog.Info("Prevented cloud sync from overwriting local save '" + path + "' because a local endless run save exists.");
			return false;
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while evaluating whether save write should be blocked for '" + path + "'.", ex);
			return true;
		}
	}

	public static bool BeforeDeleteFile(string path)
	{
		try
		{
			if (!ShouldPreserveCurrentRunDuringCloudSync(path))
			{
				return true;
			}
			EndlessLog.Info("Prevented cloud sync from deleting local save '" + path + "' because a local endless run save exists.");
			return false;
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while evaluating whether save deletion should be blocked for '" + path + "'.", ex);
			return true;
		}
	}

	public static bool BeforeNetGameDisconnect(object reason, bool now)
	{
		if (!EndlessRuntime.ShouldPreserveMultiplayerConnectionDuringGameOver)
		{
			return true;
		}
		EndlessLog.Info($"Blocked multiplayer disconnect while holding the endless game-over lobby open. Reason={reason}, now={now}.");
		return false;
	}

	private static bool ShouldPreserveCurrentRunDuringCloudSync(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return false;
		}
		string normalizedRelativePath = NormalizeRelativePath(path);
		if (!IsProtectedSavesCloudSyncPath(normalizedRelativePath))
		{
			return false;
		}
		if (!IsCloudSyncToLocalStack())
		{
			return false;
		}
		string localPath;
		return TryFindExistingEndlessRunSaveForCloudPath(normalizedRelativePath, out localPath);
	}

	private static bool IsProtectedSavesCloudSyncPath(string normalizedRelativePath)
	{
		if (string.IsNullOrWhiteSpace(normalizedRelativePath))
		{
			return false;
		}
		string[] source = normalizedRelativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
		return source.Any((string part) => string.Equals(part, "saves", StringComparison.OrdinalIgnoreCase));
	}

	private static bool IsProtectedCurrentRunSave(string? fileName)
	{
		return ProtectedCurrentRunFileNames().Any((string candidateName) => string.Equals(fileName, candidateName, StringComparison.OrdinalIgnoreCase));
	}

	private static bool IsCloudSyncToLocalStack()
	{
		string stackTrace = System.Environment.StackTrace;
		return stackTrace.Contains("CloudSaveStore.SyncCloudToLocalInternal", StringComparison.Ordinal) || stackTrace.Contains("SaveManager.SyncCloudToLocal", StringComparison.Ordinal);
	}

	private static List<SaveFileSnapshot> CaptureProtectedSavesSnapshot()
	{
		List<SaveFileSnapshot> list = new List<SaveFileSnapshot>();
		foreach (string item in FindLocalSavesDirectoriesWithCurrentRun())
		{
			foreach (string item2 in Directory.EnumerateFiles(item, "*", SearchOption.AllDirectories))
			{
				try
				{
					list.Add(new SaveFileSnapshot(item2, File.ReadAllBytes(item2), File.GetLastWriteTimeUtc(item2)));
				}
				catch (Exception ex)
				{
					EndlessLog.Error("Failed to snapshot local save file '" + item2 + "'.", ex);
				}
			}
		}
		if (list.Count > 0)
		{
			EndlessLog.Info($"Snapshotted {list.Count} local save file(s) before cloud sync so the local endless run remains authoritative.");
		}
		return list;
	}

	private static void RestoreProtectedSavesSnapshot(List<SaveFileSnapshot>? snapshots)
	{
		if (snapshots == null || snapshots.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (SaveFileSnapshot snapshot in snapshots)
		{
			try
			{
				string directoryName = Path.GetDirectoryName(snapshot.Path);
				if (!string.IsNullOrWhiteSpace(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				File.WriteAllBytes(snapshot.Path, snapshot.Bytes);
				File.SetLastWriteTimeUtc(snapshot.Path, snapshot.LastWriteTimeUtc);
				num++;
			}
			catch (Exception ex)
			{
				EndlessLog.Error("Failed to restore local save file '" + snapshot.Path + "' after cloud sync.", ex);
			}
		}
		EndlessLog.Info($"Restored {num}/{snapshots.Count} local save file(s) after cloud sync to prevent Steam Cloud from replacing the endless run.");
	}

	private static IEnumerable<string> FindLocalSavesDirectoriesWithCurrentRun()
	{
		string userRoot = ProjectSettings.GlobalizePath("user://");
		if (string.IsNullOrWhiteSpace(userRoot) || !Directory.Exists(userRoot))
		{
			yield break;
		}
		IEnumerable<string> saveDirectories;
		try
		{
			saveDirectories = Directory.EnumerateDirectories(userRoot, "saves", SearchOption.AllDirectories).ToList();
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			EndlessLog.Error("Failed to enumerate save directories under '" + userRoot + "'.", ex2);
			yield break;
		}
		foreach (string savesDirectory in saveDirectories)
		{
			if (HasProtectedCurrentRunSave(savesDirectory))
			{
				yield return savesDirectory;
			}
		}
	}

	private static bool HasProtectedCurrentRunSave(string savesDirectory)
	{
		foreach (string item in ProtectedCurrentRunFileNames())
		{
			if (File.Exists(Path.Combine(savesDirectory, item)))
			{
				return true;
			}
		}
		return false;
	}

	private static bool TryFindExistingEndlessRunSaveForCloudPath(string normalizedRelativePath, out string localPath)
	{
		localPath = string.Empty;
		if (!TryFindExistingLocalSavesDirectory(normalizedRelativePath, out string localPath2))
		{
			return false;
		}
		foreach (string item in ProtectedCurrentRunFileNames())
		{
			string text = Path.Combine(localPath2, item);
			if (!File.Exists(text))
			{
				continue;
			}
			localPath = text;
			return true;
		}
		return false;
	}

	private static IEnumerable<string> ProtectedCurrentRunFileNames()
	{
		yield return "current_run.save";
		yield return "current_run.save.backup";
		yield return "current_run_mp.save";
		yield return "current_run_mp.save.backup";
	}

	private static bool TryFindExistingLocalSavesDirectory(string normalizedRelativePath, out string localPath)
	{
		localPath = string.Empty;
		string text = ProjectSettings.GlobalizePath("user://");
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		string savesRelativePath = GetSavesRelativePath(normalizedRelativePath);
		if (savesRelativePath == null)
		{
			return false;
		}
		string text2 = Path.Combine(text, savesRelativePath);
		if (Directory.Exists(text2))
		{
			localPath = text2;
			return true;
		}
		string path = Path.Combine(text, "steam");
		if (!Directory.Exists(path))
		{
			return false;
		}
		foreach (string item in Directory.EnumerateDirectories(path))
		{
			string text3 = Path.Combine(item, savesRelativePath);
			if (!Directory.Exists(text3))
			{
				continue;
			}
			localPath = text3;
			return true;
		}
		return false;
	}

	private static string NormalizeRelativePath(string path)
	{
		return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
	}

	private static string? GetSavesRelativePath(string normalizedRelativePath)
	{
		string[] array = normalizedRelativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			if (string.Equals(array[i], "saves", StringComparison.OrdinalIgnoreCase))
			{
				return Path.Combine(array.Take(i + 1).ToArray());
			}
		}
		return null;
	}
}
