using DanmakuEngine.Timing;

namespace DanmakuEngine.Movement;

public class GravityDropMovement : LinearAccelerateMovement
{
    /// <summary>
    /// You must be very familiar with this number
    /// </summary>
    private const double GRAVITY = 9.81;

    public GravityDropMovement(IClock clock)
        : base(GRAVITY, clock)
    {
    }

    public GravityDropMovement(double initialSpeed, IClock clock)
        : base(GRAVITY, initialSpeed, clock)
    {
    }
}
