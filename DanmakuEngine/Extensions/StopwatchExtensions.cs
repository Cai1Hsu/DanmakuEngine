using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DanmakuEngine.Extensions;

public static class StopwatchExtensions
{
    public static double GetElapsedMilliseconds(this Stopwatch sw)
        => sw.GetElapsedSeconds() * 1000;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GetElapsedSeconds(this Stopwatch sw)
        => (double)sw.ElapsedTicks / Stopwatch.Frequency;
}
