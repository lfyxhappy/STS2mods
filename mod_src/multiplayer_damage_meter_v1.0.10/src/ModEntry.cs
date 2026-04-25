using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Runs;

namespace MultiplayerDamageMeter;

[ModInitializer("Init")]
public static class ModEntry
{
	private const string ModId = "multiplayer_damage_meter";

	private static bool _initialized;

	private static Harmony? _harmony;

	public static void Init()
	{
		if (_initialized)
		{
			return;
		}

		_initialized = true;
		Log.Info($"Initializing {ModId} {ModVersionInfo.DisplayVersion}.");
		DamageStatsService.Initialize();
		_harmony = new Harmony("codex.multiplayer_damage_meter");
		_harmony.PatchAll(typeof(ModEntry).Assembly);
		ModHelper.SubscribeForCombatStateHooks(ModId, static _ => new[] { DamageStatsCombatHookModel.GetCanonical() });
		RunManager.Instance.RunStarted += DamageStatsService.OnRunStarted;
		CombatManager.Instance.CombatSetUp += DamageStatsService.OnCombatSetUp;
		CombatManager.Instance.CombatEnded += DamageStatsService.OnCombatEnded;
	}
}
