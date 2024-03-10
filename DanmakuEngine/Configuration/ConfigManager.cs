using DanmakuEngine.Arguments;
using DanmakuEngine.Dependency;
using DanmakuEngine.Engine;
using DanmakuEngine.Engine.Platform.Windows;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Configuration;

public partial class ConfigManager
{
    [LoadFromArgument("-refresh")]
    public int RefreshRate { get; private set; }

    [LoadFromArgument("-debug")]
    public bool DebugMode { get; private set; }

    [LoadFromArgument("-fullscreen")]
    public bool FullScreen { get; private set; }

    [LoadFromArgument("-exclusive")]
    public bool Exclusive { get; private set; }

    [LoadFromArgument("-vsync")]
    public bool Vsync { get; private set; }

    [LoadFromArgument("-clear")]
    public bool ClearScreen { get; private set; }

    [LoadFromArgument("-frequency")]
    public double FpsUpdateFrequency { get; private set; }

    [LoadFromArgument("-singlethread")]
    public bool Singlethreaded { get; private set; }

    public bool RunningTest { get; private set; }

    public void TestModeDectected()
    {
        RunningTest = true;
        Logger.SetPrintLevel(LogLevel.Silent);
    }

    public void LoadFromArguments(ArgumentProvider argProvider)
    {
        foreach (var propInfo in typeof(ConfigManager).GetProperties())
        {
            var attribute = propInfo.GetCustomAttributes(typeof(LoadFromArgumentAttribute), false);

            if (attribute.Length == 0
                || !attribute.Any(a => a is LoadFromArgumentAttribute))
                continue;

            var flag = ((LoadFromArgumentAttribute)attribute.Where(a => a is LoadFromArgumentAttribute).First()).Flag;

            if (!argProvider.IsSupport(flag))
                throw new NotSupportedException($"Unrecognized flag {flag} for property {propInfo.Name}");

            var value = argProvider.GetValue(flag);

            propInfo.SetValue(this, value);
        }
    }

    public static bool DebugBuild =>
#if DEBUG
            true;
#else
            false;
#endif

    private static bool? _hasConsole = null;
    public static bool HasConsole
    {
        get
        {
            if (_hasConsole.HasValue)
                return _hasConsole.Value;

            _hasConsole = GameHost.HasConsole();

            return _hasConsole.Value;
        }
    }
    public static void UpdateConsoleStatus(bool hasConsole)
        => _hasConsole = hasConsole;

    [AttributeUsage(AttributeTargets.Property)]
    private class LoadFromArgumentAttribute : Attribute
    {
        public string Flag { get; set; }

        public LoadFromArgumentAttribute(string flag)
        {
            this.Flag = flag;
        }
    }

    public bool Headless { get; private set; }

    public ConfigManager(bool headless)
    {
        this.Headless = headless;
    }
}
