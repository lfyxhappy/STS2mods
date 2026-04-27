using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class PrayerWish : WatcherCard, IProphecyCard
{
	private const int _kfCost = 5;

	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[4]
	{
		new CardsVar("MagicNumber", 3),
		new PowerVar<Mantra>(3m),
		new BlockVar(12m, ValueProp.Move),
		new DamageVar(8m, ValueProp.Unpowered)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[3]
	{
		HoverTipFactory.FromPower<KnowFatePower>(),
		HoverTipFactory.FromPower<Mantra>(),
		HoverTipFactory.FromPower<BlessProphecyDamagePower>()
	});

	public PrayerWish()
		: base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
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
		CardModel possessTok = await WatcherCombatHelper.CreateWatcherCard<WishChoicePossess>(base.Owner);
		CardModel protectTok = await WatcherCombatHelper.CreateWatcherCard<WishChoiceProtect>(base.Owner);
		CardModel cardModel = await WatcherCombatHelper.CreateWatcherCard<WishChoiceStrike>(base.Owner);
		if (base.IsUpgraded)
		{
			possessTok.UpgradeInternal();
			protectTok.UpgradeInternal();
			cardModel.UpgradeInternal();
		}
		List<CardModel> options = new List<CardModel> { possessTok, protectTok, cardModel };
		CardModel cardModel2 = await WatcherCombatHelper.ChooseOne(base.Owner, options, new LocString("card_selection", "TO_PLAY"));
		if (!(cardModel2 is WishChoicePossess))
		{
			if (!(cardModel2 is WishChoiceProtect))
			{
				if (cardModel2 is WishChoiceStrike)
				{
					await PowerCmd.Apply<BlessProphecyDamagePower>(base.Owner.Creature, base.DynamicVars.Damage.BaseValue, base.Owner.Creature, this);
				}
			}
			else
			{
				await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
			}
		}
		else
		{
			await WatcherCombatHelper.GainMantra(base.Owner, base.DynamicVars[typeof(Mantra).Name].IntValue, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars[typeof(Mantra).Name].UpgradeValueBy(2m);
		base.DynamicVars.Block.UpgradeValueBy(6m);
		base.DynamicVars.Damage.UpgradeValueBy(4m);
	}
}
