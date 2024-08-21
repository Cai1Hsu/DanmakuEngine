using Silk.NET.Maths;

namespace DanmakuEngine.Movements;

public abstract class AcceleratedMovementBase<T> : AccumulatedMovement<T>, ICanGetAcceleration<T>, ICanSetAcceleration<T>, ICanGetSpeed<T>
    where T : IEquatable<T>
{
    private T _acceleration = default!;

    public T Acceleration
    {
        get => _acceleration;
        set
        {
            if (_acceleration.Equals(value))
                return;

            _acceleration = value;
        }
    }

    protected T initialSpeed;

    public abstract T Speed { get; }

    protected abstract override T AccumulatedValue();

    public AcceleratedMovementBase(T initialSpeed, T acceleration)
    {
        this.initialSpeed = initialSpeed;
        this.Acceleration = acceleration;
    }
}

public class AcceleratedMovementF(float initialSpeed, float acceleration)
    : AcceleratedMovementBase<float>(initialSpeed, acceleration)
{
    public override float Speed
        => initialSpeed + (Acceleration * (float)ElapsedTime);

    protected override float AccumulatedValue()
        => Value.Value +
           (initialSpeed * (float)Clock.DeltaTime) +
           (Acceleration * (float)Clock.DeltaTime * (float)Clock.DeltaTime / 2);
}

public class AcceleratedMovementD(double initialSpeed, double acceleration)
    : AcceleratedMovementBase<double>(initialSpeed, acceleration)
{
    public override double Speed
        => initialSpeed + (Acceleration * ElapsedTime);

    protected override double AccumulatedValue()
        => Value.Value +
           (initialSpeed * Clock.DeltaTime) +
           (Acceleration * Clock.DeltaTime * Clock.DeltaTime / 2);
}

public class AcceleratedMovementV2F(Vector2D<float> initialSpeed, Vector2D<float> acceleration)
    : AcceleratedMovementBase<Vector2D<float>>(initialSpeed, acceleration)
{
    public override Vector2D<float> Speed
        => initialSpeed + (Acceleration * (float)ElapsedTime);

    protected override Vector2D<float> AccumulatedValue()
        => Value.Value +
           (initialSpeed * (float)Clock.DeltaTime) +
           (Acceleration * (float)Clock.DeltaTime * (float)Clock.DeltaTime / 2);
}

public class AcceleratedMovementV2D(Vector2D<double> initialSpeed, Vector2D<double> acceleration)
    : AcceleratedMovementBase<Vector2D<double>>(initialSpeed, acceleration)
{
    public override Vector2D<double> Speed
        => initialSpeed + (Acceleration * ElapsedTime);

    protected override Vector2D<double> AccumulatedValue()
        => Value.Value +
           (initialSpeed * Clock.DeltaTime) +
           (Acceleration * Clock.DeltaTime * Clock.DeltaTime / 2);
}
