using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.addons.mega_text;

namespace WatcherMod;

[HarmonyPatch(typeof(NSettingsScreen), "_Ready")]
internal static class WatcherSettingsScreenPatch
{
	private const string SkeletonToggleName = "WatcherSkeletonLine";

	private const string PortraitToggleName = "WatcherPortraitLine";

	private static void FixOwnerRecursive(Node root, Node owner)
	{
		foreach (Node child in root.GetChildren())
		{
			child.Owner = owner;
			FixOwnerRecursive(child, owner);
		}
	}

	private static NTickbox? FindTickbox(Control node)
	{
		if (node is NTickbox result)
		{
			return result;
		}
		foreach (Node child in node.GetChildren())
		{
			if (child is NTickbox result2)
			{
				return result2;
			}
		}
		return null;
	}

	private static int InjectToggle(Control content, Control fastModeLine, Node? fastModeDivider, Node? moddingDivider, string toggleName, string dividerName, string labelText, Action<NTickbox> assignInstance, int insertOffset)
	{
		if (content.GetNodeOrNull(toggleName) != null)
		{
			return insertOffset;
		}
		Control control = (Control)fastModeLine.Duplicate(6);
		control.Name = toggleName;
		FixOwnerRecursive(control, control);
		NTickbox nTickbox = FindTickbox(control);
		if (nTickbox != null)
		{
			assignInstance(nTickbox);
		}
		if (moddingDivider != null)
		{
			int num = moddingDivider.GetIndex() + insertOffset;
			content.AddChild(control, forceReadableName: false, Node.InternalMode.Disabled);
			content.MoveChild(control, num);
			if (fastModeDivider != null)
			{
				Node node = fastModeDivider.Duplicate();
				node.Name = dividerName;
				content.AddChild(node, forceReadableName: false, Node.InternalMode.Disabled);
				content.MoveChild(node, num + 1);
			}
		}
		else
		{
			content.AddChild(control, forceReadableName: false, Node.InternalMode.Disabled);
		}
		Node nodeOrNull = control.GetNodeOrNull("Label");
		if (nodeOrNull is MegaRichTextLabel megaRichTextLabel)
		{
			megaRichTextLabel.Text = labelText;
		}
		else if (nodeOrNull is RichTextLabel richTextLabel)
		{
			richTextLabel.Text = labelText;
		}
		return insertOffset + ((fastModeDivider == null) ? 1 : 2);
	}

	private static void Postfix(NSettingsScreen __instance)
	{
		try
		{
			VBoxContainer content = __instance.GetNode<NSettingsPanel>("%GeneralSettings").Content;
			if (!(content.GetNodeOrNull("FastMode") is Control fastModeLine))
			{
				return;
			}
			Node nodeOrNull = content.GetNodeOrNull("FastModeDivider");
			Node nodeOrNull2 = content.GetNodeOrNull("ModdingDivider");
			int insertOffset = 0;
			if (WatcherModSettings.V2SkeletonAvailable)
			{
				insertOffset = InjectToggle(content, fastModeLine, nodeOrNull, nodeOrNull2, "WatcherSkeletonLine", "WatcherSkeletonDivider", "Watcher: 社区骨骼 / Community Skeleton", delegate(NTickbox tb)
				{
					WatcherSettingsToggle.Instance = tb;
				}, insertOffset);
			}
			insertOffset = InjectToggle(content, fastModeLine, nodeOrNull, nodeOrNull2, "WatcherV2Line", "WatcherV2Divider", "Watcher: 启用二代观者 / Enable Gen2 Watcher", delegate(NTickbox tb)
			{
				WatcherV2ToggleHolder.Instance = tb;
			}, insertOffset);
		}
		catch (Exception ex)
		{
			GD.PrintErr("[Watcher] Settings screen injection error: " + ex.Message);
		}
	}
}
