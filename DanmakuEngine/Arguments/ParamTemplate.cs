// This file defines all avaliable arguments and default value

using System.ComponentModel;
using DanmakuEngine.Configuration;
using DanmakuEngine.Games.Platform;
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
            var enabled = arg.GetValue<bool>();
            
            if (enabled && DesktopGameHost.IsWindows && !ConfigManager.HasConsole)
                WindowsGameHost.CreateConsole();
            
            var status = enabled ? "enabled" : "disabled";
            Logger.Debug($"Debug mode {status}");
        });
    
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

    // For some arguments need non-static support.
    public ParamTemplate()
    {
        Help = new("-help", _ => PrintHelp());

        HelpShort = new("-h", _ => PrintHelp());

        Children = new()
        {
            { RefreshRate, "Specify the refresh rate of the game" },
            { Vsync, "Specify whether to enable Vsync" },
            { Fullscreen, "Specify whether to play the game under fullscreen mode" },
            { Debug, "Enable debug-mode" },
            { LogDirectory, "Specify the directory of the log files" },
            { Help, "Print the help screen" },
            { HelpShort, "Print the help screen" },
            { ClearScreen, "Clear screen before rendering the next frame" },
        };
    }

    private void PrintHelp()
    {
        using var argParser = new ArgumentParser(this, null!, false);

        argParser.PrintHelp();

        Environment.Exit(0);
    }
}