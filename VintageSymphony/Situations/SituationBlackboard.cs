using Vintagestory.API.MathTools;
using VintageSymphony.Situations.Evaluator;
using VintageSymphony.Situations.Facts;
using VintageSymphony.Storage;

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
				assessment.Certainty = GameMath.Clamp(evaluator.Evaluate(assessment.Situation, facts), 0f, 1f);
			}
		}
		blackboard.Sort(blackboardComparator);
	}

	public float GetSituationCertainty(Situation situation)
	{
		if (assessmentsBySituation.TryGetValue(situation, out var assessment))
		{
			return assessment.Certainty;
		}

		return 0f;
	}
}