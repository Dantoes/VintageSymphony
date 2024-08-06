using VintageSymphony.Situations.Evaluator;

namespace VintageSymphony.Situations;

public enum Situation
{
	[SituationData(10f)]
	TemporalStorm,

	[SituationData(2.0f, breaksPause: true, breaksJustStartedTracks: true, smoothIncreasingCertainty: false)]
	Fight,

	[SituationData(1.5f, breaksPause: true, breaksJustStartedTracks: true, smoothIncreasingCertainty: false)]
	Danger,

	[SituationData(1.4f, breaksPause: true)]
	Cave,

	[SituationData(weight: 1.2f)]
	Adventure,
	Idle,
	Calm,
}