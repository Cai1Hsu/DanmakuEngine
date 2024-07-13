using DanmakuEngine.Bindables;

namespace DanmakuEngine.Allocations.ValueAccessors;

/// <summary>
/// A value accessor for bindable values.
/// </summary>
/// <typeparam name="TValue">The type of the value</typeparam>
public class BindableAccessor<TValue> : IAccessor<TValue>
{
    public TValue Value
    {
        get => _bindable.Value;
        set => _bindable.Value = value;
    }

    internal readonly Bindable<TValue> _bindable;

    public BindableAccessor(Bindable<TValue> bindable)
    {
        ArgumentNullException.ThrowIfNull(bindable, nameof(bindable));

        _bindable = bindable.GetBoundCopy();
    }
}
