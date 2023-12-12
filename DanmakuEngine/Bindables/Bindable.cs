namespace DanmakuEngine.Bindables;

public class Bindable<T>
{
    protected event Action<bool> EnabledChanged = null!;

    private bool _enabled;
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (value == Enabled)
                return;

            _enabled = value;

            OnEnabledChanged(value);
        }
    }

    protected event Action<BindValueChangedEvent<T>> ValueChanged = null!;

    private T _value;
    public T Value
    {
        get => _value;
        set
        {
            if (!Enabled)
                throw new InvalidOperationException("Can NOT change a disabled Bindable.");

            if (value!.Equals(_value))
                return;

            T old = _value;
            _value = value;

            OnValueChanged(new BindValueChangedEvent<T>(old, _value));
        }
    }

    private Bindable<T>? _weakReference = null;

    public bool IsBound() => _weakReference != null;

    public bool IsBoundWith(Bindable<T> them) => IsBound() && this._weakReference!.Equals(them);

    public void AddEnabledChangedEvent(Action<bool> enabledChanged, bool executeImmediately = false)
    {
        this.EnabledChanged += enabledChanged;

        if (executeImmediately)
            enabledChanged(_enabled);
    }

    public void RemoveEnabledChangedEvent(Action<bool> enabledChanged)
    {
        this.EnabledChanged -= enabledChanged;
    }

    protected virtual void OnEnabledChanged(bool enabled)
    {
        EnabledChanged?.Invoke(enabled);
    }

    protected virtual void OnValueChanged(BindValueChangedEvent<T> e)
    {
        ValueChanged?.Invoke(e);
    }

    public void RemoveBindValueChangedEvent(Action<BindValueChangedEvent<T>> eventHandler)
    {
        if (!Enabled)
            throw new InvalidOperationException("Can NOT change a disabled Bindable.");

        this.ValueChanged -= eventHandler;
    }

    public void AddBindValueChangedEvent(Action<BindValueChangedEvent<T>> eventHandler, bool executeImmediately = false)
    {
        if (!Enabled)
            throw new InvalidOperationException("Can NOT change a disabled Bindable.");

        this.ValueChanged += eventHandler;

        if (executeImmediately)
            eventHandler(new BindValueChangedEvent<T>(_value, _value));
    }

    public void BindTo(Bindable<T> them)
    {
        if (them is not Bindable<T>)
            throw new InvalidOperationException($"Can NOT bind to a bindable with different type, this: {this.GetType()}, them: {them.GetType()}");

        if (!Enabled || !them.Enabled)
            throw new InvalidOperationException("Can NOT change a disabled Bindable.");

        if (this.IsBound() || them.IsBound())
            throw new InvalidOperationException("Can NOT bind to multiple bindables.");

        this.CopyTo(them);

        this.addWeakReference(them);
        them.addWeakReference(this);
    }

    protected void addWeakReference(Bindable<T> them)
    {
        this._weakReference = them;

        this.ValueChanged += syncOnValueChanged;
        this.EnabledChanged += syncOnEnabledChanged;
    }

    protected virtual void syncOnValueChanged(BindValueChangedEvent<T> e)
    {
        _weakReference!.Value = this._value;
    }

    protected virtual void syncOnEnabledChanged(bool enabled)
    {
        _weakReference!.Enabled = this._enabled;
    }

    public void Unbind(Bindable<T> them)
    {
        if (!IsBound() || !them.IsBound())
            throw new InvalidOperationException("Can NOT unbind before bind");

        if (!this._weakReference!.Equals(them) ||
            !them._weakReference!.Equals(this))
            throw new InvalidOperationException("Can NOT unbind from a bindable that has not bound to this.");

        this.removeWeakReference();
        them.removeWeakReference();
    }

    /// <summary>
    /// I want to unbind it no matter what it is!!!
    /// </summary>
    public void Unbind()
    {
        // We should remove their's weak reference first    
        this._weakReference?.removeWeakReference();

        this.removeWeakReference();
    }

    protected void removeWeakReference()
    {
        this.ValueChanged -= syncOnValueChanged;
        this.EnabledChanged -= syncOnEnabledChanged;

        this._weakReference = null;
    }

    public void CopyTo(Bindable<T> them)
    {
        them._value = this._value;
        them._enabled = this._enabled;
    }

    public bool ValueEquals(Bindable<T> them) => this.Value!.Equals(them.Value);

    public bool ValueEquals(T value) => this.Value!.Equals(value);

    public Bindable(T value, bool enabled = true)
    {
        this._value = value;
        this._enabled = enabled;
    }

    public override string ToString() => $"Bindable<{typeof(T)}>({Value})";

    public string ToString(string formatter) => string.Format("Bindable<{0}>({{1}:{2}})", typeof(T), Value, formatter);
}
