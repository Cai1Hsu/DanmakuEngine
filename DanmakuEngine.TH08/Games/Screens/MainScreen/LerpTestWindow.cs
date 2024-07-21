using System.Numerics;
using DanmakuEngine.DearImgui.Windowing;
using DanmakuEngine.Graphics.Colors;
using DanmakuEngine.Transformation;
using ImGuiNET;

namespace DanmakuEngine.Games.Screens.MainMenu;

public class LerpTestWindow : ImguiWindowBase
{
    public LerpTestWindow() : base("Lerp Test")
    {
    }

    private SRGBColor? _result = null;

    private SRGBLerpHandle? _lerpHandle = null;

    private Vector4 _startColor = new(1, 0, 0, 1);

    private Vector4 _endColor = new(0, 1, 0, 1);

    private float _lerpValue = 0.5f;

    protected override void Update()
    {
        bool baseChanged = false;

        baseChanged |= ImGui.ColorPicker4("Start Color", ref _startColor);

        baseChanged |= ImGui.ColorPicker4("End Color", ref _endColor);

        if (_lerpHandle is null || baseChanged)
        {
            _lerpHandle = new SRGBLerpHandle(SRGBColor.FromFloatRGB(_startColor), SRGBColor.FromFloatRGB(_endColor));
        }

        bool lerpChanged = ImGui.SliderFloat("factor", ref _lerpValue, 0, 1);

        if (lerpChanged || !_result.HasValue || baseChanged)
        {
            _result = _lerpHandle.Lerp(_lerpValue);
        }

        var result = _result.Value.ToFloatRGB();

        ImGui.ColorButton("Result", result, ImGuiColorEditFlags.None, new Vector2
        {
            X = 100,
            Y = 100,
        });
    }
}
