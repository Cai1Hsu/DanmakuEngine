// Copyright (c) ppy Pty Ltd <contact@ppy.sh>.

using DanmakuEngine.Extensions;
using OpenTK.Mathematics;

namespace DanmakuEngine.Graphics.Colors;

public struct SRGBColor : IEquatable<SRGBColor>
{
    /// <summary>
    /// A <see cref="Color4"/> representation of this Color in the sRGB space.
    /// </summary>
    public Color4 SRGB;

    /// <summary>
    /// A <see cref="Color4"/> representation of this Color in the linear space.
    /// </summary>
    public readonly Color4 Linear => SRGB.ToLinear();

    public static Color4 ToColor4(SRGBColor sRGBColor)
        => sRGBColor.SRGB;

    public readonly Color4 ToColor4()
        => ToColor4(this);

    public static SRGBColor FromColor4(Color4 value)
        => new() { SRGB = value };

    public SRGBColor(Color4 srgb)
    {
        SRGB = srgb;
    }

    public SRGBColor(float r, float g, float b, float a)
    {
        SRGB = new Color4(r, g, b, a);
    }

    public readonly float Alpha => SRGB.A;

    public static SRGBColor operator *(SRGBColor first, SRGBColor second)
    {
        var firstLinear = first.Linear;
        var secondLinear = second.Linear;

        return new SRGBColor
        {
            SRGB = new Color4(
                firstLinear.R * secondLinear.R,
                firstLinear.G * secondLinear.G,
                firstLinear.B * secondLinear.B,
                firstLinear.A * secondLinear.A).ToSRGB(),
        };
    }

    public static SRGBColor operator *(SRGBColor first, float second)
    {
        var firstLinear = first.Linear;

        return new SRGBColor
        {
            SRGB = new Color4(
                firstLinear.R * second,
                firstLinear.G * second,
                firstLinear.B * second,
                firstLinear.A * second).ToSRGB(),
        };
    }

    public static SRGBColor operator /(SRGBColor first, float second)
        => first * (1 / second);

    public static SRGBColor operator +(SRGBColor first, SRGBColor second)
    {
        var firstLinear = first.Linear;
        var secondLinear = second.Linear;

        return new SRGBColor
        {
            SRGB = new Color4(
                firstLinear.R + secondLinear.R,
                firstLinear.G + secondLinear.G,
                firstLinear.B + secondLinear.B,
                firstLinear.A + secondLinear.A).ToSRGB(),
        };
    }

    public readonly Vector4 ToVector()
        => new(SRGB.R, SRGB.G, SRGB.B, SRGB.A);

    public static SRGBColor FromVector(Vector4 v)
        => new() { SRGB = new Color4(v.X, v.Y, v.Z, v.W) };

    /// <summary>
    /// Multiplies the alpha value of this Color by the given alpha factor.
    /// </summary>
    /// <param name="alpha">The alpha factor to multiply with.</param>
    public void MultiplyAlpha(float alpha)
        => SRGB.A *= alpha;

    public readonly bool Equals(SRGBColor other)
        => SRGB.Equals(other.SRGB);
}
