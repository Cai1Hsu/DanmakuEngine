using DanmakuEngine.Threading;

namespace DanmakuEngine.Engine.Threading;

public class RenderThread : GameThread
{
    public RenderThread(Action<double> task)
        : base(task, ThreadType.Render)
    {
    }

    public override void MakeCurrent()
    {
        base.MakeCurrent();

        ThreadSync.IsRenderThread = true;
    }

    public override bool IsCurrent => ThreadSync.IsRenderThread;
}