using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class DestinyAwaits : WatcherCard, IProphecyCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new CardsVar("MagicNumber", 3),
		new DamageVar(8m, ValueProp.Move),
		new PowerVar<KnowFatePower>(2m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<KnowFatePower>());

	public DestinyAwaits()
		: base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		PlayerCombatState combat = base.Owner.PlayerCombatState;
		if (combat == null)
		{
			return;
		}
		int count = combat.DrawPile.Cards.Count;
		if (count > 0)
		{
			await WatcherCombatHelper.Scry(choiceContext, base.Owner, count, this);
		}
		int intValue = base.DynamicVars["MagicNumber"].IntValue;
		int kfGainPerPlay = base.DynamicVars[typeof(KnowFatePower).Name].IntValue;
		List<CardModel> list = combat.DrawPile.Cards.Where((CardModel c) => c.Type == CardType.Attack && c is IProphecyCard).Take(intValue).ToList();
		foreach (CardModel card in list)
		{
			if (base.Owner.Creature.IsDead || card.Owner.Creature.IsDead)
			{
				break;
			}
			CardPile? pile = card.Pile;
			if (pile != null && pile.Type == PileType.Draw)
			{
				await CardPileCmd.Add(card, PileType.Play);
				await CardCmd.AutoPlay(choiceContext, card, null);
				if (base.Owner.Creature.IsDead)
				{
					break;
				}
				await PowerCmd.Apply<KnowFatePower>(base.Owner.Creature, kfGainPerPlay, base.Owner.Creature, this);
				if (base.CombatState != null)
				{
					await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
						.WithHitFx("vfx/vfx_attack_blunt")
						.SpawningHitVfxOnEachCreature()
						.Execute(choiceContext);
				}
			}
		}
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this
		});
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(2m);
	}
}
