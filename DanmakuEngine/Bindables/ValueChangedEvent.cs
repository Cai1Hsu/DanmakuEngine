namespace DanmakuEngine.Bindables;

public class ValueChangedEvent<T> : EventArgs
{
    public readonly T OldValue;
    public readonly T NewValue;

    public ValueChangedEvent(T oldValue, T newValue)
    {
        this.OldValue = oldValue;
        this.NewValue = newValue;
    }
}
