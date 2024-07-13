namespace DanmakuEngine.Allocations.ValueAccessors;

public class RefAccessorImpl_AcceptInstance<TValue, TInstance> : IAccessor<TValue>
{
    public delegate ref TValue RefAccessorDelegate(TInstance instance);

    private RefAccessorDelegate _getRef;

    private TInstance _instance;

    public RefAccessorImpl_AcceptInstance(TInstance instance, RefAccessorDelegate getRef)
    {
        ArgumentNullException.ThrowIfNull(getRef, nameof(getRef));

        _getRef = getRef;

        _instance = instance;
    }

    public TValue Value
    {
        get => _getRef(_instance);
        set => _getRef(_instance) = value;
    }
}
