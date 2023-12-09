﻿using DanmakuEngine.Engine;
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

        var headless = argProvider.GetValue<bool>("-headless");

        using GameHost host = headless ? new HeadlessGameHost(() => true)
                                  : DesktopGameHost.GetSuitableHost();

        host.Run(game, argProvider);
    }
}