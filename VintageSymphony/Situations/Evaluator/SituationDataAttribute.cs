namespace VintageSymphony.Situations.Evaluator;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class SituationDataAttribute : Attribute
{
	public float Priority { get; }
	public float Weight { get; }
	public bool PauseAfterPlayback { get; }
	public bool ForcedPauseAfterPlayback { get; }
	public bool BreaksPause { get; }
	public bool BreaksForcedPause { get; }
	public bool BreaksJustStartedTracks { get; }
	public bool SmoothIncreasingCertainty { get; }
	public bool SmoothDecreasingCertainty { get; }
	public Situation[] Aversions { get; }

	public SituationDataAttribute(
		float priority = 1f,
		float weight = 1f,
		bool pauseAfterPlayback = true, 
		bool forcedPauseAfterPlayback = false, 
		bool breaksPause = false,
		bool breaksJustStartedTracks = false,
		bool breaksForcedPause = false,
		bool smoothIncreasingCertainty = true,
		bool smoothDecreasingCertainty = true,
		Situation[]? aversions = null)
	{

		Weight = weight;
		Priority = priority;
		PauseAfterPlayback = pauseAfterPlayback;
		ForcedPauseAfterPlayback = forcedPauseAfterPlayback;
		BreaksPause = breaksPause;
		BreaksForcedPause = breaksForcedPause;
		BreaksJustStartedTracks = breaksJustStartedTracks;
		SmoothIncreasingCertainty = smoothIncreasingCertainty;
		SmoothDecreasingCertainty = smoothDecreasingCertainty;
		Aversions = aversions ?? Array.Empty<Situation>();
	}
}