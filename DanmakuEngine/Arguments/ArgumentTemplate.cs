// This file defines all avaliable arguments and default value

using System.ComponentModel;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Arguments;

public class ArgumentTemplate : IArgumentTemplate
{
    [Description("Specify the refresh rate of the game")]
    public Argument RefreshRate =
        new Argument("-refresh", typeof(int), 60);

    [Description("Enable debug-mode")]
    public Argument Debug =
        new("-debug", _ =>
        {
            Logger.Debug("Debug-mode enabled");
        });

    [Description("Print the help screen")]
    public Argument Help;


    [Description("Print the help screen")]
    public Argument HelpShort;

    // For some arguments need non-static support.
    public ArgumentTemplate()
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