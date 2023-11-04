using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using DanmakuEngine.Arguments;
using DanmakuEngine.Dependency;
using DanmakuEngine.Games.Platform;

namespace DanmakuEngine.Configuration;

public class ConfigManager : IInjectable
{
    [LoadFromArgument("-refresh")] public int RefreshRate { get; private set; }

    [LoadFromArgument("-debug")] public bool DebugMode { get; private set; }

    [LoadFromArgument("-fullscreen")] public bool FullScreen { get; private set; }

    [LoadFromArgument("-vsync")] public bool Vsync { get; private set; }

    [LoadFromArgument("-clear")] public bool ClearScreen { get; private set; }

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
    public static void UpdateConsoleStatus()
        => _hasConsole = WindowsGameHost.HasConsole();

    static ConfigManager()
    {
        // if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        // {
        //     HasConsole = GetConsoleWindow() != IntPtr.Zero;
        // }
    }

    [AttributeUsage(AttributeTargets.Property)]
    private class LoadFromArgumentAttribute : Attribute
    {
        public string Flag { get; set; }

        public LoadFromArgumentAttribute(string flag)
        {
            this.Flag = flag;
        }
    }
}