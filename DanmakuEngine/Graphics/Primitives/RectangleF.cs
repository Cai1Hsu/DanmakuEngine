using System.Numerics;
using Silk.NET.Maths;

namespace DanmakuEngine.Graphics.Primitives;

public struct RectangleF : IEquatable<RectangleF>
{
    public float X;
    public float Y;

    public float Width;
    public float Height;

    public RectangleF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public RectangleF(Vector2D<float> p1, Vector2D<float> p2)
    {
        if (p1.X < p2.X)
        {
            X = p1.X;
            Width = p2.X - p1.X;
        }
        else
        {
            X = p2.X;
            Width = p1.X - p2.X;
        }

        if (p1.Y < p2.Y)
        {
            Y = p1.Y;
            Height = p2.Y - p1.Y;
        }
        else
        {
            Y = p2.Y;
            Height = p1.Y - p2.Y;
        }
    }

    public Vector2D<float> Location
    {
        get => new(X, Y);
        set => (X, Y) = (value.X, value.Y);
    }

    public Vector2D<float> Size
    {
        get => new(Width, Height);
        set => (Width, Height) = (value.X, value.Y);
    }

    public float Area
        => Width * Height;

    public float Left
    {
        get => X;
        set => X = value;
    }

    public float Right
    {
        get => X + Width;
        set => Width = value - X;
    }

    public float Top
    {
        get => Y;
        set => Y = value;
    }

    public float Bottom
    {
        get => Y + Height;
        set => Height = value - Y;
    }

    public Vector2D<float> TopLeft
    {
        get => new(Left, Top);
        set => (Left, Top) = (value.X, value.Y);
    }

    public Vector2D<float> TopRight
    {
        get => new(Right, Top);
        set => (Right, Top) = (value.X, value.Y);
    }

    public Vector2D<float> BottomLeft
    {
        get => new(Left, Bottom);
        set => (Left, Bottom) = (value.X, value.Y);
    }

    public Vector2D<float> BottomRight
    {
        get => new(Right, Bottom);
        set => (Right, Bottom) = (value.X, value.Y);
    }

    public readonly Vector2D<float> Center
        => new(X + (Width / 2), Y + (Height / 2));

    public bool IsEmpty => Width <= 0 || Height <= 0;

    public static bool operator ==(RectangleF left, RectangleF right)
        => left.X == right.X && left.Y == right.Y
            && left.Width == right.Width && left.Height == right.Height;

    public static bool operator !=(RectangleF left, RectangleF right)
        => !(left == right);

    public static RectangleF operator *(RectangleF rectangle, float scale)
        => new RectangleF(rectangle.X * scale, rectangle.Y * scale,
                rectangle.Width * scale, rectangle.Height * scale);

    public static RectangleF operator /(RectangleF rectangle, float scale)
        => new RectangleF(rectangle.X / scale, rectangle.Y / scale,
                rectangle.Width / scale, rectangle.Height / scale);

    public static RectangleF operator *(RectangleF rectangle, Vector2D<float> scale)
        => new RectangleF(rectangle.X * scale.X, rectangle.Y * scale.Y,
                rectangle.Width * scale.X, rectangle.Height * scale.Y);

    public static RectangleF operator /(RectangleF rectangle, Vector2D<float> scale)
        => new RectangleF(rectangle.X / scale.X, rectangle.Y / scale.Y,
                rectangle.Width / scale.X, rectangle.Height / scale.Y);

    public bool Equals(RectangleF other)
        => this == other;

    public bool Contains(float x, float y)
        => x >= Left && x <= Right
            && y >= Top && y <= Bottom;

    public bool Contains(Vector2D<float> point)
        => point.X >= Left && point.X <= Right
            && point.Y >= Top && point.Y <= Bottom;

    public bool Contains(RectangleF rectangle)
        => rectangle.Left >= Left && rectangle.Right <= Right
            && rectangle.Top >= Top && rectangle.Bottom <= Bottom;

    public bool Intersects(RectangleI other)
        => other.Left < Right && other.Right > Left
        && other.Top < Bottom && other.Bottom > Top;

    public RectangleF Intersect(RectangleF other)
    {
        float left = Math.Max(Left, other.Left);
        float top = Math.Max(Top, other.Top);
        float right = Math.Min(Right, other.Right);
        float bottom = Math.Min(Bottom, other.Bottom);

        return new RectangleF(left, top, right - left, bottom - top);
    }

    public RectangleF Union(RectangleF other)
    {
        float left = Math.Min(Left, other.Left);
        float top = Math.Min(Top, other.Top);
        float right = Math.Max(Right, other.Right);
        float bottom = Math.Max(Bottom, other.Bottom);

        return new RectangleF(left, top, right - left, bottom - top);
    }

    /// <summary>Adjusts the location of this rectangle by the specified amount.</summary>
    /// <returns>This method does not return a value.</returns>
    /// <param name="pos">The amount to offset the location.</param>
    /// <filterpriority>1</filterpriority>
    public RectangleF Offset(Vector2 pos) => Offset(pos.X, pos.Y);

    /// <summary>Adjusts the location of this rectangle by the specified amount.</summary>
    /// <returns>This method does not return a value.</returns>
    /// <param name="y">The amount to offset the location vertically.</param>
    /// <param name="x">The amount to offset the location horizontally.</param>
    /// <filterpriority>1</filterpriority>
    public RectangleF Offset(float x, float y) => new RectangleF(X + x, Y + y, Width, Height);

    internal float DistanceSquared(Vector2D<float> localSpacePos)
    {
        Vector2D<float> dist = new(
            Math.Max(0.0f, Math.Max(localSpacePos.X - Right, Left - localSpacePos.X)),
            Math.Max(0.0f, Math.Max(localSpacePos.Y - Bottom, Top - localSpacePos.Y))
        );

        return dist.LengthSquared;
    }

    internal float DistanceExponentiated(Vector2 localSpacePos, float exponent)
    {
        float distX = Math.Max(0.0f, Math.Max(localSpacePos.X - Right, Left - localSpacePos.X));
        float distY = Math.Max(0.0f, Math.Max(localSpacePos.Y - Bottom, Top - localSpacePos.Y));

        return MathF.Pow(distX, exponent) + MathF.Pow(distY, exponent);
    }

    // This could be optimized further in the future, but made for a simple implementation right now.
    public RectangleI AABB => ((Quad)this).AABB;

    /// <summary>
    /// Constructs a <see cref="RectangleF"/> from left, top, right, and bottom coordinates.
    /// </summary>
    /// <param name="left">The left coordinate.</param>
    /// <param name="top">The top coordinate.</param>
    /// <param name="right">The right coordinate.</param>
    /// <param name="bottom">The bottom coordinate.</param>
    /// <returns>The <see cref="RectangleF"/>.</returns>
    public static RectangleF FromLTRB(float left, float top, float right, float bottom) => new RectangleF(left, top, right - left, bottom - top);

    /// <summary>
    /// Creates a new <see cref="RectangleF"/> in relative coordinate space to another <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="other">The other <see cref="RectangleF"/>.</param>
    /// <returns>The relative coordinate space representation of this <see cref="RectangleF"/> in <paramref name="other"/>.</returns>
    public RectangleF RelativeIn(RectangleF other)
    {
        float scaleX = Width / other.Width;
        float scaleY = Height / other.Height;
        return new RectangleF((X - other.X) / other.Width, (Y - other.Y) / other.Height, scaleX, scaleY);
    }

    public RectangleF Normalize()
        => new(Math.Min(Left, Right), Math.Min(Top, Bottom), Math.Abs(Width), Math.Abs(Height));

    public override bool Equals(object? obj)
        => obj is RectangleF other && Equals(other);

    // from ppy.osu.Framework.Graphics.Primitives.RectangleF
    public override int GetHashCode()
        => (int)(((uint)X ^ ((uint)Y << 13)) | (((uint)Y >> 0x13) ^ ((uint)Width << 0x1a)) | (((uint)Width >> 6) ^ ((uint)Height << 7)) | ((uint)Height >> 0x19));

    public override string ToString()
        => $"RectangleF({X}f, {Y}f, {Width}f, {Height}f)";
}
