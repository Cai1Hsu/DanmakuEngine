using DanmakuEngine.Graphics.Colors;
using DanmakuEngine.Utils;

namespace DanmakuEngine.Transformation;

public class HslLerpHandle : ILerpHandle<RGBAColor>
{
    private HkSLColor _start;
    private HkSLColor _end;

    private float _startAlpha;
    private float _endAlpha;

    public HslLerpHandle(RGBAColor start, RGBAColor end)
        : this(HkSLColor.FromRGBA(start), HkSLColor.FromRGBA(end), start.A, end.A)
    {
    }

    public HslLerpHandle(HkSLColor start, HkSLColor end, float startAlpha, float endAlpha)
    {
        _start = start;
        _end = end;

        _startAlpha = startAlpha;
        _endAlpha = endAlpha;
    }

    private HkSLColor hslLerp(float t)
    {
        float ah = _start.Hk, bh = _end.Hk;
        float delta = bh - ah;

        float hk, ht;
        if (ah > bh)
        {
            (ah, bh) = (bh, ah);

            delta = -delta;
            ht = 1 - t;
        }
        else
        {
            ht = t;
        }

        if (delta > 0.5f)
            hk = MathUtils.Lerp(ah + 1, bh, ht) % 1;
        else
            hk = ah + delta * ht;

        return new HkSLColor
        {
            Hk = hk,
            S = MathUtils.Lerp(_start.S, _end.S, t),
            L = MathUtils.Lerp(_start.L, _end.L, t),
        };
    }

    public RGBAColor Lerp(float t)
    {
        float a = MathUtils.Lerp(_startAlpha, _endAlpha, t);

        return hslLerp(t).ToRGBAColor(a / 255f);
    }
}
