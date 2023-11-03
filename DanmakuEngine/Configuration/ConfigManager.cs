using System.ComponentModel.DataAnnotations;
using DanmakuEngine.Arguments;
using DanmakuEngine.Dependency;

namespace DanmakuEngine.Configuration;

public class ConfigManager : IInjectable
{
    [LoadFromArgument("-refresh")]
    public int RefreshRate { get; private set; }

    [LoadFromArgument("-debug")]
    public bool DebugMode { get; private set; }

    [LoadFromArgument("-fullscreen")]
    public bool FullScreen { get; private set; }

    [LoadFromArgument("-vsync")]
    public bool Vsync { get; private set; }

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

            propInfo.SetValue(_configManager, value);
        }
    }

    public bool DebugBuild
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

    public void Inject(DependencyContainer container)
    {
        _configManager = container.Get<ConfigManager>();
    }
    
    [Inject]
    private ConfigManager _configManager = null!;
}