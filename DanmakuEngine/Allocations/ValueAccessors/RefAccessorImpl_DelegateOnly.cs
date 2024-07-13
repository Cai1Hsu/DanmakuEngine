namespace DanmakuEngine.Allocations.ValueAccessors;

public class RefAccessorImpl_DelegateOnly<TValue> : IAccessor<TValue>
{
    public delegate ref TValue RefAccessorDelegate();

    private RefAccessorDelegate _getRef;

    public RefAccessorImpl_DelegateOnly(RefAccessorDelegate getRef)
    {
        ArgumentNullException.ThrowIfNull(getRef, nameof(getRef));

        _getRef = getRef;
    }

    public TValue Value
    {
        get => _getRef();
        set => _getRef() = value;
    }
}
