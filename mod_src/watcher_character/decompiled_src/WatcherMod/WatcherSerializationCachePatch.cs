using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace WatcherMod;

[HarmonyPatch(typeof(ModelIdSerializationCache), "Init")]
internal static class WatcherSerializationCachePatch
{
	private static void Postfix()
	{
		Dictionary<string, int> dictionary = typeof(ModelIdSerializationCache).GetField("_entryNameToNetIdMap", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as Dictionary<string, int>;
		List<string> list = typeof(ModelIdSerializationCache).GetField("_netIdToEntryNameMap", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as List<string>;
		Dictionary<string, int> dictionary2 = typeof(ModelIdSerializationCache).GetField("_categoryNameToNetIdMap", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as Dictionary<string, int>;
		List<string> list2 = typeof(ModelIdSerializationCache).GetField("_netIdToCategoryNameMap", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as List<string>;
		if (dictionary == null || list == null || dictionary2 == null || list2 == null)
		{
			return;
		}
		bool flag = false;
		foreach (Type subtypesInMod in ReflectionHelper.GetSubtypesInMods<AbstractModel>())
		{
			ModelId id = ModelDb.GetId(subtypesInMod);
			if (!dictionary2.ContainsKey(id.Category))
			{
				dictionary2[id.Category] = list2.Count;
				list2.Add(id.Category);
				flag = true;
			}
			if (!dictionary.ContainsKey(id.Entry))
			{
				dictionary[id.Entry] = list.Count;
				list.Add(id.Entry);
				flag = true;
			}
		}
		if (flag)
		{
			PropertyInfo? property = typeof(ModelIdSerializationCache).GetProperty("EntryIdBitSize", BindingFlags.Static | BindingFlags.Public);
			PropertyInfo property2 = typeof(ModelIdSerializationCache).GetProperty("CategoryIdBitSize", BindingFlags.Static | BindingFlags.Public);
			property?.SetValue(null, Mathf.CeilToInt(Math.Log2(list.Count)));
			property2?.SetValue(null, Mathf.CeilToInt(Math.Log2(list2.Count)));
		}
	}
}
