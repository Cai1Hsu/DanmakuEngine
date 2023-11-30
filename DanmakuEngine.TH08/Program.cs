using DanmakuEngine.Engine;
using DanmakuEngine.Engine.Platform;
using DanmakuEngine.TH08.Games;
using DanmakuEngine.Logging;
using DanmakuEngine.Arguments;

internal class Program
{

    [STAThread]
    private static void Main(string[] args)
    {
        var game = new TH08Game();

        using var argParser = new ArgumentParser(new ParamTemplate());
        using var argProvider = argParser.CreateArgumentProvider();

#if HEADLESS
        Logger.Debug("Running in headless mode.");

        using var host = new HeadlessGameHost(() => true);
#else
        using var host = DesktopGameHost.GetSuitableHost();
#endif
        
        host.Run(game, argProvider);
    }
}