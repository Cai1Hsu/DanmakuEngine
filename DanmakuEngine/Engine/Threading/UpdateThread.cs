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

        Time.UpdateDelta = DeltaTime;
        Time.UpdateDeltaF = (float)DeltaTime;

        Time.ElapsedSeconds = ElapsedSeconds;

        
    }
    public override bool IsCurrent => ThreadSync.IsUpdateThread;
}
