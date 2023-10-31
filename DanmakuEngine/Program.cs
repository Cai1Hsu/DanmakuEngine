// A library to create STG game
// and magnificent *Danmaku*
using DanmakuEngine.Arguments;


using (var argParser = new ArgumentParser(/*new[] {"-refresh", "120"}*/))
using (var argProvider = argParser.CreateArgumentProvider())
{
    var refreshRate = argProvider.GetValue<int>("-refresh");

    // Load everything into ConfigManager or something like that.

    Console.WriteLine($"Refresh rate: {refreshRate}");
}

Console.Write("Press any key to continue.");
Console.ReadKey(true);