using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace WatcherMod;

public sealed class HolyWater : WatcherRelic
{
	public override RelicRarity Rarity => RelicRarity.Starter;

	public HolyWater()
		: base("holy_water")
	{
	}

	public override async Task BeforeCombatStart()
	{
		if (base.Owner.Creature.CombatState != null)
		{
			Flash();
			for (int i = 0; i < 3; i++)
			{
				await CardPileCmd.AddGeneratedCardToCombat(base.Owner.Creature.CombatState.CreateCard<Miracle>(base.Owner), PileType.Hand, addedByPlayer: true);
			}
		}
	}
}
