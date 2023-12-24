using DanmakuEngine.Threading;

namespace DanmakuEngine.Engine.Threading;

public class UpdateThread : GameThread
{
    public UpdateThread(Action<double> task)
        : base(task, ThreadType.Update)
    {
    }

    public override void MakeCurrent()
    {
        base.MakeCurrent();

        ThreadSync.IsUpdateThread = true;
    }

    public override bool IsCurrent => ThreadSync.IsUpdateThread;
}
