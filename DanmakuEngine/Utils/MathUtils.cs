using System.Runtime.CompilerServices;

namespace DanmakuEngine.Utils;

public static class MathUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static (float min, float max) MinMax(float a, float b)
    {
        if (a < b)
        {
            return (a, b);
        }

        return (b, a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static (float min, float max) MinMax(float a, float b, float c)
    {
        if (a < b)
        {
            if (c < a)
                return (c, b);

            if (c > b)
                return (a, c);

            return (a, b);
        }

        if (c < b)
            return (c, a);

        if (c > a)
            return (b, c);

        return (b, a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Lerp(float a, float b, float t)
    {
        return a + ((b - a) * t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static double Lerp(double a, double b, float t)
    {
        return a + ((b - a) * t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Half Lerp(Half a, Half b, float t)
    {
        return a + ((b - a) * (Half)t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Lerp(float a, float b, double t)
    {
        return a + ((b - a) * (float)t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static double Lerp(double a, double b, double t)
    {
        return a + ((b - a) * (float)t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Half Lerp(Half a, Half b, double t)
    {
        return a + ((b - a) * (Half)(float)t);
    }
}
