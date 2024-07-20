using DanmakuEngine.Graphics.Colors;
using DanmakuEngine.Utils;

namespace DanmakuEngine.Transformation;

public class ColorLerpHandle : ILerpHandle<RGBAColor>
{
    private HkSLColor _start;
    private HkSLColor _end;

    private float _startAlpha;
    private float _endAlpha;

    private bool _flipPath = false;

    public ColorLerpHandle(RGBAColor start, RGBAColor end)
    {
        _start = HkSLColor.FromRGBA(start);
        _end = HkSLColor.FromRGBA(end);

        _startAlpha = start.A;
        _endAlpha = end.A;

        if (_start.Hk > _end.Hk)
        {
            _flipPath = true;
            (_start.Hk, _end.Hk) = (_end.Hk, _start.Hk);
        }
    }

    public RGBAColor Lerp(float t)
    {
        float ah = _start.Hk, bh = _end.Hk;
        float delta = bh - ah;

        float th = t;

        if (_flipPath)
            th = -th;

        float hk;
        if (delta > 0.5f)
            hk = MathUtils.Lerp(ah + 1, bh, th) % 1;
        else
            hk = ah + delta * th;

        var hsl = new HkSLColor
        {
            Hk = hk,
            S = MathUtils.Lerp(_start.S, _end.S, t),
            L = MathUtils.Lerp(_start.L, _end.L, t),
        };

        var a = MathUtils.Lerp(_startAlpha, _endAlpha, t);

        return hsl.toRGBAColorInternal(a / 255f);
    }
}
