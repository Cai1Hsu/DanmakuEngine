using DanmakuEngine.Threading;

namespace DanmakuEngine.Engine.Threading;

public class RenderThread : GameThread
{
    public RenderThread(Action task)
        : base(task, ThreadType.Render)
    {
    }

    internal override void MakeCurrent()
    {
        base.MakeCurrent();

        ThreadSync.IsRenderThread = true;
    }

    public override bool IsCurrent => ThreadSync.IsRenderThread;
}
