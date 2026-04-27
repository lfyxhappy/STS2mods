using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace WatcherMod;

[HarmonyPatch(typeof(NCard), "UpdateEnchantmentVisuals")]
internal static class WatcherNCardExtraEnchantTabsPatch
{
	private static readonly AccessTools.FieldRef<NCard, Control> TabRef = AccessTools.FieldRefAccess<NCard, Control>("_enchantmentTab");

	private static readonly ConditionalWeakTable<NCard, List<Control>> _spawned = new ConditionalWeakTable<NCard, List<Control>>();

	private const float _stackOffset = 50f;

	private static void Postfix(NCard __instance)
	{
		try
		{
			Control control = TabRef(__instance);
			CardModel model = __instance.Model;
			if (control == null || model == null)
			{
				return;
			}
			List<Control> value = _spawned.GetValue(__instance, (NCard _) => new List<Control>());
			foreach (Control item in value)
			{
				if (GodotObject.IsInstanceValid(item))
				{
					item.QueueFree();
				}
			}
			value.Clear();
			List<EnchantmentModel> extras = WatcherEnchantStack.GetExtras(model);
			if (extras == null || extras.Count == 0)
			{
				return;
			}
			Node parent = control.GetParent();
			if (parent == null)
			{
				return;
			}
			Vector2 position = control.Position;
			for (int num = 0; num < extras.Count; num++)
			{
				EnchantmentModel enchantmentModel = extras[num];
				Control control2 = (Control)control.Duplicate(3);
				if (control2 != null)
				{
					control2.Visible = true;
					control2.Position = position + Vector2.Down * (50f * (float)(num + 1));
					TextureRect nodeOrNull = control2.GetNodeOrNull<TextureRect>("Icon");
					if (nodeOrNull != null)
					{
						nodeOrNull.Texture = enchantmentModel.Icon;
					}
					Control nodeOrNull2 = control2.GetNodeOrNull<Control>("Label");
					if (nodeOrNull2 != null)
					{
						nodeOrNull2.Visible = enchantmentModel.ShowAmount;
					}
					parent.AddChild(control2, forceReadableName: false, Node.InternalMode.Disabled);
					value.Add(control2);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] Extra enchant tab render failed: " + ex.Message);
		}
	}
}
