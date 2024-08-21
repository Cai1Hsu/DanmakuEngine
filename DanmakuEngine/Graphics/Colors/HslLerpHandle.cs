using System.Numerics;
using System.Runtime.CompilerServices;
using DanmakuEngine.Graphics.Colors;
using DanmakuEngine.Transformation;
using DanmakuEngine.Utils;

namespace DanmakuEngine.Graphics.Colors;

/// <summary>
/// A handle for lerping between two colors. Using HSL color space.
/// </summary>
public class HslLerpHandle : ILerpHandle<SRGBColor>
{
    private HslColor _start;
    private HslColor _end;

    private float _startAlpha;
    private float _endAlpha;

    public HslLerpHandle(SRGBColor start, SRGBColor end)
        : this(HslColor.FromRGBA(start), HslColor.FromRGBA(end), start.A, end.A)
    {
    }

    public HslLerpHandle(HslColor start, HslColor end, float startAlpha = 1.0f, float endAlpha = 1.0f)
    {
        _start = start;
        _end = end;

        _startAlpha = startAlpha;
        _endAlpha = endAlpha;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public SRGBColor Lerp(float t)
    {
        HslColor hsl;

        float ah = _start.Hk, bh = _end.Hk;
        float delta = bh - ah;

        float ht;
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
            hsl.Hk = MathUtils.Lerp(ah + 1, bh, ht) % 1;
        else
            hsl.Hk = ah + (delta * ht);

        hsl.S = MathUtils.Lerp(_start.S, _end.S, t);
        hsl.L = MathUtils.Lerp(_start.L, _end.L, t);

        float a = MathUtils.Lerp(_startAlpha, _endAlpha, t);

        return hsl.ToRGBAColor(a / 255f);
    }
}
