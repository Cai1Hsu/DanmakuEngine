using DanmakuEngine.DearImgui;
using DanmakuEngine.Graphics.Renderers;
using DanmakuEngine.Logging;
using DanmakuEngine.Threading;

namespace DanmakuEngine.Engine.Threading;

public class RenderThread : GameThread
{
    private Renderer _renderer;
    public RenderThread(Action task, Renderer renderer)
        : base(task, ThreadType.Render)
    {
        _renderer = renderer;
    }

    internal override void MakeCurrent()
    {
        base.MakeCurrent();

        ThreadSync.IsRenderThread = true;
    }

    protected override void PrepareForWork()
    {
        Logger.Debug("[Main-Thread] Preparing render thread");

        // Still in the main thread
        _renderer.UnbindCurrent();

        base.PrepareForWork();
    }

    protected override void OnInitialize()
    {
        Logger.Debug("[Render-Thread] Initializing render thread");

        // In the render thread
        _renderer.MakeCurrent();
    }

    protected override void OnExit()
    {
        Imgui.DisposeGLResources();
    }

    public override bool IsCurrent => ThreadSync.IsRenderThread;
}
