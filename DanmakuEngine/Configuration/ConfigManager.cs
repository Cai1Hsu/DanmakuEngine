using System.ComponentModel.DataAnnotations;
using DanmakuEngine.Arguments;

namespace DanmakuEngine.Configuration;

public static class ConfigManager
{
    [LoadFromArgument("-refresh")]
    public static int RefreshRate { get; private set; }

    [LoadFromArgument("-debug")]
    public static bool DebugMode { get; private set; }

    [LoadFromArgument("-fullscreen")]
    public static bool FullScreen { get; private set; }

    [LoadFromArgument("-vsync")]
    public static bool Vsync { get; private set; }

    public static void LoadFromArguments(ArgumentProvider argProvider)
    {
        foreach (var propInfo in typeof(ConfigManager).GetProperties())
        {
            var attribute = propInfo.GetCustomAttributes(typeof(LoadFromArgumentAttribute), false);

            if (attribute == null || attribute.Length == 0)
                continue;

            var flag = ((LoadFromArgumentAttribute)attribute.First()).Flag;

            if (!argProvider.IsSupport(flag))
                throw new NotSupportedException($"Unrecognized flag {flag} for property {propInfo.Name}");

            var value = argProvider.GetValue(flag);

            propInfo.SetValue(value, null);
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