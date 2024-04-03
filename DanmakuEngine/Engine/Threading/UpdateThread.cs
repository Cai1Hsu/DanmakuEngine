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

    public override bool IsCurrent => ThreadSync.IsUpdateThread;
}
