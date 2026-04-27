using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace WatcherMod;

internal static class WatcherCardLibraryInjector
{
	private static void FixOwnerRecursive(Node root, Node owner)
	{
		foreach (Node child in root.GetChildren())
		{
			child.Owner = owner;
			FixOwnerRecursive(child, owner);
		}
	}

	public static void Inject(NCardLibrary instance)
	{
		if (instance.FindChild("WatcherPool", recursive: true, owned: false) != null)
		{
			return;
		}
		CharacterModel byIdOrNull = ModelDb.GetByIdOrNull<CharacterModel>(ModelDb.GetId(typeof(Watcher)));
		CharacterModel characterModel = (WatcherModSettings.EnableV2Watcher ? ModelDb.GetByIdOrNull<CharacterModel>(ModelDb.GetId(typeof(WatcherV2))) : null);
		if (byIdOrNull == null || !(AccessTools.Field(typeof(NCardLibrary), "_necrobinderFilter")?.GetValue(instance) is NCardPoolFilter template))
		{
			return;
		}
		Dictionary<NCardPoolFilter, Func<CardModel, bool>> dictionary = AccessTools.Field(typeof(NCardLibrary), "_poolFilters")?.GetValue(instance) as Dictionary<NCardPoolFilter, Func<CardModel, bool>>;
		Dictionary<CharacterModel, NCardPoolFilter> dictionary2 = AccessTools.Field(typeof(NCardLibrary), "_cardPoolFilters")?.GetValue(instance) as Dictionary<CharacterModel, NCardPoolFilter>;
		if (dictionary == null || dictionary2 == null)
		{
			return;
		}
		MethodInfo updateMethod = AccessTools.Method(typeof(NCardLibrary), "UpdateCardPoolFilter");
		FieldInfo lastHoveredField = AccessTools.Field(typeof(NCardLibrary), "_lastHoveredControl");
		Texture2D icon = WatcherTextureHelper.LoadTexture("res://images/ui/top_panel/character_icon_watcher.png");
		WatcherCardPool watcherCardPool = ModelDb.CardPool<WatcherCardPool>();
		WatcherV2CardPool watcherV2CardPool = ((characterModel != null) ? ModelDb.CardPool<WatcherV2CardPool>() : null);
		HashSet<ModelId> gen1Ids = watcherCardPool.AllCardIds.ToHashSet();
		HashSet<ModelId> gen2Ids = watcherV2CardPool?.AllCardIds.ToHashSet();
		NCardPoolFilter nCardPoolFilter = CreatePoolFilter(template, "WatcherPool", icon, shimmer: false);
		RegisterPoolFilter(instance, nCardPoolFilter, updateMethod, lastHoveredField, dictionary, (CardModel c) => gen1Ids.Contains(c.Id));
		dictionary2[byIdOrNull] = nCardPoolFilter;
		if (characterModel != null && gen2Ids != null)
		{
			NCardPoolFilter nCardPoolFilter2 = CreatePoolFilter(nCardPoolFilter, "WatcherV2Pool", icon, shimmer: true);
			RegisterPoolFilter(instance, nCardPoolFilter2, updateMethod, lastHoveredField, dictionary, (CardModel c) => gen2Ids.Contains(c.Id));
			dictionary2[characterModel] = nCardPoolFilter2;
		}
	}

	private static NCardPoolFilter CreatePoolFilter(NCardPoolFilter template, string name, Texture2D? icon, bool shimmer)
	{
		NCardPoolFilter nCardPoolFilter = (NCardPoolFilter)template.Duplicate(6);
		nCardPoolFilter.Name = name;
		FixOwnerRecursive(nCardPoolFilter, nCardPoolFilter);
		Node parent = template.GetParent();
		parent.AddChild(nCardPoolFilter, forceReadableName: false, Node.InternalMode.Disabled);
		parent.MoveChild(nCardPoolFilter, template.GetIndex() + 1);
		if (nCardPoolFilter.GetNodeOrNull<Control>("Image") is TextureRect textureRect)
		{
			if (icon != null)
			{
				textureRect.Texture = icon;
			}
			if (shimmer)
			{
				WatcherShimmerOverlay.AttachTo(textureRect);
			}
		}
		nCardPoolFilter.Visible = true;
		return nCardPoolFilter;
	}

	private static void RegisterPoolFilter(NCardLibrary instance, NCardPoolFilter filter, MethodInfo? updateMethod, FieldInfo? lastHoveredField, Dictionary<NCardPoolFilter, Func<CardModel, bool>> poolFilters, Func<CardModel, bool> predicate)
	{
		poolFilters[filter] = predicate;
		if (updateMethod != null)
		{
			filter.Connect("Toggled", Callable.From(delegate(NCardPoolFilter f)
			{
				updateMethod.Invoke(instance, new object[1] { f });
			}));
		}
		if (lastHoveredField != null)
		{
			filter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
			{
				lastHoveredField.SetValue(instance, filter);
			}));
		}
	}

	public static void InstallSceneTreeListener()
	{
		if (Engine.GetMainLoop() is SceneTree sceneTree)
		{
			sceneTree.NodeAdded += OnNodeAdded;
		}
	}

	private static void OnNodeAdded(Node node)
	{
		NCardLibrary cardLibrary = node as NCardLibrary;
		if (cardLibrary == null)
		{
			return;
		}
		cardLibrary.CallDeferred("set", "_deferred_watcher_inject", true);
		cardLibrary.GetTree().CreateTimer(0.1).Timeout += delegate
		{
			if (GodotObject.IsInstanceValid(cardLibrary))
			{
				Inject(cardLibrary);
			}
		};
	}
}
