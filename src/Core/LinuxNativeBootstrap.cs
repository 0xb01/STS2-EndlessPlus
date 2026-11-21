using System;
using System.Runtime.InteropServices;
using Godot;

namespace BNKZ.STS2.EndlessPlus;

internal static class LinuxNativeBootstrap
{
	private const int RtldNow = 2;

	private const int RtldGlobal = 256;

	private static bool _prepared;

	public static void PrepareForHarmony()
	{
		if (_prepared || !OS.GetName().Equals("Linux", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		_prepared = true;
		try
		{
			LoadGlobal("libgcc_s.so.1");
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed to preload Linux native dependencies for Harmony.", ex);
		}
	}

	private static void LoadGlobal(string libraryName)
	{
		nint num = dlopen(libraryName, 258);
		if (num != IntPtr.Zero)
		{
			EndlessLog.Info("Preloaded native library '" + libraryName + "' for Harmony.");
			return;
		}
		string text = Marshal.PtrToStringAnsi(dlerror()) ?? "unknown dlopen error";
		throw new InvalidOperationException("dlopen(" + libraryName + ") failed: " + text);
	}

	[DllImport("libdl.so.2", CharSet = CharSet.Ansi)]
	private static extern nint dlopen(string fileName, int flags);

	[DllImport("libdl.so.2")]
	private static extern nint dlerror();
}
