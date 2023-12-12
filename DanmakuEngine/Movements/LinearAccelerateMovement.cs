using DanmakuEngine.Timing;

namespace DanmakuEngine.Movement;

public class LinearAccelerateMovement : DoubleMovement
{
    private readonly double acceleration;

    public override void Update()
    {
        // least accurate version:
        // speed += acceleration * Clock.UpdateDelta;
        // value += speed * Clock.UpdateDelta;

        // more accurate version:
        // double initialSpeed = speed;
        // speed += acceleration * Clock.UpdateDelta;
        // value += initialSpeed * Clock.UpdateDelta
        //     + 0.5 * acceleration * Math.Pow(Clock.UpdateDelta, 2);

        // most accurate version
        value = 0.5 * acceleration * (Clock.CurrentTime * Clock.CurrentTime);
    }

    public LinearAccelerateMovement
        (double acceleration, double initialSpeed, IClock clock)
        : base(clock)
    {
        this.speed = initialSpeed;
        this.acceleration = acceleration;
    }

    public LinearAccelerateMovement
        (double acceleration, IClock clock)
        : this(acceleration, 0, clock)
    {
    }
}
