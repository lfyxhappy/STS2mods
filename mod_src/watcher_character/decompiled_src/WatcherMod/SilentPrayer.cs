using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace WatcherMod;

public sealed class SilentPrayer : WatcherCard, IProphecyCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar("MagicNumber", 2),
		new DynamicVar("MantraYield", 1m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<KnowFatePower>(),
		HoverTipFactory.FromPower<Mantra>()
	});

	public SilentPrayer()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await WatcherCombatHelper.Scry(choiceContext, base.Owner, base.DynamicVars["MagicNumber"].IntValue, this);
		int powerAmount = base.Owner.Creature.GetPowerAmount<KnowFatePower>();
		int num = powerAmount / 2 * 2;
		if (num <= 0)
		{
			return;
		}
		int mantra = num / 2 * base.DynamicVars["MantraYield"].IntValue;
		KnowFatePower power = base.Owner.Creature.GetPower<KnowFatePower>();
		if (power != null)
		{
			if (num < powerAmount)
			{
				await PowerCmd.ModifyAmount(power, -num, base.Owner.Creature, this);
			}
			else
			{
				await PowerCmd.Remove(power);
			}
		}
		if (mantra > 0)
		{
			await WatcherCombatHelper.GainMantra(base.Owner, mantra, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MantraYield"].UpgradeValueBy(1m);
	}
}
