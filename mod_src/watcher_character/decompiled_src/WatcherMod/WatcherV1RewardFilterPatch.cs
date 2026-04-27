using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace WatcherMod;

[HarmonyPatch(typeof(CardCreationOptions), "GetPossibleCards")]
internal static class WatcherV1RewardFilterPatch
{
	private static HashSet<ModelId>? _v2OnlyIds;

	private static HashSet<ModelId> GetV2OnlyIds()
	{
		if (_v2OnlyIds != null)
		{
			return _v2OnlyIds;
		}
		WatcherCardPool watcherCardPool = ModelDb.CardPool<WatcherCardPool>();
		WatcherV2CardPool watcherV2CardPool = ModelDb.CardPool<WatcherV2CardPool>();
		if (watcherCardPool == null || watcherV2CardPool == null)
		{
			return _v2OnlyIds = new HashSet<ModelId>();
		}
		HashSet<ModelId> v1Ids = watcherCardPool.AllCardIds.ToHashSet();
		_v2OnlyIds = watcherV2CardPool.AllCardIds.Where((ModelId id) => !v1Ids.Contains(id)).ToHashSet();
		return _v2OnlyIds;
	}

	private static void Postfix(Player player, ref IEnumerable<CardModel> __result)
	{
		if (player?.Character == null || player.Character.GetType() != typeof(Watcher))
		{
			return;
		}
		HashSet<ModelId> blocked = GetV2OnlyIds();
		if (blocked.Count != 0)
		{
			__result = __result.Where((CardModel c) => !blocked.Contains(c.Id));
		}
	}
}
