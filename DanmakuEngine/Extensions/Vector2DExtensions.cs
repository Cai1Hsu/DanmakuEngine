using System.Runtime.CompilerServices;
using Silk.NET.Maths;

namespace DanmakuEngine.Extensions.Vector;

public static class Vector2DExtensions
{
    public static float PerpDot(Vector2D<float> left, Vector2D<float> right)
        => (float)((left.X * right.Y) - (left.Y * right.X));

    public static float Dot(Vector2D<float> left, Vector2D<float> right)
        => (float)((left.X * right.X) + (left.Y * right.Y));

    public static float Dot(this Vector2D<float> left, Vector2D<int> right)
        => (float)((left.X * right.X) + (left.Y * right.Y));

    public static float Distance(this Vector2D<float> left, Vector2D<float> right)
        => (float)Math.Sqrt(((left.X - right.X) * (left.X - right.X)) +
                             ((left.Y - right.Y) * (left.Y - right.Y)));

    public static float Distance(Vector2D<float> left, Vector2D<int> right)
        => (float)Math.Sqrt(((left.X - right.X) * (left.X - right.X)) +
                             ((left.Y - right.Y) * (left.Y - right.Y)));

    public static float DistanceSquared(this Vector2D<float> left, Vector2D<float> right)
        => (float)(((left.X - right.X) * (left.X - right.X)) +
                    ((left.Y - right.Y) * (left.Y - right.Y)));

    public static float DistanceSquared(Vector2D<float> left, Vector2D<int> right)
        => (float)(((left.X - right.X) * (left.X - right.X)) +
                    ((left.Y - right.Y) * (left.Y - right.Y)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetOrientation(in ReadOnlySpan<Vector2D<float>> vertices)
    {
        if (vertices.Length == 0)
            return 0;

        float rotation = 0;
        for (int i = 0; i < vertices.Length - 1; ++i)
            rotation += (vertices[i + 1].X - vertices[i].X) * (vertices[i + 1].Y + vertices[i].Y);

        rotation += (vertices[0].X - vertices[^1].X) * (vertices[0].Y + vertices[^1].Y);

        return rotation;
    }

    public static float Transform(Vector2D<float> vector, Matrix3X3<float> matrix)
        => (float)((vector.X * matrix.M11) + (vector.Y * matrix.M21) + matrix.M31);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static Vector2D<float> ToNormalized(this Vector2D<float> vector)
    {
        if (vector.X is 0 && vector.Y is 0)
            return vector;

        var lengthSquared = vector.LengthSquared;

        var length = Scalar.Sqrt(lengthSquared);

        return new Vector2D<float>(vector.X / length, vector.Y / length);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static Vector2D<double> ToNormalized(this Vector2D<double> vector)
    {
        if (vector.X is 0 && vector.Y is 0)
            return vector;

        var lengthSquared = vector.LengthSquared;

        var length = Scalar.Sqrt(lengthSquared);

        return new Vector2D<double>(vector.X / length, vector.Y / length);
    }
}
