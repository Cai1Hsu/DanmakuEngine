using System.Runtime.CompilerServices;
using DanmakuEngine.Transformation;
using DanmakuEngine.Utils;

namespace DanmakuEngine.Graphics.Colors;

public class OklabLerpHandle : ILerpHandle<SRGBColor>
{
    private OklabColor _start;
    private OklabColor _end;

    public OklabLerpHandle(SRGBColor start, SRGBColor end)
    {
        _start = OklabColor.FromSRGB(start);
        _end = OklabColor.FromSRGB(end);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public SRGBColor Lerp(float t)
    {
        var lerped = new OklabColor
        {
            L = MathUtils.Lerp(_start.L, _end.L, t),
            A = MathUtils.Lerp(_start.A, _end.A, t),
            B = MathUtils.Lerp(_start.B, _end.B, t),
            Opacity = MathUtils.Lerp(_start.Opacity, _end.Opacity, t),
        };

        return lerped.ToSRGB();
    }
}
