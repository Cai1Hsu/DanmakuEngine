using System.Numerics;
using DanmakuEngine.Bindables;

namespace DanmakuEngine.Movements;

public class LinearMovement<T> : AccumulatedMovement<T>, ICanGetSpeed<T>, ICanSetSpeed<T>
    where T : IAdditionOperators<T, T, T>, IMultiplyOperators<T, double, T>
{
    private double _current_period_start_time;

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
                    _current_period_start_time = CurrentTime;

                    Active.ValueChanged -= splitPeriod;
                }
            }
        }
    }

    protected override T AccumulatedValue()
        => Value.Value +
           // we still want to try to avoid accumulating to prevent floating point errors
           Speed * (CurrentTime - _current_period_start_time);

    public LinearMovement(T speed)
    {
        this.Speed = speed;
    }
}
