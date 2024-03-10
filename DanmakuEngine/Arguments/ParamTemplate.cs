// This file defines all avaliable arguments and default value

using System.ComponentModel;
using DanmakuEngine.Configuration;
using DanmakuEngine.Engine.Platform;
using DanmakuEngine.Engine.Platform.Windows;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Arguments;

public partial class ParamTemplate : Paramaters
{
    [Description("Specify the refresh rate of the game")]
    public Argument RefreshRate =
        new Argument("-refresh", typeof(int), 60);

    [Description("Specify whether to enable Vsync")]
    public Argument Vsync =
        new("-vsync", typeof(bool), true);

    [Description("Specify whether to play the game under fullscreen mode")]
    public Argument Fullscreen =
        new("-fullscreen", typeof(bool), false);

    [Description("Specify whether to play the game under exclusive fullscreen mode, only works when fullscreen is enabled")]
    public Argument Exclusive =
        new("-exclusive", typeof(bool), true);

    [Description("Enable debug-mode")]
    public Argument Debug =
        new("-debug", typeof(bool),
#if DEBUG
        true
#else
        false
#endif
        );

    [Description("Clear screen before rendering the next frame")]
    public Argument ClearScreen =
        new("-clear", typeof(bool), false);

    [Description("Specify the directory of the log files")]
    public Argument LogDirectory =
        new("-log", typeof(string), Environment.CurrentDirectory);

    [Description("Print the help screen")]
    public Argument Help;

    [Description("Print the help screen")]
    public Argument HelpShort;

    [Description("Represents and controls the update frequency of the fps in debug console")]
    public Argument FpsUpdateFrequency =
        new("-frequency", typeof(double), 1.00);

    [Description("Specify whether to run the game in headless mode")]
    public Argument Headless =
        new("-headless", typeof(bool), false);

    [Description("Specify whether to run the game in single-threaded mode")]
    public Argument SingleThreaded =
        new("-singlethread", typeof(bool), false);

    // For some arguments need non-static support.
    public ParamTemplate()
    {
        Help = new("-help", _ => PrintHelp());

        HelpShort = new("-h", _ => PrintHelp());

        setupChildren();
    }

    private void PrintHelp()
    {
        using var argParser = new ArgumentParser(this, null!, false);

        argParser.PrintHelp();

        Environment.Exit(0);
    }
}
