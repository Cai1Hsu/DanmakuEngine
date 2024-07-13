namespace DanmakuEngine.Allocations.ValueAccessors;

internal class DelegateAccessorImpl_DelegateOnly<TValue> : IAccessor<TValue>
{
    private Func<TValue> _getter;
    private Action<TValue> _setter;

    public DelegateAccessorImpl_DelegateOnly(Func<TValue> getter, Action<TValue> setter)
    {
        ArgumentNullException.ThrowIfNull(getter, nameof(getter));
        ArgumentNullException.ThrowIfNull(setter, nameof(setter));

        _getter = getter;
        _setter = setter;
    }

    public TValue Value
    {
        get => _getter();
        set => _setter(value);
    }
}
