using System;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace PauseMenuRerun;

internal static class RerunCoordinator
{
	private static readonly MethodInfo CloseToMenuMethod =
		AccessTools.Method(typeof(NPauseMenu), "CloseToMenu")
		?? throw new MissingMethodException(typeof(NPauseMenu).FullName, "CloseToMenu");

	private static readonly MethodInfo ContinueButtonMethod =
		AccessTools.Method(typeof(NMainMenu), "OnContinueButtonPressed")
		?? throw new MissingMethodException(typeof(NMainMenu).FullName, "OnContinueButtonPressed");

	private static readonly StringName ShaderS = new StringName("s");
	private static readonly StringName ShaderV = new StringName("v");
	private static readonly Color RerunGreen = new Color("6FEA50");
	private static readonly Color RerunGreenOutline = new Color("1E5F27");

	public static bool Pending { get; private set; }

	public static void AddRerunButton(NPauseMenu pauseMenu)
	{
		Control buttonContainer = pauseMenu.GetNode<Control>("%ButtonContainer");
		if (buttonContainer.GetNodeOrNull<NPauseMenuButton>("Rerun") != null)
		{
			RefreshFocusNeighbors(buttonContainer);
			return;
		}

		NPauseMenuButton? saveAndQuitButton = buttonContainer.GetNodeOrNull<NPauseMenuButton>("SaveAndQuit");
		if (saveAndQuitButton == null)
		{
			return;
		}

		NPauseMenuButton rerunButton = CreatePauseMenuButton();
		rerunButton.Name = "Rerun";
		rerunButton.LayoutMode = saveAndQuitButton.LayoutMode;
		rerunButton.SizeFlagsHorizontal = saveAndQuitButton.SizeFlagsHorizontal;
		rerunButton.Visible = saveAndQuitButton.Visible;
		rerunButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(button => StartRerun(pauseMenu, button)));

		buttonContainer.AddChild(rerunButton);
		buttonContainer.MoveChild(rerunButton, saveAndQuitButton.GetIndex());
		StyleRerunButton(rerunButton);
		RefreshFocusNeighbors(buttonContainer);
	}

	public static void TryContinueFromMainMenu(NMainMenu mainMenu)
	{
		if (!Pending)
		{
			return;
		}

		Pending = false;
		if (!SaveManager.Instance.HasRunSave)
		{
			return;
		}

		Callable.From(() => ContinueButtonMethod.Invoke(mainMenu, new object?[] { null })).CallDeferred();
	}

	private static NPauseMenuButton CreatePauseMenuButton()
	{
		const string scenePath = "res://scenes/pause_menu/pause_menu_button.tscn";
		PackedScene scene = ResourceLoader.Load<PackedScene>(scenePath);
		return scene.Instantiate<NPauseMenuButton>(PackedScene.GenEditState.Disabled);
	}

	private static void StartRerun(NPauseMenu pauseMenu, NButton button)
	{
		button.Disable();
		Pending = true;
		if (CloseToMenuMethod.Invoke(pauseMenu, Array.Empty<object>()) is Task closeToMenuTask)
		{
			TaskHelper.RunSafely(closeToMenuTask);
		}
	}

	private static void StyleRerunButton(NPauseMenuButton button)
	{
		MegaLabel? label = button.GetNodeOrNull<MegaLabel>("Label");
		if (label != null)
		{
			label.Text = "重打";
			label.SetTextAutoSize("重打");
			label.AddThemeColorOverride("font_outline_color", RerunGreenOutline);
		}

		TextureRect? image = button.GetNodeOrNull<TextureRect>("ButtonImage");
		if (image == null)
		{
			return;
		}

		image.Modulate = RerunGreen;
		if (image.Material is ShaderMaterial material)
		{
			ShaderMaterial copy = (ShaderMaterial)material.Duplicate();
			copy.ResourceLocalToScene = true;
			copy.SetShaderParameter(ShaderS, 1.25f);
			copy.SetShaderParameter(ShaderV, 1.15f);
			image.Material = copy;
		}
	}

	private static void RefreshFocusNeighbors(Control buttonContainer)
	{
		NPauseMenuButton[] buttons = buttonContainer.GetChildren()
			.OfType<NPauseMenuButton>()
			.Where(button => button.Visible)
			.ToArray();

		for (int i = 0; i < buttons.Length; i++)
		{
			NPauseMenuButton previous = i > 0 ? buttons[i - 1] : buttons[i];
			NPauseMenuButton next = i < buttons.Length - 1 ? buttons[i + 1] : buttons[i];
			buttons[i].FocusNeighborLeft = buttons[i].GetPath();
			buttons[i].FocusNeighborRight = buttons[i].GetPath();
			buttons[i].FocusNeighborTop = previous.GetPath();
			buttons[i].FocusNeighborBottom = next.GetPath();
		}
	}
}
