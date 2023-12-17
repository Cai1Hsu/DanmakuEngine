using System.Numerics;
using DanmakuEngine.Bindables;
using Silk.NET.Maths;

namespace DanmakuEngine.Movements;

public abstract class LinearMovement<T> : AccumulatedMovement<T>, ICanGetSpeed<T>, ICanSetSpeed<T>
    where T : IEquatable<T>
{
    protected double currentPeriodStartTime;

    private T _speed = default!;

    public T Speed
    {
        get => _speed;
        set
        {
            if (_speed.Equals(value))
                return;

            _speed = value;

            Active.BindValueChanged(splitPeriod, true);

            void splitPeriod(ValueChangedEvent<bool> e)
            {
                if (e.NewValue)
                {
                    currentPeriodStartTime = CurrentTime;

                    Active.ValueChanged -= splitPeriod;
                }
            }
        }
    }

    public LinearMovement(T speed)
    {
        this.Speed = speed;
    }
}

public class LinearMovementF(float speed)
    : LinearMovement<float>(speed)
{
    protected override float AccumulatedValue()
        => Value.Value +
           // we still want to try to avoid accumulating to prevent floating point errors
           Speed * (float)(CurrentTime - currentPeriodStartTime);

}

public class LinearMovementD(double speed)
    : LinearMovement<double>(speed)
{
    protected override double AccumulatedValue()
        => Value.Value +
           // we still want to try to avoid accumulating to prevent floating point errors
           Speed * (CurrentTime - currentPeriodStartTime);
}

public class LinearMovementV2F(Vector2D<float> speed)
    : LinearMovement<Vector2D<float>>(speed)
{
    protected override Vector2D<float> AccumulatedValue()
        => Value.Value +
           // we still want to try to avoid accumulating to prevent floating point errors
           Speed * (float)(CurrentTime - currentPeriodStartTime);
}

public class LinearMovementV2D(Vector2D<double> speed)
    : LinearMovement<Vector2D<double>>(speed)
{
    protected override Vector2D<double> AccumulatedValue()
        => Value.Value +
           // we still want to try to avoid accumulating to prevent floating point errors
           Speed * (CurrentTime - currentPeriodStartTime);
}