using System;
using System.Reflection;
using Godot;
using HarmonyLib;

namespace WatcherMod;

[HarmonyPatch]
internal static class DieAnimFallbackPatch
{
	internal static MethodBase? TargetMethod()
	{
		Type type = AccessTools.TypeByName("MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimationState");
		if (!(type != null))
		{
			return null;
		}
		return AccessTools.Method(type, "SetAnimation");
	}

	internal static void Prefix(object __instance, ref string animationName)
	{
		if (animationName != "die" || __instance == null)
		{
			return;
		}
		try
		{
			if (__instance.GetType().GetProperty("BoundObject")?.GetValue(__instance) is GodotObject godotObject && DieAnimFallbackRegistry.NeedsFallback(godotObject.GetInstanceId()))
			{
				animationName = "Hit";
			}
		}
		catch
		{
		}
	}
}
