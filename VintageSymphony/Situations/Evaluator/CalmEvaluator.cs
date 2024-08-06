using VintageSymphony.Situations.Facts;
using VintageSymphony.Util;

namespace VintageSymphony.Situations.Evaluator;

public class CalmEvaluator : IEvaluator
{
	public bool IsEvaluatingSituation(Situation situation)
	{
		return situation == Situation.Calm;
	}

	public float Evaluate(Situation situation, SituationalFacts facts)
	{
		float movement = MoreMath.ClampMap(facts.MovementRadius, 0, 100, 1, 0);
		float enemies = MoreMath.ClampMap(facts.EnemyDistance, 0, SituationalFacts.EnemyDistanceMax, 0, 1);
		float holdingWeapon = facts.IsHoldingWeapon ? 1f : 0f;
		float damage = MoreMath.ClampMap(facts.SecondsSinceLastDamage, 0, 60, 0, 1);

		return MoreMath.WeightedAverage(
			new Tuple<float, float>(movement, 0.5f),
			new Tuple<float, float>(enemies, 1.2f),
			new Tuple<float, float>(holdingWeapon, 0.7f),
			new Tuple<float, float>(damage, 2f)
		);
	}
}