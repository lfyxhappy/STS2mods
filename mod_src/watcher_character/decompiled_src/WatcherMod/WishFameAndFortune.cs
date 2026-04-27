using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace WatcherMod;

public sealed class WishFameAndFortune : WatcherCard
{
	public override CardPoolModel Pool => ModelDb.CardPool<ColorlessCardPool>();

	public override string PortraitPath => "res://images/packed/card_portraits/watcher/fame_and_fortune.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("MagicNumber", 25m));

	public WishFameAndFortune()
		: base(-2, CardType.Skill, CardRarity.Token, TargetType.None)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return Task.CompletedTask;
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(5m);
	}
}
