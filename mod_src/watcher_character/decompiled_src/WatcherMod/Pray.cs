using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace WatcherMod;

public sealed class Pray : WatcherCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<Mantra>(),
		HoverTipFactory.FromCard<Insight>()
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("MagicNumber", 3m));

	public Pray()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await WatcherCombatHelper.GainMantra(base.Owner, base.DynamicVars["MagicNumber"].IntValue, this);
		await CardPileCmd.AddGeneratedCardToCombat(await WatcherCombatHelper.CreateWatcherCard<Insight>(base.Owner), PileType.Draw, addedByPlayer: true, CardPilePosition.Random);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(1m);
	}
}
