using System.Diagnostics;
using DanmakuEngine.Threading;

namespace DanmakuEngine.Engine.Threading;

public class MainThread : GameThread
{
    public MainThread(Action task)
        : base(task, ThreadType.Main)
    {
    }

    internal override void MakeCurrent()
    {
        base.MakeCurrent();

        ThreadSync.IsMainThread = true;
    }

    protected override void PrepareForWork()
    {
        Initialize();

        Thread.CurrentThread.Name = "GameThread-Main";

        // Do not create extra thread for Main GameThread.
        Debug.Assert(this.Thread is null);
    }

    public override bool IsCurrent => ThreadSync.IsMainThread;
}
