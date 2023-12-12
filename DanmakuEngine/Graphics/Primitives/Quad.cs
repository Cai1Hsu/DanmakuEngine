// Copyright (c) ppy Pty Ltd <contact@ppy.sh>.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DanmakuEngine.Extensions.Vector;
using DanmakuEngine.Utils;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace DanmakuEngine.Graphics.Primitives;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Quad : IConvexPolygon, IEquatable<Quad>
{
    // Note: Do not change the order of vertices. They are ordered in screen-space counter-clockwise fashion.
    // See: IPolygon.GetVertices()
    public readonly Vector2D<float> TopLeft;
    public readonly Vector2D<float> BottomLeft;
    public readonly Vector2D<float> BottomRight;
    public readonly Vector2D<float> TopRight;

    public Quad(Vector2D<float> topLeft, Vector2D<float> topRight,
                Vector2D<float> bottomLeft, Vector2D<float> bottomRight)
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
    }

    public Quad(float x, float y, float width, float height)
        : this()
    {
        TopLeft = new Vector2D<float>(x, y);
        TopRight = new Vector2D<float>(x + width, y);
        BottomLeft = new Vector2D<float>(x, y + height);
        BottomRight = new Vector2D<float>(x + width, y + height);
    }

    public static implicit operator Quad(RectangleI r) => FromRectangle(r);
    public static implicit operator Quad(RectangleF r) => FromRectangle(r);

    public static Quad FromRectangle(RectangleI rectangle) =>
    new Quad(new Vector2D<float>(rectangle.Left, rectangle.Top),
        new Vector2D<float>(rectangle.Right, rectangle.Top),
        new Vector2D<float>(rectangle.Left, rectangle.Bottom),
        new Vector2D<float>(rectangle.Right, rectangle.Bottom));

    public static Quad FromRectangle(RectangleF rectangle) =>
        new Quad(new Vector2D<float>(rectangle.Left, rectangle.Top),
            new Vector2D<float>(rectangle.Right, rectangle.Top),
            new Vector2D<float>(rectangle.Left, rectangle.Bottom),
            new Vector2D<float>(rectangle.Right, rectangle.Bottom));

    public static Quad operator *(Quad r, Matrix3X3<float> m) =>
        new Quad(
            Vector2DExtensions.Transform(r.TopLeft, m),
            Vector2DExtensions.Transform(r.TopRight, m),
            Vector2DExtensions.Transform(r.BottomLeft, m),
            Vector2DExtensions.Transform(r.BottomRight, m));

    public Matrix2X2<float> BasisTransform
    {
        get
        {
            Vector2D<float> row0 = TopRight - TopLeft;
            Vector2D<float> row1 = BottomLeft - TopLeft;

            if (row0 != Vector2D<float>.Zero)
                row0 /= row0.LengthSquared;

            if (row1 != Vector2D<float>.Zero)
                row1 /= row1.LengthSquared;

            return new(
                row0.X, row0.Y,
                row1.X, row1.Y);
        }
    }

    public Vector2D<float> Center => (TopLeft + TopRight + BottomLeft + BottomRight) / 4;
    public Vector2D<float> Size => new Vector2D<float>(Width, Height);

    public float Width
        => Vector2DExtensions.Distance(TopLeft, TopRight);
    public float Height
        => Vector2DExtensions.Distance(TopLeft, BottomLeft);

    public RectangleI AABB
    {
        get
        {
            int xMin = (int)Math.Floor(Math.Min(TopLeft.X, Math.Min(TopRight.X, Math.Min(BottomLeft.X, BottomRight.X))));
            int yMin = (int)Math.Floor(Math.Min(TopLeft.Y, Math.Min(TopRight.Y, Math.Min(BottomLeft.Y, BottomRight.Y))));
            int xMax = (int)Math.Ceiling(Math.Max(TopLeft.X, Math.Max(TopRight.X, Math.Max(BottomLeft.X, BottomRight.X))));
            int yMax = (int)Math.Ceiling(Math.Max(TopLeft.Y, Math.Max(TopRight.Y, Math.Max(BottomLeft.Y, BottomRight.Y))));

            return new RectangleI(xMin, yMin, xMax - xMin, yMax - yMin);
        }
    }

    public RectangleF AABBFloat
    {
        get
        {
            float xMin = Math.Min(TopLeft.X, Math.Min(TopRight.X, Math.Min(BottomLeft.X, BottomRight.X)));
            float yMin = Math.Min(TopLeft.Y, Math.Min(TopRight.Y, Math.Min(BottomLeft.Y, BottomRight.Y)));
            float xMax = Math.Max(TopLeft.X, Math.Max(TopRight.X, Math.Max(BottomLeft.X, BottomRight.X)));
            float yMax = Math.Max(TopLeft.Y, Math.Max(TopRight.Y, Math.Max(BottomLeft.Y, BottomRight.Y)));

            return new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin);
        }
    }

    public ReadOnlySpan<Vector2D<float>> GetAxisVertices()
        => GetVertices();

    public ReadOnlySpan<Vector2D<float>> GetVertices()
        => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in TopLeft), 4);

    /// <summary>
    /// Checks whether <paramref name="pos"/> is inside of this quad.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method assumes a convex quad. The convexity of the quad is not checked.
    /// Vertices of the quad as returned by <see cref="GetVertices"/> must be arranged either in clockwise or counter-clockwise order.
    /// </para>
    /// <para>
    /// The method works by checking whether the point lies on the same side of all four sides of the quad.
    /// Note that the quad vertices are *not* using the standard Cartesian coordinates, but rather a Y-inverted version of them
    /// (as in higher Y is *down*),
    /// which is why the sign of the perpendicular dot product is opposite to what would be normally expected on the Cartesian plane.
    /// </para>
    /// </remarks>
    public bool Contains(Vector2D<float> pos)
    {
        if (Width == 0 && Height == 0)
            return pos == TopLeft;

        // to check if the point is inside the quad, we will calculate on which side of each quad segment the tested point is using the sign of the perp dot product.
        // note that the order in which we walk the segments matters - it must be clockwise or counterclockwise.
        // in theory quads should always be CCW (see note at top of class), but this is not necessarily true after applying negative horizontal or vertical scale.
        // if the signs all match (all positive or negative), then the point is inside of the quad.
        // a zero perp dot anywhere means that the point lies on one of the lines going through one of the quad sides, so zeroes are treated like positive values.

        // we test if two perp dots have matching signs by multiplying them and testing against 0.
        // if the result is positive, both perp dots were nonzero and had matching signs => don't reject point.
        // if the result is zero, one (or both) perp dots was zero, so the point may lie on the quad boundary => don't reject point.
        // if the result is negative, both perp dots were nonzero and had different signs => reject point.

        // note that we don't generally care about overflows there as long as the sign is right.
        // however, NaN values may come from Infinity - Infinity subtractions in `Vector2.PerpDot`.
        // there's not much good left to be done in such cases, so we err on the side of caution and reject points that generate any NaNs on sight.

        float perpDot1 = Vector2DExtensions.PerpDot(BottomLeft - TopLeft, pos - TopLeft);
        if (float.IsNaN(perpDot1))
            return false;

        float perpDot2 = Vector2DExtensions.PerpDot(BottomRight - BottomLeft, pos - BottomLeft);
        if (float.IsNaN(perpDot2) || perpDot1 * perpDot2 < 0)
            return false;

        float perpDot3 = Vector2DExtensions.PerpDot(TopRight - BottomRight, pos - BottomRight);
        if (float.IsNaN(perpDot3) || perpDot1 * perpDot3 < 0 || perpDot2 * perpDot3 < 0)
            return false;

        float perpDot4 = Vector2DExtensions.PerpDot(TopLeft - TopRight, pos - TopRight);
        if (float.IsNaN(perpDot4) || perpDot1 * perpDot4 < 0 || perpDot2 * perpDot4 < 0 || perpDot3 * perpDot4 < 0)
            return false;

        return true;
    }

    /// <summary>
    /// Computes the area of this <see cref="Quad"/>.
    /// </summary>
    /// <remarks>
    /// If the quad is self-intersecting the area is interpreted as the sum of all positive and negative areas and not the "visible area" enclosed by the <see cref="Quad"/>.
    /// </remarks>
    public float Area => 0.5f * Math.Abs(Vector2DExtensions.GetOrientation(GetVertices()));

    public bool Equals(Quad other) =>
        TopLeft == other.TopLeft &&
        TopRight == other.TopRight &&
        BottomLeft == other.BottomLeft &&
        BottomRight == other.BottomRight;

    public bool AlmostEquals(Quad other) =>
        Precision.AlmostEquals(TopLeft.X, other.TopLeft.X) &&
        Precision.AlmostEquals(TopLeft.Y, other.TopLeft.Y) &&
        Precision.AlmostEquals(TopRight.X, other.TopRight.X) &&
        Precision.AlmostEquals(TopRight.Y, other.TopRight.Y) &&
        Precision.AlmostEquals(BottomLeft.X, other.BottomLeft.X) &&
        Precision.AlmostEquals(BottomLeft.Y, other.BottomLeft.Y) &&
        Precision.AlmostEquals(BottomRight.X, other.BottomRight.X) &&
        Precision.AlmostEquals(BottomRight.Y, other.BottomRight.Y);

    public override string ToString() => $"{TopLeft} {TopRight} {BottomLeft} {BottomRight}";
}
