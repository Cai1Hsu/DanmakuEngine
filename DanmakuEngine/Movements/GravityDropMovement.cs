using Silk.NET.Maths;

namespace DanmakuEngine.Movements;

public class GravityDropMovementF(float initialSpeed)
    : FixedAcceleratedMovementF(initialSpeed, GRAVITY_F)
{
    /// <summary>
    /// You must be very familiar with this number
    /// </summary>
    public const float GRAVITY_F = 9.81f;
}

public class GravityDropMovementD(double initialSpeed)
    : FixedAcceleratedMovementD(initialSpeed, GRAVITY_D)
{
    /// <summary>
    /// You must be very familiar with this number
    /// </summary>
    public const double GRAVITY_D = 9.81;
}

public class GravityDropMovementV2F(Vector2D<float> initialSpeed)
    : FixedAcceleratedMovementV2F(initialSpeed, GRAVITY_V2F)
{
    /// <summary>
    /// You must be very familiar with this number
    /// </summary>
    public static readonly Vector2D<float> GRAVITY_V2F = new(0, -9.81f);
}

public class GravityDropMovementV2D(Vector2D<double> initialSpeed)
    : FixedAcceleratedMovementV2D(initialSpeed, GRAVITY_V2D)
{
    /// <summary>
    /// You must be very familiar with this number
    /// </summary>
    public static readonly Vector2D<double> GRAVITY_V2D = new(0, -9.81);
}
