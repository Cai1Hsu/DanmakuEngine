using DanmakuEngine.Arguments;
using DanmakuEngine.Engine;
using DanmakuEngine.Engine.Platform;
using DanmakuEngine.Logging;
using DanmakuEngine.TH08.Games;
using GraphicsBackend = Veldrid.GraphicsBackend;
using GraphicsBinding = DanmakuEngine.Graphics.Renderers.RendererType;

internal class Program
{

    [STAThread]
    internal static void Main(string[] args)
    {
        using var argParser = new ArgumentParser(new ParamTemplate(), args);

        var argProvider = argParser.CreateProvider();

        var headless = argProvider.GetValue<bool>("-headless");

        using GameHost host = headless ? new HeadlessGameHost(() => true)
                                  : DesktopGameHost.GetSuitableHost();

        // host.GraphicsBackend = GraphicsBackend.OpenGL;
        // host.RendererType = GraphicsBinding.Silk;

        host.Run(new TH08Game(), argProvider);
    }
}
