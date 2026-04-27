using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class Sanctification : WatcherCard
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		WatcherHoverTips.Stance,
		HoverTipFactory.FromPower<Divinity>()
	});

	public Sanctification()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.AllAllies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IEnumerable<Creature> enumerable = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
			where c != null && c.IsAlive && c.IsPlayer && c != base.Owner.Creature
			select c;
		foreach (Creature item in enumerable)
		{
			if (item.Player != null)
			{
				await WatcherCombatHelper.EnterDivinity(item.Player, this);
			}
		}
		await CreatureCmd.Damage(choiceContext, base.Owner.Creature, 99999m, (ValueProp)0, null, null);
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Retain);
	}
}
