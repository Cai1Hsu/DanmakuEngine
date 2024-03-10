namespace DanmakuEngine.Extensions;

public static class LinqExtensions
{
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
            action.Invoke(item);
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
    {
        int i = 0;

        foreach (var item in enumerable)
            action.Invoke(item, i++);
    }
}
