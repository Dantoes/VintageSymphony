using VintageSymphony.Situations.Evaluator;

namespace VintageSymphony.Situations;

public enum Situation
{
	[SituationData(10f)]
	TemporalStorm,

	[SituationData(2.0f, 
		pauseAfterPlayback: true,
		forcedPauseAfterPlayback: true,
		breaksPause: true, 
		breaksForcedPause: true,
		breaksJustStartedTracks: true, 
		smoothIncreasingCertainty: false)]
	Fight,

	[SituationData(1.5f, 
		pauseAfterPlayback: true,
		forcedPauseAfterPlayback: true,
		breaksPause: true,
		breaksJustStartedTracks: true,
		smoothIncreasingCertainty: false,
		aversions: new []{Cave})]
	Danger,

	[SituationData(1.6f, 
		pauseAfterPlayback: true,
		breaksPause: true, 
		smoothDecreasingCertainty: false)]
	Cave,

	[SituationData(weight: 1.2f, aversions: new []{Cave})]
	Adventure,
	
	[SituationData(weight: 0.9f, aversions: new []{Cave})]
	Idle,
	
	[SituationData(aversions: new []{Cave})]
	Calm,
}