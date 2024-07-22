using System.Runtime.CompilerServices;
using DanmakuEngine.Extensions;

namespace DanmakuEngine.Graphics.Colors;

/// <summary>
/// A color in the CIELAB color space
/// Code and consts from https://observablehq.com/@mbostock/lab-and-rgb
/// </summary>
public struct LabColor
{
    /// <summary>
    /// Lightness
    /// </summary>
    public float L;

    /// <summary>
    /// A component
    /// </summary>
    public float A;

    /// <summary>
    /// B component
    /// </summary>
    public float B;

    /// <summary>
    /// Opacity, from 0 to 1
    /// </summary>
    public float Opacity;

    public LabColor(float l, float a, float b, float opacity = 1)
    {
        L = l;
        A = a;
        B = b;
        Opacity = opacity;
    }

    public static LabColor FromLch(LchColor lch)
        => LchColor.ToLab(lch);

    public static LchColor ToLch(LabColor lab)
        => LchColor.FromLab(lab);

    public LchColor ToLch()
        => LchColor.FromLab(this);

    private const float xn = 0.96422f;
    private const float yn = 1.0f;
    private const float zn = 0.82521f;
    private const float t0 = 4.0f / 29.0f;
    private const float t1 = 6.0f / 29.0f;
    private const float t2 = 3.0f * t1 * t1;
    private const float t3 = t1 * t1 * t1;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static float xyz2Lab(float t)
        => t > t3 ? MathF.Pow(t, 1.0f / 3.0f) : (t / t2) + t0;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static float lab2Xyz(float t)
        => t > t1 ? t * t * t : t2 * (t - t0);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static SRGBColor toSRGBInternal(LabColor lab)
    {
        float y = (lab.L + 16f) / 116f,
            x = float.IsNaN(lab.A) ? y : y + (lab.A / 500f),
            z = float.IsNaN(lab.B) ? y : y - (lab.B / 200f);

        float newX = lab2Xyz(x) * xn;
        float newY = lab2Xyz(y) * yn;
        float newZ = lab2Xyz(z) * zn;

        return new SRGBColor(
            (float)(ColorExtensions.ToSRGB((+3.1338561f * newX) - (1.6168667f * newY) - (0.4906146f * newZ)) * 255f),
            (float)(ColorExtensions.ToSRGB((-0.9787684f * newX) + (1.9161415f * newY) + (0.0334540f * newZ)) * 255f),
            (float)(ColorExtensions.ToSRGB((+0.0719453f * newX) - (0.2289914f * newY) + (1.4052427f * newZ)) * 255f),
            lab.Opacity * 255f
        );
    }

    public SRGBColor ToSRGB()
        => toSRGBInternal(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static LabColor FromSRGB(SRGBColor sRGBColor)
    {
        var linear = sRGBColor.ToLinear();

        float x, z, y = xyz2Lab(((0.2225045f * linear.X) + (0.7168786f * linear.Y) + (0.0606169f * linear.Z)) / yn);

        if (linear.X == linear.Y && linear.Y == linear.Z)
        {
            x = z = y;
        }
        else
        {
            x = xyz2Lab(((0.4360747f * linear.X) + (0.3850649f * linear.Y) + (0.1430804f * linear.Z)) / xn);
            z = xyz2Lab(((0.0139322f * linear.X) + (0.0971045f * linear.Y) + (0.7141733f * linear.Z)) / zn);
        }

        return new LabColor(
            (float)((116f * y) - 16f),
            (float)(500f * (x - y)),
            (float)(200f * (y - z)),
            sRGBColor.A / 255f
        );
    }
}
