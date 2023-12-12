using System.Diagnostics.CodeAnalysis;
using DanmakuEngine.Extensions;
using DanmakuEngine.Utils;
using Silk.NET.Maths;

namespace DanmakuEngine.Graphics;

public struct DrawInfo : IEquatable<DrawInfo>
{
    public Matrix3X3<float> Matrix;
    public Matrix3X3<float> MatrixInverse;

    public DrawInfo(Matrix3X3<float> matrix, Matrix3X3<float> matrixInverse)
    {
        Matrix = matrix;
        MatrixInverse = matrixInverse;
    }

    private void _translate(Vector2D<float> translation)
    {
        MatrixExtensions.TranslateFromLeft(ref Matrix, translation);
        MatrixExtensions.TranslateFromRight(ref MatrixInverse, -translation);
    }

    private void _rotate(float angle)
    {
        float radians = angle * MathF.PI / 180f;

        MatrixExtensions.RotateFromLeft(ref Matrix, radians);
        MatrixExtensions.RotateFromRight(ref MatrixInverse, -radians);
    }

    private void _shear(Vector2D<float> shear)
    {
        MatrixExtensions.ShearFromLeft(ref Matrix, shear);
        MatrixExtensions.ShearFromRight(ref MatrixInverse, -shear);
    }

    private void _scale(Vector2D<float> scale)
    {
        if (scale.X == 0) scale.X = Precision.FLOAT_EPSILON;
        if (scale.Y == 0) scale.Y = Precision.FLOAT_EPSILON;

        MatrixExtensions.ScaleFromLeft(ref Matrix, scale);
        MatrixExtensions.ScaleFromRight(ref MatrixInverse, Vector2D.Divide(Vector2D<float>.One, scale));
    }

    public void Transform(Vector2D<float> translation, Vector2D<float> scale, float rotation, Vector2D<float> shear, Vector2D<float> origin)
    {
        if (translation != Vector2D<float>.Zero)
            _translate(translation);

        if (rotation != 0)
            _rotate(rotation);

        if (shear != Vector2D<float>.Zero)
            _shear(shear);

        if (scale != Vector2D<float>.One)
            _scale(scale);

        if (origin.X != 0 || origin.Y != 0)
            _translate(origin);
    }

    public static DrawInfo Identity
        => new(Matrix3X3<float>.Identity, Matrix3X3<float>.Identity);

    public static DrawInfo operator *(DrawInfo a, DrawInfo b)
        => new(a.Matrix * b.Matrix, b.MatrixInverse * a.MatrixInverse);

    public static DrawInfo operator *(DrawInfo a, Matrix3X3<float> b)
        => new(a.Matrix * b, b * a.MatrixInverse);

    public static DrawInfo operator *(Matrix3X3<float> a, DrawInfo b)
        => new(a * b.Matrix, b.MatrixInverse * a);

    public static DrawInfo operator /(DrawInfo a, DrawInfo b)
        => new(a.Matrix * b.MatrixInverse, b.Matrix * a.MatrixInverse);

    public static DrawInfo operator /(DrawInfo a, Matrix3X3<float> b)
        => new(a.Matrix * b, b * a.MatrixInverse);

    public static DrawInfo operator /(Matrix3X3<float> a, DrawInfo b)
        => new(a * b.MatrixInverse, b.Matrix * a);

    public static DrawInfo operator +(DrawInfo a, DrawInfo b)
        => new(a.Matrix + b.Matrix, a.MatrixInverse + b.MatrixInverse);

    public static DrawInfo operator +(DrawInfo a, Matrix3X3<float> b)
        => new(a.Matrix + b, a.MatrixInverse + b);

    public static DrawInfo operator +(Matrix3X3<float> a, DrawInfo b)
        => new(a + b.Matrix, a + b.MatrixInverse);

    public static DrawInfo operator -(DrawInfo a, DrawInfo b)
        => new(a.Matrix - b.Matrix, a.MatrixInverse - b.MatrixInverse);

    public static DrawInfo operator -(DrawInfo a, Matrix3X3<float> b)
        => new(a.Matrix - b, a.MatrixInverse - b);

    public static DrawInfo operator -(Matrix3X3<float> a, DrawInfo b)
        => new(a - b.Matrix, a - b.MatrixInverse);

    public static DrawInfo operator -(DrawInfo a)
        => new(-a.Matrix, -a.MatrixInverse);

    public static bool operator ==(DrawInfo a, DrawInfo b)
        => a.Equals(b);

    public static bool operator !=(DrawInfo a, DrawInfo b)
        => !a.Equals(b);

    public bool Equals(DrawInfo other)
        => this.Matrix == other.Matrix;

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj != null && obj is DrawInfo info && Equals(info);

    public override int GetHashCode()
        => HashCode.Combine(Matrix);
}
