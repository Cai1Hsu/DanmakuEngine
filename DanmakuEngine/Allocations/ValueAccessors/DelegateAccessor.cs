namespace DanmakuEngine.Allocations.ValueAccessors;

/// <summary>
/// A value accessor using getter and setter delegates.
/// </summary>
/// <typeparam name="TValue">The type of the value</typeparam>
public class DelegateAccessor<TValue> : IAccessor<TValue>
{
    private IAccessor<TValue> _impl;

    private DelegateAccessor(IAccessor<TValue> impl)
    {
        _impl = impl;
    }

    public static DelegateAccessor<TValue> Create(Func<TValue> getter, Action<TValue> setter)
        => new DelegateAccessor<TValue>(new DelegateAccessorImpl_DelegateOnly<TValue>(getter, setter));

    public static DelegateAccessor<TValue> Create<TInstance>(TInstance instance, Func<TInstance, TValue> getter, Action<TInstance, TValue> setter)
        => new DelegateAccessor<TValue>(new DelegateAccessorImpl_AcceptInstance<TValue, TInstance>(instance, getter, setter));

    public TValue Value
    {
        get => _impl.Value;
        set => _impl.Value = value;
    }
}
