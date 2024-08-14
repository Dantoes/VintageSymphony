using Vintagestory.API.MathTools;
using VintageSymphony.Situations.Evaluator;
using VintageSymphony.Situations.Facts;
using VintageSymphony.Storage;
using VintageSymphony.Util;

namespace VintageSymphony.Situations;

public class SituationBlackboard
{
	private readonly List<SituationAssessment> blackboard = new();
	private readonly Dictionary<Situation, SituationAssessment> assessmentsBySituation = new();

	public IList<SituationAssessment> Blackboard => blackboard.AsReadOnly();
	private readonly Dictionary<Situation, IEvaluator> evaluators = new();
	private readonly Comparison<SituationAssessment> blackboardComparator =  (x, y) => y.WeightedCertainty.CompareTo(x.WeightedCertainty);

	private readonly List<IEvaluator> allEvaluators = new(new IEvaluator[]
	{
		new CalmEvaluator(),
		new DangerEvaluator(),
		new FightEvaluator(),
		new IdleEvaluator(),
		new AdventureEvaluator(),
		new CaveEvaluator(),
		new TemporalStormEvaluator()
	});

	private readonly SituationalFactsCollector situationalFactsCollector;
	private SituationalFacts facts;
	public SituationalFacts SituationalFacts => facts;

	public SituationBlackboard(AttributeStorage attributeStorage)
	{
		situationalFactsCollector = new(attributeStorage);

		foreach (var situation in Enum.GetValues<Situation>())
		{
			var situationAssessment = new SituationAssessment(situation, 0f);
			blackboard.Add(situationAssessment);
			assessmentsBySituation[situation] = situationAssessment;

			foreach (var evaluator in allEvaluators)
			{
				if (evaluator.IsEvaluatingSituation(situation))
				{
					evaluators[situation] = evaluator;
					break;
				}
			}
		}
	}

	public void Update(float dt)
	{
		facts = situationalFactsCollector.AssessSituation(dt);

		foreach (var assessment in blackboard)
		{
			if (evaluators.TryGetValue(assessment.Situation, out var evaluator))
			{
				var newCertainty = GameMath.Clamp(evaluator.Evaluate(assessment.Situation, facts), 0f, 1f);
				assessment.Certainty = ExponentialSmoothing(assessment, dt, assessment.Certainty, newCertainty);
			}
		}
		blackboard.Sort(blackboardComparator);
	}

	private static float ExponentialSmoothing(SituationAssessment assessment, float dt, float oldCertainty, float newCertainty)
	{
		if ((newCertainty > oldCertainty && assessment.SmoothIncreasingCertainty) 
		    || (newCertainty < oldCertainty && assessment.SmoothDecreasingCertainty))
		{
			return MoreMath.ExponentialSmoothing(oldCertainty, newCertainty, 0.1f, dt);
		}

		return newCertainty;
	}
}