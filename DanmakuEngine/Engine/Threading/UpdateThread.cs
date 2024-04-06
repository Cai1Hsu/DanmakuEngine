using System.ComponentModel;
using System.Diagnostics;
using DanmakuEngine.Allocations;
using DanmakuEngine.Scheduling;
using DanmakuEngine.Threading;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Engine.Threading;

public class UpdateThread : GameThread
{
    public UpdateThread(Action task)
        : base(task, ThreadType.Update, new UpdateExecutor(task))
    {
    }

    internal override void MakeCurrent()
    {
        base.MakeCurrent();

        ThreadSync.IsUpdateThread = true;
    }

    private long lastFrameFixedUpdateCount = 0;
    protected override void postRunFrame()
    {
        base.postRunFrame();

        var currentTime = Executor.SourceTime;

        int fixedUpdateCount = (int)(Time.FixedUpdateCount - lastFrameFixedUpdateCount);
        var didFixedUpdate = fixedUpdateCount != 0;

        var error = Time.ExcessFixedFrameTime - (currentTime - Time.RealLastFixedUpdateTime);
        if (Time.LastFixedUpdateTimeWithErrors + error + Time.FixedUpdateDeltaNonScaled < currentTime)
        {
            Time.AccumulatedErrorForFixedUpdate += error;
            Time.AccumulatedErrorForFixedUpdate = Math.Max(Time.AccumulatedErrorForFixedUpdate, -Time.FixedUpdateDeltaNonScaled);
        }

        if (didFixedUpdate)
        {
            periodFixedUpdateCount += fixedUpdateCount;

            _fixedDeltaPool.CurrentAndNext = Time.RealFixedUpdateDelta;
        }

        var elapsedSinceLastCalcFramerate = ElapsedSeconds - lastCalcFixedDeltaTime;
        if (elapsedSinceLastCalcFramerate > 1.0 /* sec */)
        {
            var fixedAvg = _fixedDeltaPool.Average();
            var stddev = Math.Sqrt(_fixedDeltaPool.Average(v => Math.Pow((v - fixedAvg) * 1000, 2)));
            Time.RealFixedUpdateFramerate = 1.0 / fixedAvg;
            Time.FixedUpdateJitter = stddev;

            lastCalcFixedDeltaTime = ElapsedSeconds;
            periodFixedUpdateCount = 0;
        }

        lastFrameFixedUpdateCount = Time.FixedUpdateCount;
    }

    private RingPool<double> _fixedDeltaPool = new(128);
    private double lastCalcFixedDeltaTime = 0;
    private int periodFixedUpdateCount = 0;

    protected override void OnInitialize()
    {
        Time.FixedUpdateCount = 0;
        Time.RealFixedUpdateDelta = 0;
        Time.ExcessFixedFrameTime = 0;
        Time.AccumulatedErrorForFixedUpdate = 0;
        Time.RealLastFixedUpdateTime = 0;
        Time.ElapsedSeconds = 0;
        Time.ElapsedSecondsNonScaled = 0;

        Time.UpdateDelta = 0;
        Time.UpdateDeltaF = 0;
        Time.UpdateDeltaNonScaled = 0;
        Time.UpdateDeltaNonScaledF = 0;

        Time.RealFixedUpdateFramerate = 60.0;
        Time.RealFixedUpdateDelta = 0;
        Time.FixedUpdateJitter = 0;

        Time.QueuedFixedUpdateCount = 0;

        Time.GlobalTimeScale = 1.0;
        Time.ApplyTimeScale();

        Time.EngineTimer = Executor.TimeSource;

        OnLoad?.Invoke();
    }

    public event Action? OnLoad;

    public override bool IsCurrent => ThreadSync.IsUpdateThread;
}
