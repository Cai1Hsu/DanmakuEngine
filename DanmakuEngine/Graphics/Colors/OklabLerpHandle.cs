using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DanmakuEngine.Extensions;
using DanmakuEngine.Transformation;
using DanmakuEngine.Utils;

namespace DanmakuEngine.Graphics.Colors;

public class OklabLerpHandle : ILerpHandle<SRGBColor>
{
    private Lms _start;
    private Lms _end;

    private float _startAlpha;
    private float _endAlpha;

    public OklabLerpHandle(SRGBColor start, SRGBColor end)
    {
        _start = linearToLms(start.ToLinearRGB());
        _end = linearToLms(end.ToLinearRGB());

        _startAlpha = start.A;
        _endAlpha = end.A;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public SRGBColor Lerp(float t)
    {
        var lerped = new Lms
        {
            L = MathUtils.Lerp(_start.L, _end.L, t),
            M = MathUtils.Lerp(_start.M, _end.M, t),
            S = MathUtils.Lerp(_start.S, _end.S, t),
        };

        Vector3 linear = lmsToLinear(lerped);

        return new SRGBColor
        {
            R = ColorExtensions.ToSRGB(linear.X) * 255f,
            G = ColorExtensions.ToSRGB(linear.Y) * 255f,
            B = ColorExtensions.ToSRGB(linear.Z) * 255f,
            A = MathUtils.Lerp(_startAlpha, _endAlpha, t),
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Lms
    {
        public float L;
        public float M;
        public float S;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static Lms linearToLms(Vector3 linear)
    {
        float l = (0.4122214708f * linear.X) + (0.5363325363f * linear.Y) + (0.0514459929f * linear.Z);
        float m = (0.2119034982f * linear.X) + (0.6806995451f * linear.Y) + (0.1073969566f * linear.Z);
        float s = (0.0883024619f * linear.X) + (0.2817188376f * linear.Y) + (0.6299787005f * linear.Z);

        float l_ = MathF.Cbrt(l);
        float m_ = MathF.Cbrt(m);
        float s_ = MathF.Cbrt(s);

        return new Lms
        {
            L = l_,
            M = m_,
            S = s_,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static Vector3 lmsToLinear(Lms lms)
    {
        float l = lms.L;
        float m = lms.M;
        float s = lms.S;

        l = l * l * l;
        m = m * m * m;
        s = s * s * s;

        return new Vector3(
            (+4.0767416621f * l) - (3.3077115913f * m) + (0.2309699292f * s),
            (-1.2684380046f * l) + (2.6097574011f * m) - (0.3413193965f * s),
            (-0.0041960863f * l) - (0.7034186147f * m) + (1.7076147010f * s)
        );
    }
}
