using System.Runtime.CompilerServices;
using DanmakuEngine.Transformation;
using DanmakuEngine.Utils;

namespace DanmakuEngine.Graphics.Colors;

public class LabLerpHandle : ILerpHandle<SRGBColor>
{
    private LabColor _start;
    private LabColor _end;

    public LabLerpHandle(SRGBColor start, SRGBColor end)
    {
        _start = LabColor.FromSRGB(start);
        _end = LabColor.FromSRGB(end);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public SRGBColor Lerp(float t)
    {
        var lab = new LabColor
        {
            L = MathUtils.Lerp(_start.L, _end.L, t),
            A = MathUtils.Lerp(_start.A, _end.A, t),
            B = MathUtils.Lerp(_start.B, _end.B, t),
        };

        return lab.ToSRGB();
    }
}
