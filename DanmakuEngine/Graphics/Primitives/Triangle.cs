using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DanmakuEngine.Extensions.Vector;
using Silk.NET.Maths;

namespace DanmakuEngine.Graphics.Primitives;

public readonly struct Triangle : IEquatable<Triangle>
{
    public readonly Vector2D<float> P1;

    public readonly Vector2D<float> P2;

    public readonly Vector2D<float> P3;

    public Triangle(Vector2D<float> p1, Vector2D<float> p2, Vector2D<float> p3)
    {
        P1 = p1;
        P2 = p2;
        P3 = p3;
    }

    public ReadOnlySpan<Vector2D<float>> GetAxisVertices()
        => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in P1), 3);

    public ReadOnlySpan<Vector2D<float>> GetVertices()
        => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in P1), 3);

    public bool Contains(Vector2D<float> p)
    {
        var v0 = P3 - P1;
        var v1 = P2 - P1;
        var v2 = p - P1;

        var dot00 = Vector2DExtensions.Dot(v0, v0);
        var dot01 = Vector2DExtensions.Dot(v0, v1);
        var dot02 = Vector2DExtensions.Dot(v0, v2);
        var dot11 = Vector2DExtensions.Dot(v1, v1);
        var dot12 = Vector2DExtensions.Dot(v1, v2);

        var invDenom = 1 / ((dot00 * dot11) - (dot01 * dot01));
        var u = ((dot11 * dot02) - (dot01 * dot12)) * invDenom;
        var v = ((dot00 * dot12) - (dot01 * dot02)) * invDenom;

        return (u >= 0) && (v >= 0) && (u + v < 1);
    }

    public RectangleF AABBFloat
    {
        get
        {
            float xMin = Math.Min(P1.X, Math.Min(P2.X, P3.X));
            float yMin = Math.Min(P1.Y, Math.Min(P2.Y, P3.Y));
            float xMax = Math.Max(P1.X, Math.Max(P2.X, P3.X));
            float yMax = Math.Max(P1.Y, Math.Max(P2.Y, P3.Y));

            return new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin);
        }
    }

    public float Area
        => 0.5f * Math.Abs(Vector2DExtensions.GetOrientation(GetVertices()));

    public bool Equals(Triangle other)
        => this.P1 == other.P1
        && this.P2 == other.P2
        && this.P3 == other.P3;
}
