using System.Numerics;
using DanmakuEngine.DearImgui.Windowing;
using DanmakuEngine.Graphics.Colors;
using ImGuiNET;

namespace DanmakuEngine.Games.Screens.MainMenu;

public class LerpTestWindow : ImguiWindowBase
{
    public LerpTestWindow() : base("Lerp Test")
    {
    }

    private SRGBColor? _result1 = null;
    private SRGBColor? _result2 = null;
    private SRGBColor? _result3 = null;
    private SRGBColor? _result4 = null;
    private SRGBColor? _result5 = null;

    private SRGBLerpHandle? _srgbHandle = null;
    private HslLerpHandle? _hslHandle = null;
    private LchLerpHandle? _lchHandle = null;
    private LabLerpHandle? _labHandle = null;
    private OklabLerpHandle? _oklabHandle = null;

    private Vector4 _startColor = new(1, 0, 0, 1);

    private Vector4 _endColor = new(0, 1, 0, 1);

    private float _lerpValue = 0.5f;

    protected override void Update()
    {
        bool baseChanged = false;

        baseChanged |= ImGui.ColorPicker4("Start Color", ref _startColor);

        baseChanged |= ImGui.ColorPicker4("End Color", ref _endColor);

        if (_srgbHandle is null || _hslHandle is null || _lchHandle is null || _labHandle is null || _oklabHandle is null || baseChanged)
        {
            _srgbHandle = new SRGBLerpHandle(SRGBColor.FromFloatRGB(_startColor), SRGBColor.FromFloatRGB(_endColor));
            _hslHandle = new HslLerpHandle(SRGBColor.FromFloatRGB(_startColor), SRGBColor.FromFloatRGB(_endColor));
            _lchHandle = new LchLerpHandle(SRGBColor.FromFloatRGB(_startColor), SRGBColor.FromFloatRGB(_endColor));
            _labHandle = new LabLerpHandle(SRGBColor.FromFloatRGB(_startColor), SRGBColor.FromFloatRGB(_endColor));
            _oklabHandle = new OklabLerpHandle(SRGBColor.FromFloatRGB(_startColor), SRGBColor.FromFloatRGB(_endColor));
        }

        bool lerpChanged = ImGui.SliderFloat("factor", ref _lerpValue, 0, 1);

        if (lerpChanged || baseChanged || _result1 is null || _result2 is null || _result3 is null || _result4 is null || _result5 is null)
        {
            _result1 = _srgbHandle.Lerp(_lerpValue);
            _result2 = _hslHandle.Lerp(_lerpValue);
            _result3 = _lchHandle.Lerp(_lerpValue);
            _result4 = _labHandle.Lerp(_lerpValue);
            _result5 = _oklabHandle.Lerp(_lerpValue);
        }

        var result1 = _result1.Value.ToFloatRGB();
        var result2 = _result2.Value.ToFloatRGB();
        var result3 = _result3.Value.ToFloatRGB();
        var result4 = _result4.Value.ToFloatRGB();
        var result5 = _result5.Value.ToFloatRGB();

        ImGui.ColorButton("linear sRGB lerp", result1, ImGuiColorEditFlags.None, new Vector2
        {
            X = 100,
            Y = 100,
        });

        ImGui.ColorButton("Hsl lerp", result2, ImGuiColorEditFlags.None, new Vector2
        {
            X = 100,
            Y = 100,
        });

        ImGui.ColorButton("Lch lerp", result3, ImGuiColorEditFlags.None, new Vector2
        {
            X = 100,
            Y = 100,
        });

        ImGui.ColorButton("Lab lerp", result4, ImGuiColorEditFlags.None, new Vector2
        {
            X = 100,
            Y = 100,
        });

        ImGui.ColorButton("Oklab lerp", result5, ImGuiColorEditFlags.None, new Vector2
        {
            X = 100,
            Y = 100,
        });
    }
}
