using System.Runtime.InteropServices;

namespace DanmakuEngine.Graphics.Colors;


[StructLayout(LayoutKind.Sequential)]
public struct HSLColor
{
    /// <summary>
    /// Hue in degrees, from 0 to 360;
    /// </summary>
    public float H;

    /// <summary>
    /// Saturation, from 0 to 1.
    /// </summary>
    public float S;

    /// <summary>
    /// Lightness, from 0 to 1.
    /// </summary>
    public float L;

    public static HSLColor FromHkSL(HkSLColor hksl)
        => new()
        {
            H = hksl.Hk * 360f,
            S = hksl.S,
            L = hksl.L
        };

    public static HkSLColor ToHkSL(HSLColor hsl)
        => new()
        {
            Hk = hsl.H / 360f,
            S = hsl.S,
            L = hsl.L
        };
}
