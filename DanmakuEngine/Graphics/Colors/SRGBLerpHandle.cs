using System.Numerics;
using DanmakuEngine.Extensions;
using DanmakuEngine.Transformation;

namespace DanmakuEngine.Graphics.Colors;

public class SRGBLerpHandle : ILerpHandle<SRGBColor>
{
    private Vector4 _startLinear;
    private Vector4 _endLinear;

    public SRGBColor Lerp(float t)
        => ColorExtensions.FromLinearVector4(Vector4.Lerp(_startLinear, _endLinear, t));

    public SRGBLerpHandle(SRGBColor start, SRGBColor end)
    {
        _startLinear = start.ToLinear();
        _endLinear = end.ToLinear();
    }
}
