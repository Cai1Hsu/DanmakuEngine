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
    public static T Lerp<T>(T a, T b, float t)
    {
        if (typeof(T) == typeof(float))
        {
            return (T)(object)((float)(object)a! + ((float)(object)b! - (float)(object)a) * t);
        }

        if (typeof(T) == typeof(double))
        {
            return (T)(object)((double)(object)a! + ((double)(object)b! - (double)(object)a) * t);
        }

        if (typeof(T) == typeof(Half))
        {
            return (T)(object)((Half)(object)a! + ((Half)(object)b! - (Half)(object)a) * (Half)t);
        }

        throw new NotSupportedException();
    }
}
