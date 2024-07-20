using System.Numerics;
using System.Runtime.CompilerServices;
using DanmakuEngine.Graphics.Colors;
using DanmakuEngine.Utils;

namespace DanmakuEngine.Transformation;

/// <summary>
/// A handle for lerping between two colors. Using HSL color space and Quaternion spherical linear interpolation.
/// </summary>
public class ColorLerpHandle : ILerpHandle<RGBAColor>
{
    private Quaternion _start;
    private Quaternion _end;

    private float _startAlpha;
    private float _endAlpha;

    public ColorLerpHandle(RGBAColor start, RGBAColor end)
    {
        _start = hslToQuaternion(HkSLColor.FromRGBA(start));
        _end = hslToQuaternion(HkSLColor.FromRGBA(end));

        _startAlpha = start.A;
        _endAlpha = end.A;
    }

    public RGBAColor Lerp(float t)
    {
        var lerped = Quaternion.Slerp(_start, _end, t);

        var hsl = quaternionToHSL(lerped);

        float a = MathUtils.Lerp(_startAlpha, _endAlpha, t);

        return hsl.ToRGBAColor(a / 255f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static Quaternion hslToQuaternion(HkSLColor hksl)
    {
        float radius = hksl.S;

        float halfHueRad = hksl.Hk * MathF.Tau / 2;
        float halfLightnessRad = hksl.L * MathF.PI / 2;

        float CosHalfHue = MathF.Cos(halfHueRad);
        float SinHalfHue = MathF.Sin(halfHueRad);

        float CosHalfLightness = MathF.Cos(halfLightnessRad);
        float SinHalfLightness = MathF.Sin(halfLightnessRad);

        return new Quaternion
        {
            W = CosHalfHue * CosHalfLightness,
            X = SinHalfHue * CosHalfLightness,
            Y = CosHalfHue * SinHalfLightness,
            Z = SinHalfHue * SinHalfLightness * radius,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static HkSLColor quaternionToHSL(Quaternion quaternion)
    {
        float radius = quaternion.Length();

        float lightnessRad = MathF.Atan2(quaternion.Y, quaternion.W) * 2;
        float hueRad = MathF.Atan2(quaternion.X, quaternion.W) * 2;

        var color = new HkSLColor
        {
            Hk = hueRad / MathF.Tau,
            S = radius,
            L = lightnessRad / MathF.PI,
        };

        if (color.Hk < 0)
            color.Hk += 1;

        return color;
    }
}
