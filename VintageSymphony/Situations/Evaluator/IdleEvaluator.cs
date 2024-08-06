using VintageSymphony.Situations.Facts;
using VintageSymphony.Util;

namespace VintageSymphony.Situations.Evaluator;

public class IdleEvaluator : IEvaluator
{
	public bool IsEvaluatingSituation(Situation situation)
	{
		return situation == Situation.Idle;
	}

	public float Evaluate(Situation situation, SituationalFacts facts)
	{
		float movement = MoreMath.ClampMap(facts.MovementRadius, 0, 30, 1, 0);
		return movement;
	}
}