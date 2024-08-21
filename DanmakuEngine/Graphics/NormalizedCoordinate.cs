namespace DanmakuEngine.Graphics;

public readonly struct Coordinate : IEquatable<Coordinate>
{
    public readonly float X { get; }
    public readonly float Y { get; }

    public Coordinate(float x, float y)
         => (this.X, this.Y) = (x, y);

    public bool Equals(Coordinate other)
        => this.X == other.X
        && this.Y == other.Y;

    // We set a default value to prevent unwanted issue
    public static int windowWidth { get; private set; } = 640;
    public static int windowHeight { get; private set; } = 480;

    /// <summary>
    /// Must call this after creating the window
    /// and before you call <see cref="ToNormalizedCoordinate"/>
    /// </summary>
    /// <param name="width">the current width of the window</param>
    /// <param name="height">the current height of the window</param>
    public static void OnResized(int width, int height)
        => (windowWidth, windowHeight) = (width, height);

    public (float x, float y) ToNormalizedCoordinate()
        => ((this.X / windowWidth) - 0.5f,
            (this.Y / windowHeight) - 0.5f);
}
