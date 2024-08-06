using VintageSymphony.Situations.Facts;
using VintageSymphony.Util;

namespace VintageSymphony.Situations.Evaluator;

public class AdventureEvaluator : IEvaluator
{
	public bool IsEvaluatingSituation(Situation situation)
	{
		return situation == Situation.Adventure;
	}

	public float Evaluate(Situation situation, SituationalFacts facts)
	{
		if (float.IsPositiveInfinity(facts.DistanceFromHome))
		{
			return 0f;
		}
		float homeDistance = MoreMath.ClampMap(facts.DistanceFromHome, 400, 800, 0, 1);
		return homeDistance;
	}
}