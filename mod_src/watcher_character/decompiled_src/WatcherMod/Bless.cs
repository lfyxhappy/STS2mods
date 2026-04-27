using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class Bless : WatcherCard, IProphecyCard
{
	private const int _threshold = 5;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar("MagicNumber", 3),
		new DamageVar(3m, ValueProp.Unpowered)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<KnowFatePower>(),
		HoverTipFactory.FromPower<BlessProphecyDamagePower>()
	});

	public Bless()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await WatcherCombatHelper.Scry(choiceContext, base.Owner, base.DynamicVars["MagicNumber"].IntValue, this);
		int powerAmount = base.Owner.Creature.GetPowerAmount<KnowFatePower>();
		if (powerAmount < 5)
		{
			return;
		}
		KnowFatePower power = base.Owner.Creature.GetPower<KnowFatePower>();
		if (power != null)
		{
			if (5 < powerAmount)
			{
				await PowerCmd.ModifyAmount(power, -5m, base.Owner.Creature, this);
			}
			else
			{
				await PowerCmd.Remove(power);
			}
		}
		await PowerCmd.Apply<BlessProphecyDamagePower>(base.Owner.Creature, base.DynamicVars.Damage.BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(1m);
	}
}
