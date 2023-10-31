// This file defines all avaliable arguments and default value

using System.ComponentModel;

namespace DanmakuEngine.Arguments;

public static class ArgumentTemplate
{
    [Description("Specify the refresh rate of the game")]
    public static Argument RefreshRate =
        new Argument("-refresh", typeof(int), 60);

    [Description("Enable debug-mode")]
    public static Argument Debug =
        new Argument("-debug", _ =>
        {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Debug-mode enabled");

            Console.ResetColor();
        });

    [Description("Print the help screen")]
    public static Argument Help =
        new Argument("-help", _ =>
        {
            PrintHelp();

            Environment.Exit(0);
        });

    [Description("Print the help screen")]
    public static Argument HelpShort =
        new Argument("-h", _ =>
        {
            PrintHelp();

            Environment.Exit(0);
        });

    public static void PrintHelp()
    {
        var helps = ArgumentParser.GenerateHelp();

        helps.ForEach(s => Console.WriteLine(s));
    }
}