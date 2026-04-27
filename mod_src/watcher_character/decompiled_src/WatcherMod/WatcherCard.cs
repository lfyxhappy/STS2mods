using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public abstract class WatcherCard : CardModel
{
	public override CardPoolModel Pool => ModelDb.CardPool<WatcherCardPool>();

	public override string PortraitPath => "res://images/packed/card_portraits/watcher/" + base.Id.Entry.ToLower() + ".png";

	public override string BetaPortraitPath => "res://images/packed/card_portraits/watcher/beta/" + base.Id.Entry.ToLower() + ".png";

	public override IEnumerable<string> AllPortraitPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { PortraitPath, BetaPortraitPath });

	protected WatcherCard(int canonicalEnergyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary = true)
		: base(canonicalEnergyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}
}
