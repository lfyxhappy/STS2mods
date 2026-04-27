using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class WatcherV2 : Watcher
{
	public override CardPoolModel CardPool => ModelDb.CardPool<WatcherV2CardPool>();

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<WatcherV2RelicPool>();

	public override IEnumerable<CardModel> StartingDeck => new global::_003C_003Ez__ReadOnlyArray<CardModel>(new CardModel[10]
	{
		ModelDb.Card<Strike_P>(),
		ModelDb.Card<Strike_P>(),
		ModelDb.Card<Strike_P>(),
		ModelDb.Card<Conviction>(),
		ModelDb.Card<Defend_P>(),
		ModelDb.Card<Defend_P>(),
		ModelDb.Card<Defend_P>(),
		ModelDb.Card<Prescience>(),
		ModelDb.Card<Eruption_P>(),
		ModelDb.Card<Vigilance>()
	});

	public override IReadOnlyList<RelicModel> StartingRelics => new global::_003C_003Ez__ReadOnlySingleElementList<RelicModel>(ModelDb.Relic<ProphetWater>());
}
