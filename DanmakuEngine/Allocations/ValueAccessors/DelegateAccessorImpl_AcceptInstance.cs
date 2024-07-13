namespace DanmakuEngine.Allocations.ValueAccessors;

internal class DelegateAccessorImpl_AcceptInstance<TValue, TInstance> : IAccessor<TValue>
{
    private Func<TInstance, TValue> _getter;
    private Action<TInstance, TValue> _setter;

    private TInstance _instance;

    public DelegateAccessorImpl_AcceptInstance(TInstance instance, Func<TInstance, TValue> getter, Action<TInstance, TValue> setter)
    {
        ArgumentNullException.ThrowIfNull(getter, nameof(getter));
        ArgumentNullException.ThrowIfNull(setter, nameof(setter));

        _getter = getter;
        _setter = setter;

        _instance = instance;
    }

    public TValue Value
    {
        get => _getter(_instance);
        set => _setter(_instance, value);
    }
}
