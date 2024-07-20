using DanmakuEngine.Graphics.Colors;

namespace DanmakuEngine.Transformation;

public class RGBALerpHandle : ILerpHandle<RGBAColor>
{
    private RGBAColor _start;
    private RGBAColor _end;

    public RGBAColor Lerp(float t)
    {
        return new RGBAColor
        {
            R = _start.R + ((_end.R - _start.R) * t),
            G = _start.G + ((_end.G - _start.G) * t),
            B = _start.B + ((_end.B - _start.B) * t),
            A = _start.A + ((_end.A - _start.A) * t)
        };
    }

    public RGBALerpHandle(RGBAColor start, RGBAColor end)
    {
        _start = start;
        _end = end;
    }
}
