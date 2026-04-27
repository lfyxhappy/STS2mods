using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace WatcherMod;

public sealed class Lightbulb : WatcherRelic, IWatcherProphecyListener
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public Lightbulb()
		: base("lightbulb")
	{
	}

	public Task OnProphecy(Player owner, ProphecyContext ctx)
	{
		if (!ctx.ChangedIntent || ctx.AffectedEnemy == null)
		{
			return Task.CompletedTask;
		}
		if (!ctx.AffectedEnemy.IsAlive)
		{
			return Task.CompletedTask;
		}
		Flash();
		try
		{
			ctx.AffectedEnemy.StunInternal((IReadOnlyList<Creature> _) => Task.CompletedTask, null);
			NCombatRoom.Instance?.GetCreatureNode(ctx.AffectedEnemy)?.RefreshIntents();
		}
		catch
		{
		}
		return Task.CompletedTask;
	}
}
