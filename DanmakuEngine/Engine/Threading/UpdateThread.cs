using System.Diagnostics;
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

    protected override void postRunFrame()
    {
        base.postRunFrame();

        Time.UpdateDeltaNonScaled = DeltaTime;
        Time.UpdateDeltaNonScaledF = (float)DeltaTime;

        Time.ElapsedSecondsNonScaled += DeltaTime;

        var delta = DeltaTime * Time.GlobalTimeScale;

        Time.UpdateDelta = delta;
        Time.UpdateDeltaF = (float)delta;

        Time.ElapsedSeconds += delta;
    }

    protected override void OnInitialize()
    {
        Time.FixedUpdateCount = 0;
        Time.MeasuredFixedUpdateElapsedSeconds = 0;
        Time.ElapsedSeconds = 0;
        Time.ElapsedSecondsNonScaled = 0;

        Time.UpdateDelta = 0;
        Time.UpdateDeltaF = 0;
        Time.UpdateDeltaNonScaled = 0;
        Time.UpdateDeltaNonScaledF = 0;

        Time.GlobalTimeScale = 1.0;

        // start timer here to prevent jit compiling time from being counted
        Time.EngineTimer = Stopwatch.StartNew();
    }

    public override bool IsCurrent => ThreadSync.IsUpdateThread;
}
