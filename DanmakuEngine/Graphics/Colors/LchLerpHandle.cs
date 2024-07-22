using System.Runtime.CompilerServices;
using DanmakuEngine.Graphics.Colors;
using DanmakuEngine.Transformation;
using DanmakuEngine.Utils;

namespace DanmakuEngine.Graphics.Colors;

public class LchLerpHandle : ILerpHandle<SRGBColor>
{
    private LchColor _start;
    private LchColor _end;

    public LchLerpHandle(SRGBColor start, SRGBColor end)
    {
        _start = LabColor.FromSRGB(start).ToLch();
        _end = LabColor.FromSRGB(end).ToLch();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public SRGBColor Lerp(float t)
    {
        var lerp = new LchColor
        {
            L = MathUtils.Lerp(_start.L, _end.L, t),
            C = MathUtils.Lerp(_start.C, _end.C, t),
            H = MathUtils.Lerp(_start.H, _end.H, t),
            Opacity = MathUtils.Lerp(_start.Opacity, _end.Opacity, t),
        };

        return lerp.ToLab().ToSRGB();
    }
}
