namespace DanmakuEngine.Allocations.ValueAccessors;

/// <summary>
/// A value accessor for animation transform. You may pass a delegate that returns a reference to the value.
/// </summary>
/// <typeparam name="TValue">The type of the value</typeparam>
public class RefAccessor<TValue> : IAccessor<TValue>
{
    public delegate ref TValue RefAccessorDelegate();

    private RefAccessorDelegate _getRef;

    public RefAccessor(RefAccessorDelegate getRef)
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
