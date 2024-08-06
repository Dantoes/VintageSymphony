using VintageSymphony.Situations.Facts;
using VintageSymphony.Util;

namespace VintageSymphony.Situations.Evaluator;

public class FightEvaluator : IEvaluator
{
	public bool IsEvaluatingSituation(Situation situation)
	{
		return situation == Situation.Fight;
	}

	public float Evaluate(Situation situation, SituationalFacts facts)
	{
		float enemies = MoreMath.ClampMap(facts.EnemyDistance, 5, 15, 1, 0);
		float holdingWeapon = facts.IsHoldingWeapon ? 1f : 0f;
		float damage = facts.SecondsSinceLastDamage == 0f
			? 0f
			: MoreMath.ClampMap(facts.SecondsSinceLastDamage, 0, 20, 1, 0);

		float basicWeight = MoreMath.WeightedAverage(
			new Tuple<float, float>(enemies, 1.5f),
			new Tuple<float, float>(holdingWeapon, 0.5f)
		);

		return basicWeight * .75f + damage * enemies;
	}
}