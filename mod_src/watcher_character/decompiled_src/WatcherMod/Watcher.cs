using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;

namespace WatcherMod;

public class Watcher : CharacterModel
{
	public override CharacterGender Gender => CharacterGender.Feminine;

	protected override CharacterModel? UnlocksAfterRunAs => ModelDb.Character<Defect>();

	public override Color NameColor => new Color("9E68FF");

	public override int StartingHp => 72;

	public override int StartingGold => 99;

	public override float AttackAnimDelay => 0.15f;

	public override float CastAnimDelay => 0.25f;

	protected override string MapMarkerPath => "res://images/packed/map/icons/map_marker_necrobinder.png";

	public override CardPoolModel CardPool => ModelDb.CardPool<WatcherCardPool>();

	public override PotionPoolModel PotionPool => ModelDb.PotionPool<WatcherPotionPool>();

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<WatcherRelicPool>();

	public override IEnumerable<CardModel> StartingDeck => new global::_003C_003Ez__ReadOnlyArray<CardModel>(new CardModel[10]
	{
		ModelDb.Card<Strike_P>(),
		ModelDb.Card<Strike_P>(),
		ModelDb.Card<Strike_P>(),
		ModelDb.Card<Strike_P>(),
		ModelDb.Card<Defend_P>(),
		ModelDb.Card<Defend_P>(),
		ModelDb.Card<Defend_P>(),
		ModelDb.Card<Defend_P>(),
		ModelDb.Card<Eruption_P>(),
		ModelDb.Card<Vigilance>()
	});

	public override IReadOnlyList<RelicModel> StartingRelics => new global::_003C_003Ez__ReadOnlySingleElementList<RelicModel>(ModelDb.Relic<PureWater>());

	public override Color EnergyLabelOutlineColor => new Color("4E2A7AFF");

	public override Color DialogueColor => new Color("3A2254");

	public override Color MapDrawingColor => new Color("9E68FF");

	public override Color RemoteTargetingLineColor => new Color("C099FF");

	public override Color RemoteTargetingLineOutline => new Color("4E2A7AFF");

	public override string CharacterSelectSfx => "";

	public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_necrobinder";

	public override List<string> GetArchitectAttackVfx()
	{
		int num = 3;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = "vfx/vfx_attack_slash";
		num2++;
		span[num2] = "vfx/vfx_bloody_impact";
		num2++;
		span[num2] = "vfx/vfx_attack_blunt";
		return list;
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		WatcherSkeletonHelper.ApplySkeletonVariant(controller);
		if (controller.BoundObject is Node2D node2D)
		{
			node2D.Scale = Vector2.One * 1.05f;
		}
		if (controller.BoundObject is Node2D node2D2)
		{
			Node nodeOrNull = node2D2.GetNodeOrNull("EyeAnchor/EyeSprite");
			if (nodeOrNull != null)
			{
				try
				{
					new MegaSprite(nodeOrNull).GetAnimationState()?.SetAnimation("Calm");
					GD.Print("[Watcher] Eye animation initialized");
				}
				catch (Exception ex)
				{
					GD.Print("[Watcher] Eye anim error: " + ex.Message);
				}
			}
			else
			{
				GD.Print("[Watcher] EyeSprite not found at Visuals/EyeAnchor/EyeSprite");
			}
		}
		AnimState animState = new AnimState("Idle", isLooping: true);
		AnimState state = new AnimState("Dead");
		AnimState animState2 = new AnimState("Hit");
		AnimState animState3 = new AnimState("Attack");
		AnimState animState4 = new AnimState("Cast");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Cast", animState4);
		return creatureAnimator;
	}
}
