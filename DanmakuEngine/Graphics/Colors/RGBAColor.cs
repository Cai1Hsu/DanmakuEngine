using System.Numerics;
using Silk.NET.Maths;

namespace DanmakuEngine.Graphics.Colors;

/// <summary>
/// A struct representing a color in the RGBA color space.
/// </summary>
public struct RGBAColor
{
    /// <summary>
    /// Red component of the color, from 0 to 1.
    /// </summary>
    public float R;

    /// <summary>
    /// Green component of the color, from 0 to 1.
    /// </summary>
    public float G;

    /// <summary>
    /// Blue component of the color, from 0 to 1.
    /// </summary>
    public float B;

    /// <summary>
    /// Alpha component of the color, from 0 to 1.
    /// </summary>
    public float A;

    public static RGBAColor FromRGBA(byte r, byte g, byte b, byte a)
    {
        return new RGBAColor
        {
            R = r,
            G = g,
            B = b,
            A = a
        };
    }

    public static RGBAColor FromRGBA(float r, float g, float b, float a)
    {
        return new RGBAColor
        {
            R = r,
            G = g,
            B = b,
            A = a
        };
    }

    public static RGBAColor FromVector4(Vector4D<float> color)
    {
        return new RGBAColor
        {
            R = color.X,
            G = color.Y,
            B = color.Z,
            A = color.W
        };
    }

    public static RGBAColor FromVector4(Vector4 color)
    {
        return new RGBAColor
        {
            R = color.X,
            G = color.Y,
            B = color.Z,
            A = color.W
        };
    }
}
