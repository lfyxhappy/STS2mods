using System.Collections.Generic;
using System.Reflection;
using Godot;
using Godot.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.addons.mega_text;

namespace WatcherMod;

internal static class WatcherInspectCardArtToggleInjector
{
	private const string ToggleTickboxName = "WatcherArtTickbox";

	private const string SetupMarker = "_watcher_art_toggle_setup";

	private static readonly FieldInfo? CardsField = AccessTools.Field(typeof(NInspectCardScreen), "_cards");

	private static readonly FieldInfo? IndexField = AccessTools.Field(typeof(NInspectCardScreen), "_index");

	private static void FixOwnerRecursive(Node root, Node owner)
	{
		foreach (Node child in root.GetChildren())
		{
			child.Owner = owner;
			FixOwnerRecursive(child, owner);
		}
	}

	private static Node? FindLabel(NTickbox tickbox)
	{
		Node node = tickbox.FindChild("ShowUpgradeLabel", recursive: true, owned: false);
		if (node != null)
		{
			return node;
		}
		foreach (Node child in tickbox.GetChildren())
		{
			if (child is MegaLabel)
			{
				return child;
			}
		}
		return null;
	}

	private static NTickbox? EnsureToggle(NInspectCardScreen screen)
	{
		if (screen.FindChild("WatcherArtTickbox", recursive: true, owned: false) is NTickbox result)
		{
			return result;
		}
		NTickbox nodeOrNull = screen.GetNodeOrNull<NTickbox>("%Upgrade");
		if (nodeOrNull == null)
		{
			return null;
		}
		NTickbox nTickbox = (NTickbox)nodeOrNull.Duplicate(6);
		nTickbox.Name = "WatcherArtTickbox";
		FixOwnerRecursive(nTickbox, nTickbox);
		float num = 24f;
		Vector2 globalPosition = new Vector2(nodeOrNull.GlobalPosition.X + nodeOrNull.Size.X + num, nodeOrNull.GlobalPosition.Y);
		screen.AddChild(nTickbox, forceReadableName: false, Node.InternalMode.Disabled);
		nTickbox.GlobalPosition = globalPosition;
		Node node = FindLabel(nTickbox);
		if (node is MegaLabel megaLabel)
		{
			megaLabel.SetTextAutoSize("手绘画风");
		}
		else if (node is Label label)
		{
			label.Text = "手绘画风";
		}
		return nTickbox;
	}

	public static void UpdateToggle(NInspectCardScreen screen)
	{
		List<CardModel> list = CardsField?.GetValue(screen) as List<CardModel>;
		int num = (int)(IndexField?.GetValue(screen) ?? ((object)(-1)));
		Log.Info($"[Watcher] ArtToggle: UpdateToggle cards={list?.Count} index={num}");
		if (list == null || num < 0 || num >= list.Count)
		{
			return;
		}
		CardModel card = list[num];
		bool flag = card.Pool is WatcherCardPool;
		NTickbox nTickbox = EnsureToggle(screen);
		if (nTickbox == null)
		{
			return;
		}
		if (!flag)
		{
			nTickbox.Visible = false;
			return;
		}
		nTickbox.Visible = true;
		nTickbox.IsTicked = WatcherCardArtSettings.IsHandDrawn(card);
		foreach (Dictionary signalConnection in nTickbox.GetSignalConnectionList(NTickbox.SignalName.Toggled))
		{
			try
			{
				Callable callable = (Callable)signalConnection["callable"];
				if (nTickbox.IsConnected(NTickbox.SignalName.Toggled, callable))
				{
					nTickbox.Disconnect(NTickbox.SignalName.Toggled, callable);
				}
			}
			catch
			{
			}
		}
		nTickbox.Connect(NTickbox.SignalName.Toggled, Callable.From<NTickbox>(delegate
		{
			WatcherCardArtSettings.ToggleCard(card);
			AccessTools.Method(typeof(NInspectCardScreen), "UpdateCardDisplay")?.Invoke(screen, null);
			RefreshMatchingCards(screen.GetTree().Root, card);
		}));
	}

	private static void RefreshMatchingCards(Node root, CardModel targetCard)
	{
		foreach (Node child in root.GetChildren())
		{
			if (child is NCard nCard && nCard.Model?.Id == targetCard.Id)
			{
				nCard.Model = nCard.Model;
			}
			RefreshMatchingCards(child, targetCard);
		}
	}

	public static void InstallSceneTreeListener()
	{
		SceneTree sceneTree = Engine.GetMainLoop() as SceneTree;
		Log.Info($"[Watcher] ArtToggle: InstallSceneTreeListener tree={sceneTree}");
		if (sceneTree != null)
		{
			sceneTree.NodeAdded += OnNodeAdded;
			Log.Info("[Watcher] ArtToggle: NodeAdded listener installed");
		}
	}

	private static void OnNodeAdded(Node node)
	{
		NInspectCardScreen screen = node as NInspectCardScreen;
		if (screen == null)
		{
			return;
		}
		Log.Info("[Watcher] ArtToggle: NInspectCardScreen detected via NodeAdded");
		screen.GetTree().CreateTimer(0.1).Timeout += delegate
		{
			if (GodotObject.IsInstanceValid(screen))
			{
				SetupSignalHooks(screen);
			}
		};
	}

	private static void SetupSignalHooks(NInspectCardScreen screen)
	{
		Log.Info("[Watcher] ArtToggle: SetupSignalHooks called");
		if (screen.HasMeta("_watcher_art_toggle_setup"))
		{
			return;
		}
		screen.SetMeta("_watcher_art_toggle_setup", true);
		Log.Info("[Watcher] ArtToggle: Signal hooks set up OK");
		screen.Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(delegate
		{
			if (screen.Visible)
			{
				Callable.From(delegate
				{
					UpdateToggle(screen);
				}).CallDeferred();
			}
		}));
		Control control = AccessTools.Field(typeof(NInspectCardScreen), "_leftButton")?.GetValue(screen) as Control;
		Control control2 = AccessTools.Field(typeof(NInspectCardScreen), "_rightButton")?.GetValue(screen) as Control;
		control?.Connect("Released", Callable.From<Control>(delegate
		{
			Callable.From(delegate
			{
				UpdateToggle(screen);
			}).CallDeferred();
		}));
		control2?.Connect("Released", Callable.From<Control>(delegate
		{
			Callable.From(delegate
			{
				UpdateToggle(screen);
			}).CallDeferred();
		}));
		if (screen.Visible)
		{
			Callable.From(delegate
			{
				UpdateToggle(screen);
			}).CallDeferred();
		}
	}
}
