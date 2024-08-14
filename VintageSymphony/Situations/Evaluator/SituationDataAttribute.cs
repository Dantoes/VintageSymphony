namespace VintageSymphony.Situations.Evaluator;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class SituationDataAttribute : Attribute
{
	public float Priority { get; }
	public float Weight { get; }
	public bool BreaksPause { get; }
	public bool BreaksJustStartedTracks { get; }
	public bool SmoothIncreasingCertainty { get; }
	public bool SmoothDecreasingCertainty { get; }

	public SituationDataAttribute(float priority = 1f,
		float weight = 1f,
		bool breaksPause = false,
		bool breaksJustStartedTracks = false,
		bool smoothIncreasingCertainty = true,
		bool smoothDecreasingCertainty = true)
	{
		Weight = weight;
		Priority = priority;
		BreaksPause = breaksPause;
		BreaksJustStartedTracks = breaksJustStartedTracks;
		SmoothIncreasingCertainty = smoothIncreasingCertainty;
		SmoothDecreasingCertainty = smoothDecreasingCertainty;
	}
}