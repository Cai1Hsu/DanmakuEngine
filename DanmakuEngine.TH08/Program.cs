using DanmakuEngine.Engine;
using DanmakuEngine.Engine.Platform;
using DanmakuEngine.TH08.Games;
using DanmakuEngine.Logging;

internal class Program
{

    [STAThread]
    private static void Main(string[] args)
    {
        var game = new TH08Game();

        #if HEADLESS
            Logger.Debug("Running in headless mode.");

            using (var host = new HeadlessGameHost(() => true))
            {
                host.Run(game);
            }
        #else
            using (var host = DesktopGameHost.GetSuitableHost())
            {
                host.Run(game);
            }
        #endif
    }
}