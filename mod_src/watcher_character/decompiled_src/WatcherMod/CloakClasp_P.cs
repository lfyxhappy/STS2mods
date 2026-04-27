using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class CloakClasp_P : WatcherRelic
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public CloakClasp_P()
		: base("clasp")
	{
	}

	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Creature.Side)
		{
			int count = PileType.Hand.GetPile(base.Owner).Cards.Count;
			if (count > 0)
			{
				Flash();
				await CreatureCmd.GainBlock(base.Owner.Creature, count, ValueProp.Unpowered, null);
			}
		}
	}
}
