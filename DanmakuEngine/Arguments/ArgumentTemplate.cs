// This file defines all avaliable arguments and default value

using System.ComponentModel;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Arguments;

public class ParamTemplate : Paramaters
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

    [Description("Enable debug-mode")]
    public Argument Debug =
        new("-debug", typeof(bool), false, arg =>
        {
            var status = arg.GetValue<bool>() ? "enabled" : "disabled";
            Logger.Debug($"Debug mode {status}");
        });

    [Description("Specify the directory of the log files")]
    public Argument LogDirectory =
        new("-log", typeof(string), Environment.CurrentDirectory);

    [Description("Print the help screen")]
    public Argument Help;


    [Description("Print the help screen")]
    public Argument HelpShort;

    // For some arguments need non-static support.
    public ParamTemplate()
    {
        Help = new("-help", _ => PrintHelp());

        HelpShort = new("-h", _ => PrintHelp());
    }

    private void PrintHelp()
    {
        using var argParser = new ArgumentParser(this, null!, false);

        argParser.PrintHelp();

        Environment.Exit(0);
    }
}