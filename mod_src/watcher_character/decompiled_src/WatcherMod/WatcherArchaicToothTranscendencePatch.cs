using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace WatcherMod;

[HarmonyPatch(typeof(ArchaicTooth), "TranscendenceUpgrades", MethodType.Getter)]
internal static class WatcherArchaicToothTranscendencePatch
{
	private static void Postfix(ref Dictionary<ModelId, CardModel> __result)
	{
		if (__result != null)
		{
			__result[ModelDb.Card<Eruption_P>().Id] = ModelDb.Card<Cataclysm>();
			__result[ModelDb.Card<Vigilance>().Id] = ModelDb.Card<Serenity>();
		}
	}
}
