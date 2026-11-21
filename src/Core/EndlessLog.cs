using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Godot;

namespace BNKZ.STS2.EndlessPlus;

internal static class EndlessLog
{
	private const string Prefix = "[BNKZ.EndlessPlus]";

	public static void Info(string message)
	{
		GD.Print(FormattableString.Invariant(FormattableStringFactory.Create("{0} {1}", Prefix, message)));
	}

	public static void Warn(string message)
	{
		GD.Print(FormattableString.Invariant(FormattableStringFactory.Create("{0} WARN: {1}", Prefix, message)));
	}

	public static void Error(string message)
	{
		GD.PrintErr(FormattableString.Invariant(FormattableStringFactory.Create("{0} ERROR: {1}", Prefix, message)));
	}

	public static void Error(string message, Exception ex)
	{
		IFormatProvider invariantCulture = CultureInfo.InvariantCulture;
		DefaultInterpolatedStringHandler handler = new DefaultInterpolatedStringHandler(9, 3, invariantCulture);
		handler.AppendFormatted(Prefix);
		handler.AppendLiteral(" ERROR: ");
		handler.AppendFormatted(message);
		handler.AppendLiteral("\n");
		handler.AppendFormatted(ex);
		GD.PrintErr(string.Create(invariantCulture, ref handler));
	}
}
