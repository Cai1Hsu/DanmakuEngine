// Copyright (c) ppy Pty Ltd <contact@ppy.sh>.

using System.Numerics;
using System.Globalization;
using DanmakuEngine.Graphics.Colors;

namespace DanmakuEngine.Extensions;

public static class ColorExtensions
{
    public const double GAMMA = 2.4;

    // ToLinear is quite a hot path in the game.
    // MathF.Pow performs way faster than Math.Pow, however on Windows it lacks a fast path for x == 1.
    // Given passing color == 1 (White or Transparent) is very common, a fast path for 1 is added.

    public static double ToLinear(double color)
    {
        if (color == 1)
            return 1;

        return color <= 0.04045 ? color / 12.92 : Math.Pow((color + 0.055) / 1.055, GAMMA);
    }

    public static float ToLinear(float color)
    {
        if (color == 1)
            return 1;

        return color <= 0.04045f ? color / 12.92f : MathF.Pow((color + 0.055f) / 1.055f, (float)GAMMA);
    }

    public static double ToSRGB(double color)
    {
        if (color == 1)
            return 1;

        return color < 0.0031308 ? 12.92 * color : 1.055 * Math.Pow(color, 1.0 / GAMMA) - 0.055;
    }

    public static float ToSRGB(float color)
    {
        if (color == 1)
            return 1;

        return color < 0.0031308f ? 12.92f * color : 1.055f * MathF.Pow(color, 1.0f / (float)GAMMA) - 0.055f;
    }

    public static Vector4 ToLinearVector4(this SRGBColor Color) =>
        new Vector4(
            ToLinear(Color.R),
            ToLinear(Color.G),
            ToLinear(Color.B),
            Color.A);

    public static SRGBColor FromLinearVector4(Vector4 Color) =>
        new SRGBColor(
            ToSRGB(Color.X),
            ToSRGB(Color.Y),
            ToSRGB(Color.Z),
            Color.W);
}
