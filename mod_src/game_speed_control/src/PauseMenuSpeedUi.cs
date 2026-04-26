using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using MegaCrit.Sts2.addons.mega_text;

namespace GameSpeedControl;

internal static class PauseMenuSpeedUi
{
	private static readonly StringName ShaderS = new("s");
	private static readonly StringName ShaderV = new("v");
	private static readonly Color SpeedBlue = new("65C7FF");
	private static readonly Color SpeedBlueOutline = new("143A66");

	public static void AddSpeedButton(NPauseMenu pauseMenu)
	{
		Control buttonContainer = pauseMenu.GetNode<Control>("%ButtonContainer");
		if (buttonContainer.GetNodeOrNull<NPauseMenuButton>("Speed") is { } existingButton)
		{
			UpdateLabel(existingButton);
			RefreshFocusNeighbors(buttonContainer);
			return;
		}

		NPauseMenuButton? saveAndQuitButton = buttonContainer.GetNodeOrNull<NPauseMenuButton>("SaveAndQuit");
		if (saveAndQuitButton == null)
		{
			return;
		}

		NPauseMenuButton speedButton = CreatePauseMenuButton();
		speedButton.Name = "Speed";
		speedButton.LayoutMode = saveAndQuitButton.LayoutMode;
		speedButton.SizeFlagsHorizontal = saveAndQuitButton.SizeFlagsHorizontal;
		speedButton.Visible = saveAndQuitButton.Visible;
		speedButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(button => CycleSpeed(button)));

		buttonContainer.AddChild(speedButton);
		buttonContainer.MoveChild(speedButton, saveAndQuitButton.GetIndex());
		StyleButton(speedButton);
		UpdateLabel(speedButton);
		RefreshFocusNeighbors(buttonContainer);
	}

	private static NPauseMenuButton CreatePauseMenuButton()
	{
		const string scenePath = "res://scenes/pause_menu/pause_menu_button.tscn";
		PackedScene scene = ResourceLoader.Load<PackedScene>(scenePath);
		return scene.Instantiate<NPauseMenuButton>(PackedScene.GenEditState.Disabled);
	}

	private static void CycleSpeed(NButton button)
	{
		SpeedController.Cycle();
		if (button is NPauseMenuButton speedButton)
		{
			UpdateLabel(speedButton);
		}
	}

	private static void UpdateLabel(NPauseMenuButton button)
	{
		MegaLabel? label = button.GetNodeOrNull<MegaLabel>("Label");
		if (label == null)
		{
			return;
		}

		string text = SpeedPresetCycle.FormatLabel(SpeedController.CurrentSpeed);
		label.Text = text;
		label.SetTextAutoSize(text);
		label.AddThemeColorOverride("font_outline_color", SpeedBlueOutline);
	}

	private static void StyleButton(NPauseMenuButton button)
	{
		TextureRect? image = button.GetNodeOrNull<TextureRect>("ButtonImage");
		if (image == null)
		{
			return;
		}

		image.Modulate = SpeedBlue;
		if (image.Material is ShaderMaterial material)
		{
			ShaderMaterial copy = (ShaderMaterial)material.Duplicate();
			copy.ResourceLocalToScene = true;
			copy.SetShaderParameter(ShaderS, 1.1f);
			copy.SetShaderParameter(ShaderV, 1.2f);
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
