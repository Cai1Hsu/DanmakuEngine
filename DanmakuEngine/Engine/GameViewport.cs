using Silk.NET.Maths;

namespace DanmakuEngine.Engine;

public class GameViewport
{
    /// <summary>
    /// The size of the viewport.
    /// </summary>
    public Vector2D<float> Size { get; internal set; }

    /// <summary>
    /// Usually to be (0, 0).
    /// </summary>
    public Vector2D<float> Position { get; internal set; }

    public Vector2D<float> ToWorldSpace(Vector2D<float> vector)
        => new Vector2D<float>(
            (vector.X - (Size.X / 2)) * 640.0f / Size.X,
            ((Size.Y / 2) - vector.Y) * 480.0f / Size.Y
        );

    public void UpdateSize(Vector2D<float> size)
        => Size = size;
}
