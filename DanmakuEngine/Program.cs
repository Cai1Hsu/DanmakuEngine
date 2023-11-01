// A library to create STG game
// and magnificent *Danmaku*
using DanmakuEngine.Arguments;
using DanmakuEngine.Logging;

using (var argParser = new ArgumentParser(new ArgumentTemplate()/*, new[] {"-refresh", "120"}*/))
using (var argProvider = argParser.CreateArgumentProvider())
{
    var refreshRate = argProvider.GetValue<int>("-refresh");

    // Load everything into ConfigManager or something like that.
    Logger.Debug($"Refresh rate: {refreshRate}");
}
