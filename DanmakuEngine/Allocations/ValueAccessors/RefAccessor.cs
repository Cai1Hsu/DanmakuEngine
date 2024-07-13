namespace DanmakuEngine.Allocations.ValueAccessors;

/// <summary>
/// A value accessor for animation transform. You may pass a delegate that returns a reference to the value.
/// </summary>
/// <typeparam name="TValue">The type of the value</typeparam>
public class RefAccessor<TValue> : IAccessor<TValue>
{
    private IAccessor<TValue> _impl;

    private RefAccessor(IAccessor<TValue> impl)
    {
        ArgumentNullException.ThrowIfNull(impl, nameof(impl));

        _impl = impl;
    }

    public static RefAccessor<TValue> Create(RefAccessorImpl_DelegateOnly<TValue>.RefAccessorDelegate getRef)
    {
        ArgumentNullException.ThrowIfNull(getRef, nameof(getRef));

        return new RefAccessor<TValue>(new RefAccessorImpl_DelegateOnly<TValue>(getRef));
    }

    public static RefAccessor<TValue> Create<TInstance>(TInstance instance, RefAccessorImpl_AcceptInstance<TValue, TInstance>.RefAccessorDelegate getRef)
    {
        ArgumentNullException.ThrowIfNull(getRef, nameof(getRef));

        return new RefAccessor<TValue>(new RefAccessorImpl_AcceptInstance<TValue, TInstance>(instance, getRef));
    }

    public TValue Value
    {
        get => _impl.Value;
        set => _impl.Value = value;
    }
}
