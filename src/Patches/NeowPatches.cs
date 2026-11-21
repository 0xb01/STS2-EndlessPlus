using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace BNKZ.STS2.EndlessPlus;

internal static class NeowPatches
{
	private static readonly Type NeowType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Models.Events.Neow") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Models.Events.Neow");

	private static readonly Type EventOptionType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Events.EventOption") ?? throw new MissingMemberException("MegaCrit.Sts2.Core.Events.EventOption");

	private static readonly PropertyInfo NeowAllPossibleOptions = AccessTools.Property(NeowType, "AllPossibleOptions") ?? throw new MissingMemberException(NeowType.FullName, "AllPossibleOptions");

	private static readonly PropertyInfo AncientEventGeneratedOptions = AccessTools.Property(NeowType, "GeneratedOptions") ?? throw new MissingMemberException(NeowType.FullName, "GeneratedOptions");

	private static readonly PropertyInfo EventOptionTextKey = AccessTools.Property(EventOptionType, "TextKey") ?? throw new MissingMemberException(EventOptionType.FullName, "TextKey");

	private static readonly PropertyInfo EventOptionRelic = AccessTools.Property(EventOptionType, "Relic") ?? throw new MissingMemberException(EventOptionType.FullName, "Relic");

	private static readonly PropertyInfo EventOptionTitle = AccessTools.Property(EventOptionType, "Title") ?? throw new MissingMemberException(EventOptionType.FullName, "Title");

	public static void AfterGenerateInitialOptions(object __instance, ref object? __result)
	{
		if (__instance == null || !(__result is IEnumerable generatedOptions))
		{
			return;
		}
		if (TryRerollEndlessOptions(__instance, generatedOptions, out object rerolledOptions, out string rerollSummary))
		{
			AncientEventGeneratedOptions.SetValue(__instance, rerolledOptions);
			__result = rerolledOptions;
			EndlessLog.Info("Rerolled Neow endless options: " + rerollSummary + ".");
		}
		if (__result is IEnumerable options)
		{
			List<string> list = DescribeOptions(options);
			if (list.Count != 0)
			{
				EndlessLog.Info("Neow generated options: " + string.Join("; ", list) + ".");
			}
		}
	}

	private static bool TryRerollEndlessOptions(object neow, IEnumerable generatedOptions, out object rerolledOptions, out string rerollSummary)
	{
		rerolledOptions = null;
		rerollSummary = string.Empty;
		object runManagerInstance = EndlessReflection.GetRunManagerInstance();
		object runState = EndlessReflection.GetRunState(runManagerInstance);
		if (EndlessRuntime.GetCompletedLoops(runState) <= 0)
		{
			return false;
		}
		List<object> list = (from object option in generatedOptions
			where option != null
			select option).ToList();
		if (list.Count == 0)
		{
			return false;
		}
		List<object> candidateOptions = GetCandidateOptions(neow);
		if (candidateOptions.Count <= list.Count)
		{
			EndlessLog.Info($"Skipped Neow endless reroll because the candidate pool only had {candidateOptions.Count} option(s): {string.Join("; ", DescribeOptions(candidateOptions))}.");
			return false;
		}
		string loopSeed = $"{EndlessReflection.GetRunSeedString(runState)}|neow-options|{EndlessRuntime.GetCompletedLoops(runState)}|{EndlessReflection.GetTotalFloor(runState)}";
		List<object> source = (from entry in (from entry in candidateOptions.Select((object option, int index) => new
				{
					Option = option,
					SortKey = ComputeDeterministicHash(loopSeed, DescribeOption(option), index)
				})
				orderby entry.SortKey
				select entry).ThenBy(entry => DescribeOption(entry.Option), StringComparer.Ordinal)
			select entry.Option).ToList();
		List<object> list2 = source.Take(list.Count).ToList();
		if (HaveSameOptionIdentity(list2, list))
		{
			list2 = source.Skip(1).Concat(source.Take(1)).Take(list.Count)
				.ToList();
		}
		if (HaveSameOptionIdentity(list2, list))
		{
			return false;
		}
		rerolledOptions = CreateOptionList(list2);
		rerollSummary = string.Join("; ", DescribeOptions(list2));
		return true;
	}

	private static List<object> GetCandidateOptions(object neow)
	{
		List<object> list = new List<object>();
		foreach (PropertyInfo neowOptionProperty in GetNeowOptionProperties())
		{
			object value;
			try
			{
				value = neowOptionProperty.GetValue(neow);
			}
			catch
			{
				continue;
			}
			if (value != null)
			{
				if (EventOptionType.IsAssignableFrom(neowOptionProperty.PropertyType))
				{
					AddOption(list, value);
				}
				else if (value is IEnumerable source)
				{
					AddOptions(list, source);
				}
			}
		}
		IEnumerable source2 = NeowAllPossibleOptions.GetValue(neow) as IEnumerable;
		AddOptions(list, source2);
		if (list.Count == 0)
		{
			return new List<object>();
		}
		list = list.Where((object option) => option != null).DistinctBy(DescribeOption).ToList();
		List<object> list2 = list.Where((object option) => EventOptionRelic.GetValue(option) != null).ToList();
		return (list2.Count > 0) ? list2 : list;
	}

	private static IEnumerable<PropertyInfo> GetNeowOptionProperties()
	{
		return NeowType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where((PropertyInfo property) =>
		{
			if (!property.CanRead || property.GetIndexParameters().Length != 0)
			{
				return false;
			}
			bool flag;
			switch (property.Name)
			{
			case "AllPossibleOptions":
			case "CurrentOptions":
			case "GeneratedOptions":
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				return false;
			}
			if (EventOptionType.IsAssignableFrom(property.PropertyType))
			{
				return true;
			}
			return typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType.IsGenericType && property.PropertyType.GetGenericArguments().Any((Type argument) => argument == EventOptionType);
		});
	}

	private static void AddOptions(ICollection<object> destination, IEnumerable? source)
	{
		if (source == null)
		{
			return;
		}
		foreach (object item in source)
		{
			AddOption(destination, item);
		}
	}

	private static void AddOption(ICollection<object> destination, object? option)
	{
		if (option != null)
		{
			destination.Add(option);
		}
	}

	private static object CreateOptionList(IReadOnlyList<object> options)
	{
		Type type = typeof(List<>).MakeGenericType(EventOptionType);
		IList list = (IList)(Activator.CreateInstance(type) ?? throw new InvalidOperationException("Could not create Neow option list."));
		foreach (object option in options)
		{
			list.Add(option);
		}
		return list;
	}

	private static bool HaveSameOptionIdentity(IReadOnlyList<object> left, IReadOnlyList<object> right)
	{
		if (left.Count != right.Count)
		{
			return false;
		}
		for (int i = 0; i < left.Count; i++)
		{
			if (!string.Equals(DescribeOption(left[i]), DescribeOption(right[i]), StringComparison.Ordinal))
			{
				return false;
			}
		}
		return true;
	}

	private static List<string> DescribeOptions(IEnumerable options)
	{
		List<string> list = new List<string>();
		foreach (object option in options)
		{
			if (option != null)
			{
				list.Add(DescribeOption(option));
			}
		}
		return list;
	}

	private static string DescribeOption(object option)
	{
		string text = EventOptionTextKey.GetValue(option)?.ToString() ?? "<no-text-key>";
		string text2 = EventOptionTitle.GetValue(option)?.ToString() ?? "<no-title>";
		object value = EventOptionRelic.GetValue(option);
		if (value != null)
		{
			return $"{text}|{text2}|relic={EndlessReflection.GetModelId(value)}";
		}
		return text + "|" + text2;
	}

	private static ulong ComputeDeterministicHash(string seed, string optionIdentity, int index)
	{
		ulong num = 14695981039346656037uL;
		foreach (char c in seed)
		{
			num ^= c;
			num *= 1099511628211L;
		}
		num ^= (uint)index;
		num *= 1099511628211L;
		foreach (char c2 in optionIdentity)
		{
			num ^= c2;
			num *= 1099511628211L;
		}
		return num;
	}
}
