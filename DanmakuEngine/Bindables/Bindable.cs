// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.

using System.Globalization;
using DanmakuEngine.Lists;

namespace DanmakuEngine.Bindables;

/// <summary>
/// A generic implementation of a <see cref="IBindable"/>
/// </summary>
/// <typeparam name="T">The type of our stored <see cref="Value"/>.</typeparam>
public class Bindable<T>
{
    /// <summary>
    /// An event which is raised when <see cref="Value"/> has changed (or manually via <see cref="TriggerValueChange"/>).
    /// </summary>
    public event Action<ValueChangedEvent<T>> ValueChanged = null!;

    /// <summary>
    /// An event which is raised when <see cref="Disabled"/> has changed (or manually via <see cref="TriggerDisabledChange"/>).
    /// </summary>
    public event Action<bool> DisabledChanged = null!;

    /// <summary>
    /// An event which is raised when <see cref="Default"/> has changed (or manually via <see cref="TriggerDefaultChange"/>).
    /// </summary>
    public event Action<ValueChangedEvent<T>> DefaultChanged = null!;

    private T value;

    private T defaultValue = default!;

    private bool disabled;

    /// <summary>
    /// Whether this bindable has been disabled. When disabled, attempting to change the <see cref="Value"/> will result in an <see cref="InvalidOperationException"/>.
    /// </summary>
    public virtual bool Disabled
    {
        get => disabled;
        set
        {
            // if a lease is active, disabled can *only* be changed by that leased bindable.
            if (disabled == value) return;

            SetDisabled(value);
        }
    }

    public bool Enabled => !Disabled;

    internal void SetDisabled(bool value, bool bypassChecks = false, Bindable<T> source = null!)
    {
        disabled = value;
        TriggerDisabledChange(source ?? this, true, bypassChecks);
    }

    /// <summary>
    /// Check whether the current <see cref="Value"/> is equal to <see cref="Default"/>.
    /// </summary>
    public virtual bool IsDefault => EqualityComparer<T>.Default.Equals(value, Default);

    /// <summary>
    /// Revert the current <see cref="Value"/> to the defined <see cref="Default"/>.
    /// </summary>
    public void SetDefault() => Value = Default;

    /// <summary>
    /// The current value of this bindable.
    /// </summary>
    public virtual T Value
    {
        get => value;
        set
        {
            // intentionally don't have throwIfLeased() here.
            // if the leased bindable decides to disable exclusive access (by setting Disabled = false) then anything will be able to write to Value.

            if (Disabled)
                throw new InvalidOperationException($"Can not set value to \"{value}\" as bindable is disabled.");

            if (EqualityComparer<T>.Default.Equals(this.value, value)) return;

            SetValue(this.value, value);
        }
    }

    internal void SetValue(T previousValue, T value, bool bypassChecks = false, Bindable<T> source = null!)
    {
        this.value = value;
        TriggerValueChange(previousValue, source ?? this, true, bypassChecks);
    }

    public void InitValue(T value)
        => this.value = value;

    /// <summary>
    /// The default value of this bindable. Used when calling <see cref="SetDefault"/> or querying <see cref="IsDefault"/>.
    /// </summary>
    public virtual T Default
    {
        get => defaultValue;
        set
        {
            // intentionally don't have throwIfLeased() here.
            // if the leased bindable decides to disable exclusive access (by setting Disabled = false) then anything will be able to write to Default.

            if (Disabled)
                throw new InvalidOperationException($"Can not set default value to \"{value}\" as bindable is disabled.");

            if (EqualityComparer<T>.Default.Equals(defaultValue, value)) return;

            SetDefaultValue(defaultValue, value);
        }
    }

    internal void SetDefaultValue(T previousValue, T value, bool bypassChecks = false, Bindable<T> source = null!)
    {
        defaultValue = value;
        TriggerDefaultChange(previousValue, source ?? this, true, bypassChecks);
    }

    private WeakReference<Bindable<T>> weakReferenceInstance = null!;

    private WeakReference<Bindable<T>> weakReference => weakReferenceInstance ??= new WeakReference<Bindable<T>>(this);

    /// <summary>
    /// Creates a new bindable instance. This is used for deserialization of bindables.
    /// </summary>
    private Bindable()
        : this(default!)
    {
    }

    /// <summary>
    /// Creates a new bindable instance initialised with a default value.
    /// </summary>
    /// <param name="defaultValue">The initial and default value for this bindable.</param>
    public Bindable(T defaultValue = default!)
    {
        value = Default = defaultValue;
    }

    protected LockedWeakList<Bindable<T>> Bindings { get; private set; } = null!;

    /// <summary>
    /// Copies all values and value limitations of this bindable to another.
    /// </summary>
    /// <param name="them">The target to copy to.</param>
    public virtual void CopyTo(Bindable<T> them)
    {
        them.Value = Value;
        them.Default = Default;
        them.Disabled = Disabled;
    }

    /// <summary>
    /// Binds this bindable to another such that bi-directional updates are propagated.
    /// This will adopt any values and value limitations of the bindable bound to.
    /// </summary>
    /// <param name="them">The foreign bindable. This should always be the most permanent end of the bind (ie. a ConfigManager).</param>
    /// <exception cref="InvalidOperationException">Thrown when attempting to bind to an already bound object.</exception>
    public virtual void BindTo(Bindable<T> them)
    {
        if (Bindings?.Contains(them.weakReference) == true)
            throw new ArgumentException("An already bound bindable cannot be bound again.");

        them.CopyTo(this);

        addWeakReference(them.weakReference);
        them.addWeakReference(weakReference);
    }

    public void RemoveValueChangedEvent(Action<ValueChangedEvent<T>> onChange)
        => ValueChanged -= onChange;

    public void RemoveDisabledChangedEvent(Action<bool> onChange)
        => DisabledChanged -= onChange;

    /// <summary>
    /// Bind an action to <see cref="ValueChanged"/> with the option of running the bound action once immediately.
    /// </summary>
    /// <param name="onChange">The action to perform when <see cref="Value"/> changes.</param>
    /// <param name="runOnceImmediately">Whether the action provided in <paramref name="onChange"/> should be run once immediately.</param>
    public void BindValueChanged(Action<ValueChangedEvent<T>> onChange, bool runOnceImmediately = false)
    {
        ValueChanged += onChange;
        if (runOnceImmediately)
            onChange(new ValueChangedEvent<T>(Value, Value));
    }

    /// <summary>
    /// Bind an action to <see cref="DisabledChanged"/> with the option of running the bound action once immediately.
    /// </summary>
    /// <param name="onChange">The action to perform when <see cref="Disabled"/> changes.</param>
    /// <param name="runOnceImmediately">Whether the action provided in <paramref name="onChange"/> should be run once immediately.</param>
    public void BindDisabledChanged(Action<bool> onChange, bool runOnceImmediately = false)
    {
        DisabledChanged += onChange;
        if (runOnceImmediately)
            onChange(Disabled);
    }

    private void addWeakReference(WeakReference<Bindable<T>> weakReference)
    {
        Bindings ??= new LockedWeakList<Bindable<T>>();
        Bindings.Add(weakReference);
    }

    private void removeWeakReference(WeakReference<Bindable<T>> weakReference) => Bindings?.Remove(weakReference);

    /// <summary>
    /// Parse an object into this instance.
    /// An object deriving T can be parsed, or a string can be parsed if T is an enum type.
    /// </summary>
    /// <param name="input">The input which is to be parsed.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="input"/>.</param>
    public virtual void Parse(object input, IFormatProvider provider)
    {
        switch (input)
        {
            // Of note, this covers the case when the input is a string and `T` is `string`.
            // Both `string.Empty` and `null` are valid values for this type.
            case T t:
                Value = t;
                break;

            case null:
                // Nullable value types and reference types (annotated or not) are allowed to be initialised with `null`.
                if (typeof(T).IsClass)
                {
                    Value = default!;
                    break;
                }

                // Non-nullable value types can't convert from null.
                throw new ArgumentNullException(nameof(input));

            case Bindable<T>:
                Value = ((Bindable<T>)input).Value;
                break;

            default:
                if (input is string strInput && string.IsNullOrEmpty(strInput))
                {
                    // Nullable value types and reference types are initialised to `null` on empty strings.
                    if (typeof(T).IsClass)
                    {
                        Value = default!;
                        break;
                    }

                    // Most likely all conversion methods will not accept empty strings, but we let this fall through so that the exception is thrown by .NET itself.
                    // For example, DateTime.Parse() throws a more contextually relevant exception than int.Parse().
                }
                break;
        }
    }

    /// <summary>
    /// Raise <see cref="ValueChanged"/> and <see cref="DisabledChanged"/> once, without any changes actually occurring.
    /// This does not propagate to any outward bound bindables.
    /// </summary>
    public virtual void TriggerChange()
    {
        TriggerValueChange(value, this, false);
        TriggerDisabledChange(this, false);
    }

    protected void TriggerValueChange(T previousValue, Bindable<T> source, bool propagateToBindings = true, bool bypassChecks = false)
    {
        // check a bound bindable hasn't changed the value again (it will fire its own event)
        T beforePropagation = value;

        if (propagateToBindings && Bindings != null)
        {
            foreach (var b in Bindings)
            {
                if (b == source) continue;

                b.SetValue(previousValue, value, bypassChecks, this);
            }
        }

        if (EqualityComparer<T>.Default.Equals(beforePropagation, value))
            ValueChanged?.Invoke(new ValueChangedEvent<T>(previousValue, value));
    }

    protected void TriggerDefaultChange(T previousValue, Bindable<T> source, bool propagateToBindings = true, bool bypassChecks = false)
    {
        // check a bound bindable hasn't changed the value again (it will fire its own event)
        T beforePropagation = defaultValue;

        if (propagateToBindings && Bindings != null)
        {
            foreach (var b in Bindings)
            {
                if (b == source) continue;

                b.SetDefaultValue(previousValue, defaultValue, bypassChecks, this);
            }
        }

        if (EqualityComparer<T>.Default.Equals(beforePropagation, defaultValue))
            DefaultChanged?.Invoke(new ValueChangedEvent<T>(previousValue, defaultValue));
    }

    protected void TriggerDisabledChange(Bindable<T> source, bool propagateToBindings = true, bool bypassChecks = false)
    {
        // check a bound bindable hasn't changed the value again (it will fire its own event)
        bool beforePropagation = disabled;

        if (propagateToBindings && Bindings != null)
        {
            foreach (var b in Bindings)
            {
                if (b == source) continue;

                b.SetDisabled(disabled, bypassChecks, this);
            }
        }

        if (beforePropagation == disabled)
            DisabledChanged?.Invoke(disabled);
    }

    /// <summary>
    /// Unbinds any actions bound to the value changed events.
    /// </summary>
    public virtual void UnbindEvents()
    {
        ValueChanged = null!;
        DefaultChanged = null!;
        DisabledChanged = null!;
    }

    /// <summary>
    /// Remove all bound <see cref="Bindable{T}"/>s via <see cref="GetBoundCopy"/> or <see cref="BindTo"/>.
    /// </summary>
    public void UnbindBindings()
    {
        if (Bindings == null)
            return;

        // ToArray required as this may be called from an async disposal thread.
        // This can lead to deadlocks since each child is also enumerating its Bindings.
        foreach (var b in Bindings.ToArray())
            UnbindFrom(b);
    }

    /// <summary>
    /// Calls <see cref="UnbindEvents"/> and <see cref="UnbindBindings"/>.
    /// Also returns any active lease.
    /// </summary>
    public void UnbindAll() => UnbindAllInternal();

    internal virtual void UnbindAllInternal()
    {
        UnbindEvents();
        UnbindBindings();
    }

    public virtual void UnbindFrom(Bindable<T> them)
    {
        removeWeakReference(them.weakReference);
        them.removeWeakReference(weakReference);
    }

    public string Description { get; set; } = null!;

    public sealed override string ToString() => ToString(null!, CultureInfo.CurrentCulture);

    public virtual string ToString(string format, IFormatProvider formatProvider) => string.Format(formatProvider, $"{{0:{format}}}", Value);

    /// <summary>
    /// Create an unbound clone of this bindable.
    /// </summary>
    public Bindable<T> GetUnboundCopy()
    {
        var newBindable = CreateInstance();
        CopyTo(newBindable);
        return newBindable;
    }

    /// <summary>
    /// Create a bound clone of this bindable.
    /// </summary>
    /// <returns>the cloned bindable</returns>
    public Bindable<T> GetBoundCopy()
    {
        var newBindable = CreateInstance();
        newBindable.BindTo(this);
        return newBindable;
    }

    /// <inheritdoc cref="CreateInstance"/>
    protected virtual Bindable<T> CreateInstance() => new Bindable<T>();

    public int BindingCount => Bindings is null ? 0 : Bindings.Count;

    public bool IsBound()
        => Bindings is not null && Bindings.Count > 0;

    public bool IsBoundWith(Bindable<T> bindable)
    {
        if (!IsBound())
            return false;

        foreach (var b in Bindings)
        {
            if (b == bindable)
                return true;
        }

        return false;
    }
}
