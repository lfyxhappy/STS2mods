using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class KarmaForeseenPower : PowerModel
{
	private const string _threshold = "Threshold";

	private const string _damageBonus = "DamageBonus";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("Threshold", 3m),
		new DynamicVar("DamageBonus", 4m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<KnowFatePower>());

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (dealer != base.Owner)
		{
			return 0m;
		}
		if (!props.IsPoweredAttack())
		{
			return 0m;
		}
		if (cardSource == null)
		{
			return 0m;
		}
		int powerAmount = base.Owner.GetPowerAmount<KnowFatePower>();
		int intValue = base.DynamicVars["Threshold"].IntValue;
		if (powerAmount < intValue)
		{
			return 0m;
		}
		return powerAmount / intValue * base.DynamicVars["DamageBonus"].IntValue;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner.Player || cardPlay.Card.Type != CardType.Attack || cardPlay.Card is Verdict)
		{
			return;
		}
		int powerAmount = base.Owner.GetPowerAmount<KnowFatePower>();
		int intValue = base.DynamicVars["Threshold"].IntValue;
		if (powerAmount < intValue)
		{
			return;
		}
		int num = powerAmount / intValue * intValue;
		KnowFatePower power = base.Owner.GetPower<KnowFatePower>();
		if (power != null)
		{
			if (num >= powerAmount)
			{
				await PowerCmd.Remove(power);
			}
			else
			{
				await PowerCmd.ModifyAmount(power, -num, base.Owner, null);
			}
		}
	}
}
