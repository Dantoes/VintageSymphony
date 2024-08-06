namespace VintageSymphony.Util;

public static class EnumerableExtensions
{
    public static IEnumerable<T> ForeachContinuous<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var element in source)
        {
            action(element);
            yield return element;
        }
    }
}