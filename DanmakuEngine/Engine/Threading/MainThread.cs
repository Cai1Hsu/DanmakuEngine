using DanmakuEngine.Threading;

namespace DanmakuEngine.Engine.Threading;

public class MainThread : GameThread
{
    public MainThread(Action<double> task)
        : base(task, ThreadType.Main)
    {
    }

    public override void MakeCurrent()
    {
        base.MakeCurrent();

        ThreadSync.IsMainThread = true;
    }

    public override bool IsCurrent => ThreadSync.IsMainThread;
}
