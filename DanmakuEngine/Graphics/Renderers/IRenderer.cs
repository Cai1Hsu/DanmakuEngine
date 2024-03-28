using Silk.NET.SDL;

namespace DanmakuEngine.Graphics.Renderers;

public interface IRenderer
{
    void Initialize();

    void MakeCurrent();

    void SwapBuffers();

    Texture CreateTexture(int width, int height);

    void BindTexture(Texture texture);

    void BeginFrame();

    void EndFrame();

    void WaitForVSync();

    void ClearScreen();

    void SetClearColor(float r, float g, float b, float a);

    void Viewport(int x, int y, int width, int height);

    Texture[] GetAllTextures();
}
