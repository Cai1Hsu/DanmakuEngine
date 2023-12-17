using Silk.NET.Maths;

namespace DanmakuEngine.Movements;

public class GravityDropMovementF(float initialSpeed)
    : FixedAcceleratedMovementF(initialSpeed, GRAVITY)
{
    /// <summary>
    /// You must be very familiar with this number
    /// </summary>
    public const float GRAVITY = 9.81f;
}

public class GravityDropMovementD(double initialSpeed)
    : FixedAcceleratedMovementD(initialSpeed, GRAVITY)
{
    /// <summary>
    /// You must be very familiar with this number
    /// </summary>
    public const double GRAVITY = 9.81;
}

public class GravityDropMovementV2F(Vector2D<float> initialSpeed)
    : FixedAcceleratedMovementV2F(initialSpeed, GRAVITY)
{
    /// <summary>
    /// You must be very familiar with this number
    /// </summary>
    public static readonly Vector2D<float> GRAVITY = new(0, -9.81f);
}

public class GravityDropMovementV2D(Vector2D<double> initialSpeed)
    : FixedAcceleratedMovementV2D(initialSpeed, GRAVITY)
{
    /// <summary>
    /// You must be very familiar with this number
    /// </summary>
    public static readonly Vector2D<double> GRAVITY = new(0, -9.81);
}
