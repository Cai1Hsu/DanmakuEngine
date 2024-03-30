using System.Diagnostics;
using System.Globalization;
using DanmakuEngine.DearImgui.Fonts;
using DanmakuEngine.Engine;
using DanmakuEngine.Logging;
using ImGuiNET;

namespace DanmakuEngine.DearImgui;

public static partial class Imgui
{
    public static bool LoadCustomFonts { get; set; } = true;
    public static float DefaultFontSize { get; set; } = 20.0f;
    private static float _dpiScale = 1.0f;
    public static bool ScaleByDpi { get; set; } = false;
    public static bool UseLowerSampleH = true;

    private static void loadFonts()
    {
        if (!_initialized)
            return;

        if (!LoadCustomFonts)
        {
            _io.Fonts.AddFontDefault();

            return;
        }

        _dpiScale = getDpiScale();

        ImguiLocale locale = getCurrentLocale();

        var loadableLists = getLoadableFonts(locale);

        if (!loadableLists.Any())
        {
            ImguiFontInfo.DebugAllAvaliableFonts();
            Logger.Warn($"No available fonts found in the fallback list. Loading default font.");

            _io.Fonts.AddFontDefault();
            return;
        }
        else
        {
            Logger.Debug($"Selected {loadableLists.Count()} available fonts in the fallback list.");
            foreach (var (info, _) in loadableLists)
                Logger.Debug($"    {info.Name}");
        }

        using (var _ = StateObject.ContextState())
        {
            bool first = true;
            foreach (var (info, data) in loadableLists)
            {
                createOrMergeFont(info, data, first);
                first = false;
            }

            unsafe
            {
                ImGuiNative.ImFontAtlas_Build(_io.Fonts);
            }
        }

        recreateFontDeviceTexture();
    }

    private static float getDpiScale()
    {
        // This should happen, because we use SDL as our graphics backend
        if (!SDL.IsInitialized)
            return 1.0f;

        float ddpi = 0, hdpi = 0, vdpi = 0;

        unsafe
        {
            SDL.Api.GetDisplayDPI(0, &ddpi, &hdpi, &vdpi);
            return hdpi / 96.0f;
        }
    }

    private static void createOrMergeFont(ImguiFontInfo info, ImguiFontRawData data, bool first)
    {
        var fontSize = DefaultFontSize * info.Scale;

        if (ScaleByDpi)
            fontSize *= _dpiScale;

        unsafe
        {
            var p_config = ImGuiNative.ImFontConfig_ImFontConfig();
            var configPtr = new ImFontConfigPtr(p_config)
            {
                MergeMode = !first,
                FontNo = info.Index,
                RasterizerMultiply = 1.25f,
                // The data is managed by us and is going to be disposed just after the font is added
                FontDataOwnedByAtlas = false,
            };

            if (UseLowerSampleH)
                configPtr.OversampleH = 2;

            try
            {
                var rawData = data.RawData;
                var dataSize = rawData.Length;
                fixed (byte* ptr = rawData)
                {
                    _io.Fonts.AddFontFromMemoryTTF(
                        (nint)ptr, dataSize, fontSize, configPtr, getGlyphRange(info.Locale));
                }
                Logger.Debug($"Added ImGui font {info.Name} at {fontSize}pt");
            }
            finally
            {
                ImGuiNative.ImFontConfig_destroy(p_config);
                data.Dispose();
            }
        }
    }

    private static ImguiLocale getCurrentLocale()
    {
        return CultureInfo.CurrentCulture.TwoLetterISOLanguageName switch
        {
            "zh" => ImguiLocale.ZH,
            "ja" => ImguiLocale.JA,
            "en" => ImguiLocale.EN,
            _ => ImguiLocale.OTHER,
        };
    }

    private static IEnumerable<(ImguiFontInfo, ImguiFontRawData)> getLoadableFonts(ImguiLocale locale)
    {
        IList<(ImguiFontInfo, ImguiFontRawData)> fonts = [];

        switch (locale)
        {
            case ImguiLocale.ZH:
                fonts.Add(tryGetFontsRawData(JA_FONT_FALLBACKS));
                fonts.Add(tryGetFontsRawData(ZH_FONT_FALLBACKS));
                fonts.Add(tryGetFontsRawData(EN_FONT_FALLBACKS));
                break;
            case ImguiLocale.JA:
                fonts.Add(tryGetFontsRawData(JA_FONT_FALLBACKS));
                fonts.Add(tryGetFontsRawData(ZH_FONT_FALLBACKS));
                fonts.Add(tryGetFontsRawData(EN_FONT_FALLBACKS));
                break;
            case ImguiLocale.EN:
                fonts.Add(tryGetFontsRawData(EN_FONT_FALLBACKS));
                fonts.Add(tryGetFontsRawData(JA_FONT_FALLBACKS));
                fonts.Add(tryGetFontsRawData(ZH_FONT_FALLBACKS));
                break;
            default:
                fonts.Add(tryGetFontsRawData(EN_FONT_FALLBACKS));
                break;
        }

        return fonts.Where(static f => f.Item1 is not null && f.Item2 is not null);
    }

    private static (ImguiFontInfo, ImguiFontRawData) tryGetFontsRawData(
        IEnumerable<ImguiFontInfo> fontInfos)
    {
        foreach (var info in fontInfos)
        {
            if (info.TryGetFamily(out var family)
                && family.HasValue)
            {
                if (family.Value.TryGetPaths(out var paths)
                    && paths is not null)
                {
                    var name = family.Value.Name;
                    foreach (var path in paths)
                    {
                        if (!ImguiFontRawData.TryLoadRawData(name, path,
                            out var fontData))
                            continue;

                        return (info, fontData);
                    }
                }
            }
        }

        return (null!, null!);
    }

    private static ImguiFontInfo[] ZH_FONT_FALLBACKS =
    [
        new(@"微软雅黑", 1.0f, 0, ImguiLocale.ZH),
        new(@"Microsoft YaHei UI Light", 0.88f, 1, ImguiLocale.ZH),
        new(@"Microsoft YaHei Light", 1.0f, 0, ImguiLocale.ZH),
        new(@"Microsoft YaHei", 1.0f, 0, ImguiLocale.ZH),
        new(@"黑体", 0.9f, 0, ImguiLocale.ZH),
        new(@"SimHei", 0.9f, 0, ImguiLocale.ZH),
        new(@"宋体", 0.9f, 0, ImguiLocale.ZH),
        new(@"SimSun", 0.9f, 0, ImguiLocale.ZH),
        new(@"WenQuanYi Micro Hei", 1.0f, 0, ImguiLocale.ZH),
        new(@"Noto Sans CJK SC", 1.0f, 0, ImguiLocale.ZH),
    ];

    private static ImguiFontInfo[] JA_FONT_FALLBACKS =
    [
        new(@"Meiryo UI", 1.0f, 0, ImguiLocale.JA),
        new(@"MS UI Gothic", 0.85f, 0, ImguiLocale.JA),
        new(@"Yu Gothic UI", 1.0f, 0, ImguiLocale.JA),
        new(@"MS Mincho", 0.85f, 0, ImguiLocale.JA),
        new(@"Meiryo", 1.0f, 0, ImguiLocale.JA),
        new(@"Noto Sans CJK JP", 1.0f, 0, ImguiLocale.JA),
    ];

    private static ImguiFontInfo[] EN_FONT_FALLBACKS =
    [
        new(@"Segoe UI", 1.0f, 0, ImguiLocale.EN),
        new(@"Tahoma", 0.88f, 0, ImguiLocale.EN),
        new(@"Arial", 0.9f, 0, ImguiLocale.EN),
        new(@"Noto Sans", 1.00f, 0, ImguiLocale.EN),
    ];

    private static nint getGlyphRange(ImguiLocale locale)
    {
        return locale switch
        {
            ImguiLocale.ZH => _io.Fonts.GetGlyphRangesChineseSimplifiedCommon(),
            ImguiLocale.JA => _io.Fonts.GetGlyphRangesJapanese(),
            _ => _io.Fonts.GetGlyphRangesDefault(),
        };
    }
}
