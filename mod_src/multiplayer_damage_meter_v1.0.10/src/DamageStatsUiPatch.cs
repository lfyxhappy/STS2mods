using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Runs;

namespace MultiplayerDamageMeter;

[HarmonyPatch]
public static class DamageStatsUiPatch
{
	private const string HudWindowName = "DamageStatsHudWindow";

	private static DamageStatsHudWindow? _window;

	private static bool _subscribed;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(NMultiplayerPlayerStateContainer), nameof(NMultiplayerPlayerStateContainer.Initialize))]
	public static void AfterInitialize(NMultiplayerPlayerStateContainer __instance, RunState runState)
	{
		if (!GodotObject.IsInstanceValid(__instance) || runState.Players.Count <= 1)
		{
			return;
		}

		Control? parent = __instance.GetParent<Control>();
		if (parent == null)
		{
			return;
		}

		AttachWindow(parent, runState, __instance);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(NGlobalUi), nameof(NGlobalUi.Initialize))]
	public static void AfterGlobalUiInitialize(NGlobalUi __instance, RunState runState)
	{
		if (!GodotObject.IsInstanceValid(__instance) || runState.Players.Count > 1)
		{
			return;
		}

		AttachWindow(__instance, runState, __instance.RelicInventory);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(NMultiplayerPlayerStateContainer), nameof(NMultiplayerPlayerStateContainer._ExitTree))]
	public static void AfterExitTree(NMultiplayerPlayerStateContainer __instance)
	{
		Control? parent = __instance.GetParent<Control>();
		if (_window != null && GodotObject.IsInstanceValid(_window) && _window.GetParent() == parent)
		{
			_window.QueueFree();
			_window = null;
		}

		if (_window == null || !GodotObject.IsInstanceValid(_window))
		{
			ReleaseSubscriptions();
		}
	}

	private static void AttachWindow(Control parent, RunState runState, Control? anchorControl)
	{
		DamageStatsHudWindow? existing = parent.GetNodeOrNull<DamageStatsHudWindow>(HudWindowName);
		if (existing != null && GodotObject.IsInstanceValid(existing))
		{
			_window = existing;
			_window.Configure(runState, anchorControl);
			_window.RefreshAll();
			EnsureSubscriptions();
			return;
		}

		if (_window != null && GodotObject.IsInstanceValid(_window) && _window.GetParent() != parent)
		{
			_window.QueueFree();
		}

		_window = new DamageStatsHudWindow
		{
			Name = HudWindowName
		};
		parent.AddChild(_window);
		_window.Configure(runState, anchorControl);
		_window.QueueInitialPlacement();
		_window.RefreshAll();
		EnsureSubscriptions();
	}

	private static void EnsureSubscriptions()
	{
		if (_subscribed)
		{
			return;
		}

		_subscribed = true;
		DamageStatsService.PlayerStatsChanged += OnPlayerStatsChanged;
		DamageStatsService.RefreshRequested += OnRefreshRequested;
	}

	private static void ReleaseSubscriptions()
	{
		if (!_subscribed)
		{
			return;
		}

		_subscribed = false;
		DamageStatsService.PlayerStatsChanged -= OnPlayerStatsChanged;
		DamageStatsService.RefreshRequested -= OnRefreshRequested;
	}

	private static void OnPlayerStatsChanged(ulong playerId)
	{
		if (_window == null || !GodotObject.IsInstanceValid(_window))
		{
			_window = null;
			ReleaseSubscriptions();
			return;
		}

		_window.RefreshPlayer(playerId);
	}

	private static void OnRefreshRequested()
	{
		if (_window == null || !GodotObject.IsInstanceValid(_window))
		{
			_window = null;
			ReleaseSubscriptions();
			return;
		}

		_window.RefreshAll();
	}
}
