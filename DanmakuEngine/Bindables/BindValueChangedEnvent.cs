namespace DanmakuEngine.Bindables;

public class BindValueChangedEvent<T> : EventArgs
{
    public readonly T OldValue;
    public readonly T NewValue;

    public BindValueChangedEvent(T oldValue, T newValue)
    {
        this.OldValue = oldValue;
        this.NewValue = newValue;
    }
}
