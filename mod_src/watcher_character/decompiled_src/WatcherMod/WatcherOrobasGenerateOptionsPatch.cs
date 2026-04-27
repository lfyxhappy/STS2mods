using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;

namespace WatcherMod;

[HarmonyPatch(typeof(Orobas), "GenerateInitialOptions")]
internal static class WatcherOrobasGenerateOptionsPatch
{
	private static Exception? Finalizer(Exception? __exception, Orobas __instance, ref IReadOnlyList<EventOption> __result)
	{
		if (__exception != null)
		{
			GD.PrintErr("[Watcher] Orobas.GenerateInitialOptions crashed: " + __exception.Message);
			try
			{
				MethodInfo methodInfo = AccessTools.Method(typeof(AncientEventModel), "RelicOption", new Type[1] { typeof(RelicModel) });
				if (methodInfo == null)
				{
					return __exception;
				}
				RelicModel[] obj = new RelicModel[3]
				{
					ModelDb.Relic<ElectricShrymp>(),
					ModelDb.Relic<GlassEye>(),
					ModelDb.Relic<SandCastle>()
				};
				List<EventOption> list = new List<EventOption>();
				RelicModel[] array = obj;
				foreach (RelicModel relicModel in array)
				{
					if (methodInfo.Invoke(__instance, new object[1] { relicModel.ToMutable() }) is EventOption item)
					{
						list.Add(item);
					}
				}
				if (list.Count > 0)
				{
					__result = list;
					GD.Print($"[Watcher] Orobas fallback: generated {list.Count} relic options");
					return null;
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Watcher] Orobas fallback failed: " + ex.Message);
			}
		}
		return __exception;
	}
}
