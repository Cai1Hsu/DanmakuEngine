// Copyright (c) ppy Pty Ltd <contact@ppy.sh>.

using Silk.NET.Maths;

namespace DanmakuEngine.Graphics.Primitives;

public interface IPolygon
{
    /// <summary>
    /// The vertices that define the axes spanned by this polygon in screen-space counter-clockwise orientation.
    /// </summary>
    /// <remarks>
    /// Counter-clockwise orientation in screen-space coordinates is equivalent to a clockwise orientation in standard coordinates.
    /// <para>
    /// E.g. For the set of vertices { (0, 0), (1, 0), (0, 1), (1, 1) }, a counter-clockwise orientation is { (0, 0), (0, 1), (1, 1), (1, 0) }.
    /// </para>
    /// </remarks>
    /// <returns>The vertices that define the axes spanned by this polygon.</returns>
    ReadOnlySpan<Vector2D<float>> GetAxisVertices();

    /// <summary>
    /// Retrieves the vertices of this polygon in screen-space counter-clockwise orientation.
    /// </summary>
    /// <remarks>
    /// Counter-clockwise orientation in screen-space coordinates is equivalent to a clockwise orientation in standard coordinates.
    /// <para>
    /// E.g. For the set of vertices { (0, 0), (1, 0), (0, 1), (1, 1) }, a counter-clockwise orientation is { (0, 0), (0, 1), (1, 1), (1, 0) }.
    /// </para>
    /// </remarks>
    /// <returns>The vertices of this polygon.</returns>
    ReadOnlySpan<Vector2D<float>> GetVertices();
}
