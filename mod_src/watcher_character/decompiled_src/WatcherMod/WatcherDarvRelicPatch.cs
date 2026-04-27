using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;

namespace WatcherMod;

[HarmonyPatch(typeof(Darv), "GenerateInitialOptions")]
internal static class WatcherDarvRelicPatch
{
	private static bool _injected;

	private static void Prefix(Darv __instance)
	{
		if (_injected)
		{
			return;
		}
		_injected = true;
		if (!(AccessTools.Field(typeof(Darv), "_validRelicSets")?.GetValue(null) is IList list))
		{
			return;
		}
		Type nestedType = typeof(Darv).GetNestedType("ValidRelicSet", BindingFlags.NonPublic);
		if (nestedType == null)
		{
			return;
		}
		Type type = typeof(Func<, >).MakeGenericType(typeof(Player), typeof(bool));
		ConstructorInfo constructor = nestedType.GetConstructor(new Type[2]
		{
			type,
			typeof(RelicModel[])
		});
		if (!(constructor == null))
		{
			Func<Player, bool> func = (Player owner) => owner?.Character is Watcher;
			object value = constructor.Invoke(new object[2]
			{
				func,
				new RelicModel[1] { ModelDb.Relic<VioletLotus>() }
			});
			list.Add(value);
		}
	}

	private static Exception? Finalizer(Exception? __exception, Darv __instance, ref IReadOnlyList<EventOption> __result)
	{
		if (__exception != null)
		{
			GD.PrintErr("[Watcher] Darv.GenerateInitialOptions crashed: " + __exception.Message);
			try
			{
				MethodInfo methodInfo = AccessTools.Method(typeof(AncientEventModel), "RelicOption", new Type[1] { typeof(RelicModel) });
				if (methodInfo == null)
				{
					return __exception;
				}
				RelicModel[] obj = new RelicModel[3]
				{
					ModelDb.Relic<Astrolabe>(),
					ModelDb.Relic<RunicPyramid>(),
					ModelDb.Relic<SneckoEye>()
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
					GD.Print($"[Watcher] Darv fallback: generated {list.Count} relic options");
					return null;
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Watcher] Darv fallback options also failed: " + ex.Message);
			}
		}
		return __exception;
	}
}
