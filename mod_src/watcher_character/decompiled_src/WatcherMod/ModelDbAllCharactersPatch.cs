using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(ModelDb), "get_AllCharacters")]
internal static class ModelDbAllCharactersPatch
{
	private static void Postfix(ref IEnumerable<CharacterModel> __result)
	{
		List<CharacterModel> list = new List<CharacterModel>();
		CharacterModel byIdOrNull = ModelDb.GetByIdOrNull<CharacterModel>(ModelDb.GetId(typeof(Watcher)));
		if (byIdOrNull != null)
		{
			list.Add(byIdOrNull);
		}
		if (WatcherModSettings.EnableV2Watcher)
		{
			CharacterModel byIdOrNull2 = ModelDb.GetByIdOrNull<CharacterModel>(ModelDb.GetId(typeof(WatcherV2)));
			if (byIdOrNull2 != null)
			{
				list.Add(byIdOrNull2);
			}
		}
		if (list.Count != 0)
		{
			__result = __result.Concat(list).Distinct().ToArray();
		}
	}
}
