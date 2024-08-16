using VintageSymphony.Situations.Facts;
using VintageSymphony.Util;

namespace VintageSymphony.Situations.Evaluator;

public class CaveEvaluator : IEvaluator
{
	public bool IsEvaluatingSituation(Situation situation)
	{
		return situation == Situation.Cave;
	}

	public float Evaluate(Situation situation, SituationalFacts facts)
	{
		float underground = MoreMath.ClampMap(facts.DistanceToSurface, 0, 10, 0, 1);
		float sunLevel = MoreMath.ClampMap(facts.SunLevel, 3, 10, 1, 0);
		return underground * sunLevel;
	}
}