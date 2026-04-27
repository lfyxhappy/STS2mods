using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(PowerModel), "get_BigIcon")]
internal static class WatcherPowerBigIconPatch
{
	private static readonly Assembly WatcherAssembly = typeof(Watcher).Assembly;

	private static bool Prefix(PowerModel __instance, ref Texture2D __result)
	{
		if (__instance.GetType().Assembly != WatcherAssembly)
		{
			return true;
		}
		Texture2D texture2D = WatcherTextureHelper.LoadTexture("res://images/powers/" + __instance.Id.Entry.ToLower() + ".png");
		if (texture2D != null)
		{
			__result = texture2D;
			return false;
		}
		return true;
	}
}
