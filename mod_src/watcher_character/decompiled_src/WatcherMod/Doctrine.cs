using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace WatcherMod;

public sealed class Doctrine : WatcherRelic, IWatcherProphecyListener
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<WeakPower>());

	public Doctrine()
		: base("doctrine")
	{
	}

	public async Task OnProphecy(Player owner, ProphecyContext ctx)
	{
		if (owner.Creature?.CombatState == null)
		{
			return;
		}
		Flash();
		List<Creature> list = owner.Creature.CombatState.Enemies.Where((Creature e) => e.IsAlive).ToList();
		foreach (Creature item in list)
		{
			await PowerCmd.Apply<WeakPower>(item, 1m, owner.Creature, ctx.Source);
		}
	}
}
