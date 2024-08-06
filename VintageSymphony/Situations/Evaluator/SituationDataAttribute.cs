namespace VintageSymphony.Situations.Evaluator;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
internal sealed class SituationDataAttribute : Attribute
{
	public float Priority { get; }
	public float Weight { get; }
	public bool BreaksPause { get; }
	public bool BreaksJustStartedTracks { get; }

	public SituationDataAttribute(
		float priority = 1f,
		float weight = 1f,
		bool breaksPause = false,
		bool breaksJustStartedTracks = false)
	{
		Weight = weight;
		Priority = priority;
		BreaksPause = breaksPause;
		BreaksJustStartedTracks = breaksJustStartedTracks;
	}
}