using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace WatcherMod;

public sealed class RealityReversal : WatcherCard, IProphecyCard
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	public RealityReversal()
		: base(3, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		Creature target = cardPlay.Target;
		int? num = WatcherTurnHpSnapshot.Get(target);
		if (num.HasValue && target.IsAlive)
		{
			int num2 = num.Value - target.CurrentHp;
			if (num2 > 0)
			{
				await CreatureCmd.Heal(target, num2);
			}
		}
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this
		});
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
