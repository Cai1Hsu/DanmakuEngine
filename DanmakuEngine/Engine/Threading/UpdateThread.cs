using System.Diagnostics;
using DanmakuEngine.Extensions;
using DanmakuEngine.Logging;
using DanmakuEngine.Threading;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Engine.Threading;

public class UpdateThread : GameThread
{
    public UpdateThread(Action task)
        : base(task, ThreadType.Update)
    {
    }

    internal override void MakeCurrent()
    {
        base.MakeCurrent();

        ThreadSync.IsUpdateThread = true;
    }

    private long lastFrameFixedUpdateCount = 0;
    private double lastFrameSinceLastFixedUpdate = 0;

    protected override void postRunFrame()
    {
        base.postRunFrame();

        bool didFixedUpdate = Time.FixedUpdateCount != lastFrameFixedUpdateCount;

        // Sometimes the Update ended after the next FixedUpdate should start
        // When we execute the next FixedUpdate, the ElapsedSeconds will be pull back to the last FixedUpdate
        // We want to fake a time for next frame to keep the time consistent
        if (ElapsedSeconds >= Time.LastFixedUpdateSecondsWithErrors + Time.FixedUpdateDeltaNonScaled)
        {
            var elapsedTimeNonScaled = (Time.FixedUpdateCount + 1) * Time.FixedUpdateDeltaNonScaled;
            var elapsedTime = (Time.FixedUpdateCount + 1) * Time.FixedUpdateDelta;

            // The delta is not the actual, we use this to keep the time consistent
            var deltaNonScaled = elapsedTimeNonScaled - Time.ElapsedSecondsNonScaled;
            var delta = elapsedTime - Time.ElapsedSeconds;

            Time.UpdateDeltaNonScaled = deltaNonScaled;
            Time.UpdateDeltaNonScaledF = (float)deltaNonScaled;

            Time.UpdateDelta = delta;
            Time.UpdateDeltaF = (float)delta;

            Time.ElapsedSecondsNonScaled = elapsedTimeNonScaled;
            Time.ElapsedSeconds = elapsedTime;

            // Should run FixedUpdate at the beginning of the next frame
        }
        else
        {
            var timeSinceLastFixedUpdate = ElapsedSeconds - Time.RealLastFixedUpdateElapsedSeconds;

            var deltaNonScaled = !didFixedUpdate
                ? timeSinceLastFixedUpdate - lastFrameSinceLastFixedUpdate
                : timeSinceLastFixedUpdate;

            Debug.Assert(timeSinceLastFixedUpdate >= 0, $"timeSinceLastFixedUpdate: {timeSinceLastFixedUpdate}, ElapsedSeconds: {ElapsedSeconds}, RealLastFixedUpdateElapsedSeconds: {Time.RealLastFixedUpdateElapsedSeconds}");
            Debug.Assert(deltaNonScaled >= 0, $"deltaNonScaled: {deltaNonScaled}, timeSinceLastFixedUpdate: {timeSinceLastFixedUpdate}, lastFrameSinceLastFixedUpdate: {lastFrameSinceLastFixedUpdate}");

            var delta = deltaNonScaled * Time.GlobalTimeScale;

            Time.UpdateDeltaNonScaled = deltaNonScaled;
            Time.UpdateDeltaNonScaledF = (float)deltaNonScaled;

            Time.UpdateDelta = delta;
            Time.UpdateDeltaF = (float)delta;

            Time.ElapsedSecondsNonScaled += deltaNonScaled;
            Time.ElapsedSeconds += delta;
        }

        lastFrameSinceLastFixedUpdate = ElapsedSeconds - Time.RealLastFixedUpdateElapsedSeconds;

        Debug.Assert(lastFrameSinceLastFixedUpdate >= 0);

        if (Time.LastFixedUpdateSecondsWithErrors + Time.FixedUpdateDeltaNonScaled < ElapsedSeconds)
        {
            Time.AccumulatedErrorForFixedUpdate += Time.ExcessFixedFrameTime - (ElapsedSeconds - Time.RealLastFixedUpdateElapsedSeconds);
            Time.AccumulatedErrorForFixedUpdate = Math.Max(Time.AccumulatedErrorForFixedUpdate, -Time.FixedUpdateDeltaNonScaled);
        }

        lastFrameFixedUpdateCount = Time.FixedUpdateCount;

        if (didFixedUpdate)
            fixedUpdateCount++;

        var elapsedSinceLastCalcFramerate = ElapsedSeconds - lastCalcTime;
        if (elapsedSinceLastCalcFramerate > 1.0 /* sec */)
        {
            Time.RealFixedUpdateFramerate = fixedUpdateCount / elapsedSinceLastCalcFramerate;

            lastCalcTime = ElapsedSeconds;
            fixedUpdateCount = 0;
        }
    }

    private double lastCalcTime = 0;
    private int fixedUpdateCount = 0;

    protected override void OnInitialize()
    {
        Time.FixedUpdateCount = 0;
        Time.ExcessFixedFrameTime = 0;
        Time.AccumulatedErrorForFixedUpdate = 0;
        Time.RealLastFixedUpdateElapsedSeconds = 0;
        Time.ElapsedSeconds = 0;
        Time.ElapsedSecondsNonScaled = 0;

        Time.UpdateDelta = 0;
        Time.UpdateDeltaF = 0;
        Time.UpdateDeltaNonScaled = 0;
        Time.UpdateDeltaNonScaledF = 0;

        Time.GlobalTimeScale = 1.0;

        Time.EngineTimer = Executor.TimeSource;

        OnLoad?.Invoke();
    }

    public event Action? OnLoad;

    public override bool IsCurrent => ThreadSync.IsUpdateThread;
}
