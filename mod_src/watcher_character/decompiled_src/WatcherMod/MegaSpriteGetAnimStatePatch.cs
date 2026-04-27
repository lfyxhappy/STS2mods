using System;
using System.Reflection;
using Godot;
using HarmonyLib;

namespace WatcherMod;

[HarmonyPatch]
internal static class MegaSpriteGetAnimStatePatch
{
	internal static MethodBase? TargetMethod()
	{
		Type type = AccessTools.TypeByName("MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite");
		if (!(type != null))
		{
			return null;
		}
		return AccessTools.Method(type, "GetAnimationState");
	}

	internal static void Postfix(object __instance, object? __result)
	{
		if (__result == null || __instance == null)
		{
			return;
		}
		try
		{
			MethodInfo method = __instance.GetType().GetMethod("HasAnimation", new Type[1] { typeof(string) });
			if (!(method == null) && !(bool)(method.Invoke(__instance, new object[1] { "die" }) ?? ((object)false)) && __result.GetType().GetProperty("BoundObject")?.GetValue(__result) is GodotObject godotObject)
			{
				DieAnimFallbackRegistry.Register(godotObject.GetInstanceId());
			}
		}
		catch
		{
		}
	}
}
