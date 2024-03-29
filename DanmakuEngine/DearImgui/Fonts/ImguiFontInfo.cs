using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DanmakuEngine.Logging;
using SixLabors.Fonts;
namespace DanmakuEngine.DearImgui.Fonts;

public class ImguiFontInfo(
    string name, float scale, int index, ImguiLocale locale)
{
    public string Name { get; private set; } = name;
    public float Scale { get; private set; } = scale;
    public int Index { get; private set; } = index;
    public ImguiLocale Locale { get; private set; } = locale;

    public bool TryGetFamily([NotNullWhen(true)] out FontFamily? family)
    {
        try
        {
            family = SystemFonts.Get(Name);
            Logger.Debug($"Font family {Name} found.");
            return true;
        }
        catch
        {
            Logger.Warn($"Font family {Name} not found.");
            family = null;
            return false;
        }
    }

    [Conditional("DEBUG")]
    public static void DebugAllAvaliableFonts()
    {
        Logger.Debug("All available fonts:");
        foreach (var font in SystemFonts.Families)
        {
            Logger.Debug($"    {font.Name}");
        }
    }
}
