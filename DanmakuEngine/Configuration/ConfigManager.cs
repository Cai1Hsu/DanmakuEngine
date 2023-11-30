using DanmakuEngine.Arguments;
using DanmakuEngine.Dependency;
using DanmakuEngine.Engine.Platform;
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

    private readonly List<string> skipProperties = new();

    public void DynamicLoadDefaultValues()
    {
        DynamicFetchVideoMode();
    }

    public void DynamicFetchVideoMode()
    {
#if !HEADLESS
        try
        {
            var mainMonitor = Silk.NET.Windowing.Monitor.GetMainMonitor(null);

            if (mainMonitor is not null)
            {
                var videoMode = mainMonitor.VideoMode;

                Logger.Debug($"Main monitor video mode: {videoMode.Resolution!.Value.X}x{videoMode.Resolution!.Value.Y}@{videoMode.RefreshRate}Hz");

                RefreshRate = videoMode.RefreshRate.GetValueOrDefault(60);
            }
            else
            {
                Logger.Debug("No main monitor found, using default refresh rate 60Hz");

                RefreshRate = 60;
            }

            skipProperties.Add(nameof(RefreshRate));
        }
        catch (Exception)
        {
            // ignored
        }
#endif
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

            // see DynamicLoadDefaultValues()
            // This helps us to avoid overriding the default value loaded from runtime
            // For example, we load default value of RefreshRate at runtime instead of compile time
            if (skipProperties.Contains(propInfo.Name))
            {
                Logger.Debug($"[ConfigManager] skipping: Property {propInfo.Name} is already loaded from runtime, value: {propInfo.GetValue(this)}");

                continue;
            }

            if (!argProvider.IsSupport(flag))
                throw new NotSupportedException($"Unrecognized flag {flag} for property {propInfo.Name}");

            var value = argProvider.GetValue(flag);

            propInfo.SetValue(this, value);
        }
    }

    public static bool DebugBuild
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }

    private static bool? _hasConsole = null;
    public static bool HasConsole
    {
        get
        {
            if (_hasConsole.HasValue)
                return _hasConsole.Value;

            _hasConsole = WindowsGameHost.HasConsole();

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