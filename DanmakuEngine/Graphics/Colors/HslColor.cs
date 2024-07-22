using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DanmakuEngine.Utils;
using Silk.NET.Maths;

namespace DanmakuEngine.Graphics.Colors;

[StructLayout(LayoutKind.Sequential)]
public struct HslColor
{
    /// <summary>
    /// Hue in degrees, from 0 to 360;
    /// </summary>
    public float H => Hk * 360;

    /// <summary>
    /// Hue in degrees, from 0 to 1;
    /// </summary>
    public float Hk;

    /// <summary>
    /// Saturation, from 0 to 1.
    /// </summary>
    public float S;

    /// <summary>
    /// Lightness, from 0 to 1.
    /// </summary>
    public float L;

    public static HslColor FromHSL(Vector3D<float> hsl)
        => new()
        {
            Hk = hsl.X / 360,
            S = hsl.Y,
            L = hsl.Z
        };

    public static HslColor FromHSL(float h, float s, float l)
        => new()
        {
            Hk = h / 360,
            S = s,
            L = l
        };

    public static HslColor FromRGB(Vector3D<int> rgb)
        => fromRGBInternal(new Vector3D<float>(rgb.X / 255f, rgb.Y / 255f, rgb.Z / 255f));

    public static HslColor FromRGB(Vector3D<float> rgb)
        => fromRGBInternal(rgb);

    public static HslColor FromRGBA(SRGBColor rgba)
        => fromRGBInternal(new Vector3D<float>(rgba.R / 255f, rgba.G / 255f, rgba.B / 255f));

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static HslColor fromRGBInternal(Vector3D<float> rgb)
    {
        var (min, max) = MathUtils.MinMax(rgb.X, rgb.Y, rgb.Z);
        float delta = max - min;

        float l = (min + max) / 2;

        float s = (l == 1 || l == 0) switch
        {
            true => 0,
            _ => delta / (1 - MathF.Abs((2 * l) - 1)),
        };

        float h = delta switch
        {
            0 => 0,
            _ when max == rgb.X => (((rgb.Y - rgb.Z) / delta) % 6) / 6,
            _ when max == rgb.Y => (((rgb.Z - rgb.X) / delta) + 2) / 6,
            _ => (((rgb.X - rgb.Y) / delta) + 4) / 6,
        };

        if (h < 0)
            h += 1;

        return new HslColor
        {
            Hk = h,
            S = s,
            L = l
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public SRGBColor ToRGBAColor(float alpha = 1.00f)
        => toRGBAColorInternal(alpha);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal SRGBColor toRGBAColorInternal(float alpha = 1.00f)
    {
        float c = (1 - MathF.Abs((2 * L) - 1)) * S;
        float x = c * (1 - MathF.Abs(((Hk * 6) % 2) - 1));
        float m = L - (c / 2);

        float r, g, b;

        if (Hk < 1 / 6f)
            (r, g, b) = (c, x, 0);
        else if (Hk < 2 / 6f)
            (r, g, b) = (x, c, 0);
        else if (Hk < 3 / 6f)
            (r, g, b) = (0, c, x);
        else if (Hk < 4 / 6f)
            (r, g, b) = (0, x, c);
        else if (Hk < 5 / 6f)
            (r, g, b) = (x, 0, c);
        else
            (r, g, b) = (c, 0, x);

        return new SRGBColor
        {
            R = (r + m) * 255,
            G = (g + m) * 255,
            B = (b + m) * 255,
            A = alpha * 255,
        };
    }
}
