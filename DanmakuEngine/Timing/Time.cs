using System.Diagnostics;
using DanmakuEngine.Allocations;
using DanmakuEngine.Arguments;
using DanmakuEngine.Engine;

namespace DanmakuEngine.Timing;

public class Time
{
    public static Stopwatch AppTimer { get; } = Stopwatch.StartNew();
    public static Stopwatch EngineTimer { get; internal set; } = null!;

    public static readonly StandardClock Clock = new();

    /// <summary>
    /// The number of fixed update frames that have passed since the game started.
    /// </summary>
    public static long FixedUpdateCount { get; internal set; }

    public static double RealFixedUpdateDelta { get; internal set; }
    public static double RealLastFixedUpdateTime { get; internal set; }

    public static double LastFixedUpdateTimeWithErrors => FixedElapsedSecondsNonScaled + AccumulatedErrorForFixedUpdate;
    public static double AccumulatedErrorForFixedUpdate { get; internal set; }
    public static double ExcessFixedFrameTime { get; internal set; }

    /// <summary>
    /// The value of <see cref="FixedUpdateCount"/> * <see cref="FixedUpdateDelta"/>
    /// </summary>
    public static double FixedElapsedSeconds => FixedUpdateCount * FixedUpdateDelta;
    public static double FixedElapsedSecondsNonScaled => FixedUpdateCount * FixedUpdateDeltaNonScaled;

    /// <summary>
    /// The value of <see cref="FixedUpdateCount"/> + elapsed time since the last fixed update
    /// </summary>
    public static double ElapsedSeconds { get; internal set; }

    public static double ElapsedSecondsNonScaled { get; internal set; }

    /// <summary>
    /// Update delta of the last frame in seconds
    /// </summary>
    public static double UpdateDelta { get; internal set; }

    public static float UpdateDeltaF { get; internal set; }

    public static double UpdateDeltaNonScaled { get; internal set; }
    public static double UpdateDeltaNonScaledF { get; internal set; }

    public static double FixedUpdateHz => FixedUpdateHzNonScaled * GlobalTimeScale;
    public static double FixedUpdateHzNonScaled { get; protected set; } = 60.0;

    public static double FixedUpdateDelta => (1.0 * GlobalTimeScale) / FixedUpdateHz;

    public static float FixedUpdateDeltaF => (float)FixedUpdateDelta;

    public static double FixedUpdateDeltaNonScaled => 1.0 / FixedUpdateHz;

    public static float FixedUpdateDeltaNonScaledF => (float)FixedUpdateDeltaNonScaled;

    public static double RealFixedUpdateFramerate = 60.0;

    public static double GlobalTimeScale { get; internal set; } = 1.0;

    internal static void ApplyTimeScale()
    {
        if (GlobalTimeScale == _requestedTimeScale)
            return;

        GlobalTimeScale = _requestedTimeScale;
    }

    private static double _requestedTimeScale = 1.0;
    public static void SetGlobalTimeScale(double scale)
    {
        ArgumentOutOfRangeException
            .ThrowIfNegativeOrZero(scale, nameof(scale));

        _requestedTimeScale = scale;
    }

    public static int QueuedFixedUpdateCount { get; internal set; }
    public static bool DoFixedUpdate => QueuedFixedUpdateCount > 0;

    public double DebugFpsHz { get; protected set; } = 1.0;
}
