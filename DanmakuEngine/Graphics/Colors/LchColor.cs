using System.Runtime.CompilerServices;

namespace DanmakuEngine.Graphics.Colors;

public struct LchColor
{
    /// <summary>
    /// Hue
    /// </summary>
    public float H;

    /// <summary>
    /// Chroma
    /// </summary>
    public float C;

    /// <summary>
    /// Lightness
    /// </summary>
    public float L;

    /// <summary>
    /// Opacity, from 0 to 1
    /// </summary>
    public float Opacity;

    public LchColor(float h, float c, float l, float opacity = 1)
    {
        H = h;
        C = c;
        L = l;
        Opacity = opacity;
    }

    public static LchColor FromLab(LabColor lab)
        => fromLabInternal(lab);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static LchColor fromLabInternal(LabColor lab)
    {
        if (lab.A == 0 && lab.B == 0)
            return new LchColor
            {
                H = float.NaN,
                C = lab.L > 0 && lab.L < 100 ? 0 : float.NaN,
                L = lab.L,
                Opacity = lab.Opacity,
            };

        float h = MathF.Atan2(lab.B, lab.A) * 180 / MathF.PI;
        return new LchColor
        {
            H = h < 0 ? h + 360 : h,
            C = MathF.Sqrt((lab.A * lab.A) + (lab.B * lab.B)),
            L = lab.L,
            Opacity = lab.Opacity,
        };
    }

    public static LabColor ToLab(LchColor lch)
        => toLabInternal(lch);

    public LabColor ToLab()
        => toLabInternal(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static LabColor toLabInternal(LchColor lch)
    {
        float h = lch.H * MathF.PI / 180;
        return new LabColor(lch.L, lch.C * MathF.Cos(h), lch.C * MathF.Sin(h), lch.Opacity);
    }
}
