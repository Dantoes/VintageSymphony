namespace VintageSymphony.Situations;

public class SituationAssessment
{
    public Situation Situation { get; }
    public float Certainty { get; set; }

    public float Priority => SituationDataProvider.GetPriorityValue(Situation);

    public float WeightedCertainty => Certainty * Priority;

    public SituationAssessment(Situation situation, float certainty)
    {
        Situation = situation;
        Certainty = certainty;
    }
}