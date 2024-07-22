using System.Numerics;
using System.Runtime.InteropServices;
using DanmakuEngine.Extensions;
using Silk.NET.Maths;

namespace DanmakuEngine.Graphics.Colors;

[StructLayout(LayoutKind.Sequential)]
public struct SRGBColor : IEquatable<SRGBColor>
{
    public float R;
    public float G;
    public float B;
    public float A;

    public readonly Vector4 Linear => ToLinear();

    public static unsafe Vector4 ToVector4(SRGBColor sRGBColor)
        => *(Vector4*)&sRGBColor;

    public static unsafe Vector4D<float> ToVector4D(SRGBColor sRGBColor)
        => *(Vector4D<float>*)&sRGBColor;

    public readonly Vector4 ToColor4()
        => ToVector4(this);

    public static unsafe SRGBColor FromVector4(Vector4 value)
        => *(SRGBColor*)&value;

    public SRGBColor(Vector4 srgb)
    {
        R = srgb.X;
        G = srgb.Y;
        B = srgb.Z;
        A = srgb.W;
    }

    public SRGBColor(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public static SRGBColor FromLinear(Vector4 linear)
        => ColorExtensions.FromLinearVector4(linear);

    public readonly float Alpha => A;

    public static SRGBColor operator *(SRGBColor first, SRGBColor second)
    {
        var firstLinear = first.Linear;
        var secondLinear = second.Linear;

        return ColorExtensions.FromLinearVector4(new Vector4(
            firstLinear.X * secondLinear.X,
            firstLinear.Y * secondLinear.Y,
            firstLinear.Z * secondLinear.Z,
            firstLinear.W * secondLinear.W
        ));
    }

    public static SRGBColor operator *(SRGBColor first, float second)
    {
        var firstLinear = first.Linear;

        return ColorExtensions.FromLinearVector4(new Vector4(
            firstLinear.X * second,
            firstLinear.Y * second,
            firstLinear.Z * second,
            firstLinear.W * second
        ));
    }

    public static SRGBColor operator /(SRGBColor first, float second)
        => first * (1 / second);

    public static SRGBColor operator +(SRGBColor first, SRGBColor second)
    {
        var firstLinear = first.Linear;
        var secondLinear = second.Linear;

        return ColorExtensions.FromLinearVector4(new Vector4(
            firstLinear.X + secondLinear.X,
            firstLinear.Y + secondLinear.Y,
            firstLinear.Z + secondLinear.Z,
            firstLinear.W + secondLinear.W
        ));
    }

    public static unsafe SRGBColor FromVector(Vector4 v)
        => *(SRGBColor*)&v;

    public static SRGBColor FromARGB(byte a, byte r, byte g, byte b)
        => new SRGBColor(r, g, b, a);

    public static unsafe SRGBColor FromFloatRGB(Vector4 aRGBColor)
        => FromVector(aRGBColor * 255f);

    public static unsafe Vector4 ToFloatRGB(SRGBColor sRGBColor)
        => ToVector4(sRGBColor) / 255f;

    public readonly Vector4 ToFloatRGB()
        => ToFloatRGB(this);

    public static Vector4 ToLinear(SRGBColor sRGBColor)
        => ColorExtensions.ToLinearVector4(sRGBColor);

    public readonly Vector4 ToLinear()
        => ToLinear(this);

    public readonly Vector3 ToLinearRGB()
        => new Vector3(
            ColorExtensions.ToLinear(R / 255f),
            ColorExtensions.ToLinear(G / 255f),
            ColorExtensions.ToLinear(B / 255f)
        );

    /// <summary>
    /// Multiplies the alpha value of this Color by the given alpha factor.
    /// </summary>
    /// <param name="alpha">The alpha factor to multiply with.</param>
    public void MultiplyAlpha(float alpha)
        => A *= alpha;

    public readonly bool Equals(SRGBColor other)
        => R == other.R && G == other.G && B == other.B && A == other.A;
}
