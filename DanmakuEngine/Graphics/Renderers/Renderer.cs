using Silk.NET.SDL;

namespace DanmakuEngine.Graphics.Renderers;

public abstract class Renderer : IRenderer, IDisposable
{
    public bool Initialized { get; protected set; }

    public abstract bool VSync { get; set; }

    public abstract void Initialize();

    public abstract void UnbindCurrent();

    public abstract void MakeCurrent();

    public abstract void SwapBuffers();

    public abstract Texture CreateTexture(int width, int height);

    public abstract void BindTexture(Texture texture);

    public abstract void BeginFrame();

    public abstract void EndFrame();

    public void WaitForVSync()
    {
        if (!VSync)
            return;

        WaitForVSyncInternal();
    }

    protected abstract void WaitForVSyncInternal();

    public abstract void Viewport(int x, int y, int width, int height);

    public abstract void ClearScreen();

    public abstract void ClearScreen(float r, float g, float b, float a);

    public abstract void SetClearColor(float r, float g, float b, float a);

    public abstract Texture[] GetAllTextures();

    public abstract void Dispose();
}
