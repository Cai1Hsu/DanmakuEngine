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

    private const double xn = 0.96422;
    private const double yn = 1f;
    private const double zn = 0.82521;
    private const double t0 = 4 / 29;
    private const double t1 = 6 / 29;
    private const double t2 = 3 * t1 * t1;
    private const double t3 = t1 * t1 * t1;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static double xyz2Lab(double t)
        => t > t3 ? Math.Pow(t, 1 / 3) : t / t2 + t0;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static double lab2Xyz(double t)
        => t > t1 ? t * t * t : t2 * (t - t0);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static SRGBColor toSRGBInternal(LabColor lab)
    {
        float y = (lab.L + 16) / 116,
            x = float.IsNaN(lab.A) ? y : y + lab.A / 500,
            z = float.IsNaN(lab.B) ? y : y - lab.B / 200;

        double newX = lab2Xyz(x) * xn;
        double newY = lab2Xyz(y) * yn;
        double newZ = lab2Xyz(z) * zn;

        return new SRGBColor(
            (float)ColorExtensions.ToSRGB(3.1338561 * newX - 1.6168667 * newY - 0.4906146 * newZ),
            (float)ColorExtensions.ToSRGB(-0.9787684 * newX + 1.9161415 * newY + 0.0334540 * newZ),
            (float)ColorExtensions.ToSRGB(0.0719453 * newX - 0.2289914 * newY + 1.4052427 * newZ),
            lab.Opacity * 255f
        );
    }

    public SRGBColor ToSRGB()
        => toSRGBInternal(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static LabColor FromSRGB(SRGBColor sRGBColor)
    {
        var linear = sRGBColor.ToLinear();

        double x, z, y = xyz2Lab((0.2225045 * linear.X + 0.7168786 * linear.Y + 0.0606169 * linear.Z) / yn);

        if (linear.X == linear.Y && linear.Y == linear.Z)
        {
            x = z = y;
        }
        else
        {
            x = xyz2Lab((0.4360747 * linear.X + 0.3850649 * linear.Y + 0.1430804 * linear.Z) / xn);
            z = xyz2Lab((0.0139322 * linear.X + 0.0971045 * linear.Y + 0.7141733 * linear.Z) / zn);
        }

        return new LabColor(
            (float)(116 * y - 16),
            (float)(500 * (x - y)),
            (float)(200 * (y - z)),
            sRGBColor.A / 255f
        );
    }
}
