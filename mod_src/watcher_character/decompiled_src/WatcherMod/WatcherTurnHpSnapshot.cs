using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace WatcherMod;

internal static class WatcherTurnHpSnapshot
{
	private static readonly ConditionalWeakTable<Creature, object> _snapshots = new ConditionalWeakTable<Creature, object>();

	public static void Record(Creature creature)
	{
		try
		{
			_snapshots.Remove(creature);
			_snapshots.Add(creature, creature.CurrentHp);
		}
		catch
		{
		}
	}

	public static int? Get(Creature creature)
	{
		if (!_snapshots.TryGetValue(creature, out object value) || !(value is int))
		{
			return null;
		}
		return (int)value;
	}
}
