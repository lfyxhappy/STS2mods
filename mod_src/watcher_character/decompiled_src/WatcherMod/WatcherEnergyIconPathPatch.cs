using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;

namespace WatcherMod;

[HarmonyPatch(typeof(EnergyIconHelper), "GetPath", new Type[] { typeof(string) })]
internal static class WatcherEnergyIconPathPatch
{
	private static bool Prefix(string prefix, ref string __result)
	{
		if (prefix == "watcher")
		{
			__result = "res://images/watcher/card_purple_orb.png";
			return false;
		}
		return true;
	}
}
