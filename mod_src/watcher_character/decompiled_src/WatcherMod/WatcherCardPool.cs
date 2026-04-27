using Godot;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class WatcherCardPool : CardPoolModel
{
	public override string Title => "watcher";

	public override string EnergyColorName => "watcher";

	public override string CardFrameMaterialPath => "card_frame_purple";

	public override Color DeckEntryCardColor => new Color("9E68FF");

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[85]
		{
			ModelDb.Card<Strike_P>(),
			ModelDb.Card<Defend_P>(),
			ModelDb.Card<Eruption_P>(),
			ModelDb.Card<Vigilance>(),
			ModelDb.Card<BowlingBash>(),
			ModelDb.Card<Consecrate>(),
			ModelDb.Card<Crescendo>(),
			ModelDb.Card<CrushJoints>(),
			ModelDb.Card<CutThroughFate>(),
			ModelDb.Card<EmptyBody>(),
			ModelDb.Card<EmptyFist>(),
			ModelDb.Card<Evaluate>(),
			ModelDb.Card<FlurryOfBlows>(),
			ModelDb.Card<FlyingSleeves>(),
			ModelDb.Card<FollowUp>(),
			ModelDb.Card<Halt>(),
			ModelDb.Card<JustLucky>(),
			ModelDb.Card<PressurePoints>(),
			ModelDb.Card<Prostrate>(),
			ModelDb.Card<Protect>(),
			ModelDb.Card<SashWhip>(),
			ModelDb.Card<ThirdEye>(),
			ModelDb.Card<Tranquility>(),
			ModelDb.Card<ColdObservation>(),
			ModelDb.Card<Mockery>(),
			ModelDb.Card<Persuasion>(),
			ModelDb.Card<BattleHymn>(),
			ModelDb.Card<CarveReality>(),
			ModelDb.Card<Collect>(),
			ModelDb.Card<Conclude>(),
			ModelDb.Card<DeceiveReality>(),
			ModelDb.Card<EmptyMind>(),
			ModelDb.Card<Fasting2>(),
			ModelDb.Card<FearNoEvil>(),
			ModelDb.Card<ForeignInfluence>(),
			ModelDb.Card<Foresight>(),
			ModelDb.Card<Indignation>(),
			ModelDb.Card<InnerPeace>(),
			ModelDb.Card<LikeWater>(),
			ModelDb.Card<Meditate>(),
			ModelDb.Card<MentalFortress>(),
			ModelDb.Card<Nirvana>(),
			ModelDb.Card<Perseverance>(),
			ModelDb.Card<Pray>(),
			ModelDb.Card<ReachHeaven>(),
			ModelDb.Card<Rushdown>(),
			ModelDb.Card<Sanctity>(),
			ModelDb.Card<SandsOfTime>(),
			ModelDb.Card<SignatureMove>(),
			ModelDb.Card<SimmeringFury>(),
			ModelDb.Card<Study>(),
			ModelDb.Card<Swivel>(),
			ModelDb.Card<TalkToTheHand>(),
			ModelDb.Card<Tantrum>(),
			ModelDb.Card<Wallop>(),
			ModelDb.Card<WaveOfTheHand>(),
			ModelDb.Card<Weave>(),
			ModelDb.Card<WheelKick>(),
			ModelDb.Card<WindmillStrike>(),
			ModelDb.Card<Worship>(),
			ModelDb.Card<WreathOfFlame>(),
			ModelDb.Card<Relinquish>(),
			ModelDb.Card<Sanctification>(),
			ModelDb.Card<Alpha>(),
			ModelDb.Card<Blasphemy>(),
			ModelDb.Card<Brilliance>(),
			ModelDb.Card<ConjureBlade>(),
			ModelDb.Card<DeusExMachina>(),
			ModelDb.Card<DevaForm>(),
			ModelDb.Card<Devotion>(),
			ModelDb.Card<Establishment>(),
			ModelDb.Card<Judgment>(),
			ModelDb.Card<LessonLearned>(),
			ModelDb.Card<MasterReality>(),
			ModelDb.Card<Omniscience>(),
			ModelDb.Card<Ragnarok>(),
			ModelDb.Card<Scrawl_P>(),
			ModelDb.Card<SpiritShield>(),
			ModelDb.Card<Vault>(),
			ModelDb.Card<Wish_P>(),
			ModelDb.Card<Cataclysm>(),
			ModelDb.Card<Serenity>(),
			ModelDb.Card<Preach>(),
			ModelDb.Card<DrawTalisman>(),
			ModelDb.Card<Miracle>()
		};
	}
}
