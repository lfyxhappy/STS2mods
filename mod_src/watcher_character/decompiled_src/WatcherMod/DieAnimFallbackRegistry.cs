using System.Collections.Generic;

namespace WatcherMod;

internal static class DieAnimFallbackRegistry
{
	private static readonly HashSet<ulong> _needsFallback = new HashSet<ulong>();

	public static void Register(ulong instanceId)
	{
		_needsFallback.Add(instanceId);
	}

	public static bool NeedsFallback(ulong instanceId)
	{
		return _needsFallback.Contains(instanceId);
	}
}
