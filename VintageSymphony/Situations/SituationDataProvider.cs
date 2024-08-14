using System.Reflection;
using System.Runtime.CompilerServices;
using VintageSymphony.Situations.Evaluator;

namespace VintageSymphony.Situations;

public static class SituationDataProvider
{
    private static readonly Dictionary<Situation, SituationDataAttribute> SituationData;

    static SituationDataProvider()
    {
        SituationData = Enum.GetValues(typeof(Situation))
            .Cast<Situation>()
            .ToDictionary(situation => situation, LoadSituationData);
    }

    private static SituationDataAttribute LoadSituationData(Situation situation)
    {
        FieldInfo? fieldInfo = situation.GetType().GetField(situation.ToString());
        return fieldInfo?.GetCustomAttribute<SituationDataAttribute>() ?? new SituationDataAttribute();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SituationDataAttribute GetAttributes(Situation situation)
    {
        return SituationData[situation];
    }
}