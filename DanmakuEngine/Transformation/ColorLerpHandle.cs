using DanmakuEngine.Graphics.Colors;
using DanmakuEngine.Logging;
using DanmakuEngine.Utils;

namespace DanmakuEngine.Transformation;

public class ColorLerpHandle : ILerpHandle<RGBAColor>
{
    private HkSLColor _start;
    private HkSLColor _end;

    private float _startAlpha;
    private float _endAlpha;

    public ColorLerpHandle(RGBAColor start, RGBAColor end)
    {
        _start = HkSLColor.FromRGBA(start);
        _end = HkSLColor.FromRGBA(end);

        start = _start.ToRGBAColor(start.A);
        end = _end.ToRGBAColor(end.A);

        _startAlpha = start.A;
        _endAlpha = end.A;
    }

    public RGBAColor Lerp(float t)
    {
        var hsl = new HkSLColor
        {
            Hk = _start.Hk + (_end.Hk - _start.Hk) * t,
            S = _start.S + (_end.S - _start.S) * t,
            L = _start.L + (_end.L - _start.L) * t
        };

        var a = MathUtils.Lerp(_startAlpha, _endAlpha, t);

        return hsl.ToRGBAColor(a / 255f);
    }
}
