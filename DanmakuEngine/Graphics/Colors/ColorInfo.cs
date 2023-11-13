// Copyright (c) ppy Pty Ltd <contact@ppy.sh>.

using DanmakuEngine.Graphics.Primitives;
using System.Diagnostics;
using OpenTK.Mathematics;
using Silk.NET.Maths;

namespace DanmakuEngine.Graphics.Colors;

/// <summary>
/// ColorInfo contains information about the Colors of all 4 vertices of a quad.
/// These Colors are always stored in linear space.
/// </summary>
public struct ColorInfo : IEquatable<ColorInfo>, IEquatable<SRGBColor>
{
    public SRGBColor TopLeft;
    public SRGBColor BottomLeft;
    public SRGBColor TopRight;
    public SRGBColor BottomRight;
    public bool HasSingleColor;

    /// <summary>
    /// Creates a ColorInfo with a single linear Color assigned to all vertices.
    /// </summary>
    /// <param name="Color">The single linear Color to be assigned to all vertices.</param>
    /// <returns>The created ColorInfo.</returns>
    public static ColorInfo SingleColor(SRGBColor Color)
    {
        ColorInfo result = new ColorInfo();
        result.TopLeft = result.BottomLeft = result.TopRight = result.BottomRight = Color;
        result.HasSingleColor = true;
        return result;
    }

    /// <summary>
    /// Creates a ColorInfo with a horizontal gradient.
    /// </summary>
    /// <param name="c1">The left Color of the gradient.</param>
    /// <param name="c2">The right Color of the gradient.</param>
    /// <returns>The created ColorInfo.</returns>
    public static ColorInfo GradientHorizontal(SRGBColor c1, SRGBColor c2)
    {
        ColorInfo result = new ColorInfo();
        result.TopLeft = result.BottomLeft = c1;
        result.TopRight = result.BottomRight = c2;
        result.HasSingleColor = false;
        return result;
    }

    /// <summary>
    /// Creates a ColorInfo with a vertical gradient.
    /// </summary>
    /// <param name="c1">The top Color of the gradient.</param>
    /// <param name="c2">The bottom Color of the gradient.</param>
    /// <returns>The created ColorInfo.</returns>
    public static ColorInfo GradientVertical(SRGBColor c1, SRGBColor c2)
    {
        ColorInfo result = new ColorInfo();
        result.TopLeft = result.TopRight = c1;
        result.BottomLeft = result.BottomRight = c2;
        result.HasSingleColor = false;
        return result;
    }

    private SRGBColor singleColor
    {
        readonly get
        {
            Debug.Assert(HasSingleColor);
            return TopLeft;
        }
        set
        {
            TopLeft = BottomLeft = TopRight = BottomRight = value;
            HasSingleColor = true;
        }
    }

    /// <summary>
    /// Attempts to extract the single Color represented by this <see cref="ColorInfo"/>.
    /// </summary>
    /// <param name="Color">The extracted Color. If <c>false</c> is returned, this represents the top-left Color.</param>
    /// <returns>Whether the extracted Color is the single Color represented by this <see cref="ColorInfo"/>.</returns>
    public readonly bool TryExtractSingleColor(out SRGBColor Color)
    {
        // To make this code branchless, we have to work around the assertion in singleColor.
        Color = TopLeft;
        return HasSingleColor;
    }

    public readonly SRGBColor Interpolate(Vector2D<float> interp) => SRGBColor.FromVector(
        (1 - interp.Y) * ((1 - interp.X) * TopLeft.ToVector() + interp.X * TopRight.ToVector()) +
        interp.Y * ((1 - interp.X) * BottomLeft.ToVector() + interp.X * BottomRight.ToVector()));

    /// <summary>
    /// Interpolates this <see cref="ColorInfo"/> across a quad.
    /// </summary>
    /// <remarks>
    /// This method is especially useful when working with multi-Color <see cref="ColorInfo"/>s.
    /// When such a Color is interpolated across a quad that is a subset of the unit quad (0, 0, 1, 1),
    /// the resulting Color can be thought of as the the original Color but "cropped" to the bounds of the subquad.
    /// </remarks>
    public readonly ColorInfo Interpolate(Quad quad)
    {
        if (HasSingleColor)
            return this;

        return new ColorInfo
        {
            TopLeft = Interpolate(quad.TopLeft),
            TopRight = Interpolate(quad.TopRight),
            BottomLeft = Interpolate(quad.BottomLeft),
            BottomRight = Interpolate(quad.BottomRight),
            HasSingleColor = false
        };
    }

    public void ApplyChild(ColorInfo childColor)
    {
        if (!HasSingleColor)
        {
            ApplyChild(childColor, new Quad(0, 0, 1, 1));
            return;
        }

        if (childColor.HasSingleColor)
            singleColor *= childColor.singleColor;
        else
        {
            HasSingleColor = false;
            BottomLeft = childColor.BottomLeft * TopLeft;
            TopRight = childColor.TopRight * TopLeft;
            BottomRight = childColor.BottomRight * TopLeft;

            // Need to assign TopLeft last to keep correctness.
            TopLeft = childColor.TopLeft * TopLeft;
        }
    }

    public void ApplyChild(ColorInfo childColor, Quad interp)
    {
        Trace.Assert(!HasSingleColor);

        SRGBColor newTopLeft = Interpolate(interp.TopLeft) * childColor.TopLeft;
        SRGBColor newTopRight = Interpolate(interp.TopRight) * childColor.TopRight;
        SRGBColor newBottomLeft = Interpolate(interp.BottomLeft) * childColor.BottomLeft;
        SRGBColor newBottomRight = Interpolate(interp.BottomRight) * childColor.BottomRight;

        TopLeft = newTopLeft;
        TopRight = newTopRight;
        BottomLeft = newBottomLeft;
        BottomRight = newBottomRight;
    }

    internal static ColorInfo Multiply(ColorInfo first, ColorInfo second) => new ColorInfo
    {
        TopLeft = first.TopLeft * second.TopLeft,
        BottomLeft = first.BottomLeft * second.BottomLeft,
        TopRight = first.TopRight * second.TopRight,
        BottomRight = first.BottomRight * second.BottomRight
    };

    /// <summary>
    /// Created a new ColorInfo with the alpha value of the Colors of all vertices
    /// multiplied by a given alpha parameter.
    /// </summary>
    /// <param name="alpha">The alpha parameter to multiply the alpha values of all vertices with.</param>
    /// <returns>The new ColorInfo.</returns>
    public readonly ColorInfo MultiplyAlpha(float alpha)
    {
        if (alpha == 1.0)
            return this;

        if (TryExtractSingleColor(out SRGBColor single))
        {
            single.MultiplyAlpha(alpha);

            return FromSRGBColor(single);
        }

        ColorInfo result = this;
        result.TopLeft.MultiplyAlpha(alpha);
        result.BottomLeft.MultiplyAlpha(alpha);
        result.TopRight.MultiplyAlpha(alpha);
        result.BottomRight.MultiplyAlpha(alpha);

        return result;
    }

    public readonly bool Equals(ColorInfo other)
    {
        if (!HasSingleColor)
        {
            if (other.HasSingleColor)
                return false;

            return
                TopLeft.Equals(other.TopLeft) &&
                TopRight.Equals(other.TopRight) &&
                BottomLeft.Equals(other.BottomLeft) &&
                BottomRight.Equals(other.BottomRight);
        }

        return other.HasSingleColor && singleColor.Equals(other.singleColor);
    }

    public readonly bool Equals(SRGBColor other) => HasSingleColor && singleColor.Equals(other);

    /// <summary>
    /// The average Color of all corners.
    /// </summary>
    public readonly SRGBColor AverageColor
    {
        get
        {
            if (HasSingleColor)
                return singleColor;

            return SRGBColor.FromVector(
                (TopLeft.ToVector() + TopRight.ToVector() + BottomLeft.ToVector() + BottomRight.ToVector()) / 4);
        }
    }

    /// <summary>
    /// The maximum alpha value of all four corners.
    /// </summary>
    public readonly float MaxAlpha
    {
        get
        {
            float max = TopLeft.Alpha;
            if (TopRight.Alpha > max) max = TopRight.Alpha;
            if (BottomLeft.Alpha > max) max = BottomLeft.Alpha;
            if (BottomRight.Alpha > max) max = BottomRight.Alpha;

            return max;
        }
    }

    /// <summary>
    /// The minimum alpha value of all four corners.
    /// </summary>
    public readonly float MinAlpha
    {
        get
        {
            float min = TopLeft.Alpha;
            if (TopRight.Alpha < min) min = TopRight.Alpha;
            if (BottomLeft.Alpha < min) min = BottomLeft.Alpha;
            if (BottomRight.Alpha < min) min = BottomRight.Alpha;

            return min;
        }
    }

    public override readonly string ToString() => HasSingleColor ? $@"{TopLeft} (Single)" : $@"{TopLeft}, {TopRight}, {BottomLeft}, {BottomRight}";

    public static ColorInfo FromSRGBColor(SRGBColor Color)
        => SingleColor(Color);

    public SRGBColor ToSRGBColor()
        => ToSRGBColor(this);

    public static SRGBColor ToSRGBColor(ColorInfo Color)
    {
        if (!Color.HasSingleColor)
            throw new InvalidOperationException("Attempted to read single Color from multi-Color ColorInfo.");

        return Color.singleColor;
    }

    public static ColorInfo FromColor4(Color4 Color)
        => FromSRGBColor(SRGBColor.FromColor4(Color));

    public static Color4 ToColor4(ColorInfo Color)
        => ToSRGBColor(Color).ToColor4();
}