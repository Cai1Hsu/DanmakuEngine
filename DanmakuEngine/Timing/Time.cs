using System.Diagnostics;
using DanmakuEngine.Allocations;
using DanmakuEngine.Engine;

namespace DanmakuEngine.Timing;

public class Time
{
    public static Stopwatch AppTimer { get; } = Stopwatch.StartNew();
    public static Stopwatch EngineTimer { get; protected set; } = null!;

    public static readonly StandardClock Clock = new();

    /// <summary>
    /// Elapsed seconds in the update loop.
    /// </summary>
    public static double ElapsedSeconds { get; internal set; }

    /// <summary>
    /// Elapsed seconds in FixedUpdate loop.
    /// </summary>
    public static double FixedElapsedSeconds { get; internal set; }

    /// <summary>
    /// Update delta of the last frame in seconds
    /// </summary>
    public static double UpdateDelta { get; internal set; }

    public static float UpdateDeltaF { get; internal set; }

    public static double FixedUpdateHz { get; set; } = 60.0;

    public static double FixedUpdateDelta => 1.0 / FixedUpdateHz;

    public static double FixedUpdateDeltaF => (float)FixedUpdateDelta;

    protected double DebugFpsHz = 1;
}
