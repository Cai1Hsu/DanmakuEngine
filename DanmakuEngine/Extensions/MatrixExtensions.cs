using Silk.NET.Maths;

namespace DanmakuEngine.Extensions;

public static class MatrixExtensions
{
    public static void TranslateFromLeft(this ref Matrix3X3<float> matrix, Vector2D<float> translation)
        => matrix.Row3 += (matrix.Row1 * translation.Y) + (matrix.Row2 * translation.X);

    public static void TranslateFromRight(this ref Matrix3X3<float> matrix, Vector2D<float> translation)
    {
        // matrix.Col1 += matrix.Column2 * translations.X;
        matrix.M11 += matrix.M13 * translation.X;
        matrix.M21 += matrix.M23 * translation.X;
        matrix.M31 += matrix.M33 * translation.X;

        // matrix.Col2 += matrix.Col3 * translations.Y;
        matrix.M12 += matrix.M13 * translation.Y;
        matrix.M22 += matrix.M23 * translation.Y;
        matrix.M32 += matrix.M33 * translation.Y;
    }

    public static void ScaleFromLeft(this ref Matrix3X3<float> matrix, Vector2D<float> scale)
    {
        matrix.Row1 *= scale.X;
        matrix.Row2 *= scale.Y;
    }

    public static void ScaleFromRight(this ref Matrix3X3<float> matrix, Vector2D<float> scale)
    {
        matrix.M11 *= scale.X;
        matrix.M12 *= scale.Y;

        matrix.M21 *= scale.X;
        matrix.M22 *= scale.Y;

        matrix.M31 *= scale.X;
        matrix.M32 *= scale.Y;
    }

    public static void RotateFromLeft(this ref Matrix3X3<float> matrix, float angle)
    {
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);

        var row1 = matrix.Row1;
        var row2 = matrix.Row2;

        matrix.Row1 = (row1 * cos) + (row2 * sin);
        matrix.Row2 = (row2 * cos) - (row1 * sin);
    }

    public static void RotateFromRight(this ref Matrix3X3<float> matrix, float angle)
    {
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);
        _ = matrix.Column1;
        _ = matrix.Column2;

        //m.Column1 = m.Column1 * cos - m.Column0 * sin;
        float m11 = (matrix.M11 * cos) - (matrix.M12 * sin);
        float m21 = (matrix.M21 * cos) - (matrix.M22 * sin);
        float m31 = (matrix.M31 * cos) - (matrix.M32 * sin);

        //m.Column1 = m.Column1 * cos - m.Column0 * sin;
        matrix.M12 = (matrix.M12 * cos) + (matrix.M11 * sin);
        matrix.M22 = (matrix.M22 * cos) + (matrix.M21 * sin);
        matrix.M32 = (matrix.M32 * cos) + (matrix.M31 * sin);

        //m.Column0 = row0;
        matrix.M11 = m11;
        matrix.M21 = m21;
        matrix.M31 = m31;
    }

    public static void ShearFromLeft(this ref Matrix3X3<float> matrix, Vector2D<float> shear)
    {
        Vector3D<float> row1 = matrix.Row1 + (matrix.Row2 * shear.Y) + (matrix.Row1 * shear.X * shear.Y);

        matrix.Row2 += matrix.Row1 * shear.X;
        matrix.Row1 = row1;
    }

    public static void ShearFromRight(this ref Matrix3X3<float> matrix, Vector2D<float> shear)
    {
        float xy = shear.X * shear.Y;

        //m.Column0 += m.Column1 * v.X;
        float m11 = matrix.M11 + (matrix.M12 * shear.X);
        float m21 = matrix.M21 + (matrix.M22 * shear.X);
        float m31 = matrix.M31 + (matrix.M32 * shear.X);

        //m.Column1 += m.Column0 * v.Y + m.Column1 * xy;
        matrix.M12 += (matrix.M11 * shear.Y) + (matrix.M12 * xy);
        matrix.M22 += (matrix.M21 * shear.Y) + (matrix.M22 * xy);
        matrix.M32 += (matrix.M31 * shear.Y) + (matrix.M32 * xy);

        matrix.M11 = m11;
        matrix.M21 = m21;
        matrix.M31 = m31;
    }
}
