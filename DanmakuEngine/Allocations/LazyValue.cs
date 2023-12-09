namespace DanmakuEngine.Allocations;

/// <summary>
/// A lazy load value that will create an instance of <typeparamref name="TValue"/> when <see cref="Value"/> is first accessed
/// </summary>
/// <typeparam name="TValue">The type of the instance</typeparam>
/// <remarks>
/// Create a lazy load instance of <typeparamref name="TValue"/> with parameters for the constructor
/// </remarks>
public class LazyValue<TValue>
    where TValue : class
{
    private volatile TValue? _value;

    private Func<TValue> _loader = null!;

    private readonly object loader_lock = new();

    private readonly object value_lock = new();

    private bool loading_failed = false;

    private Exception? exception = null;

    public TValue Value
    {
        get
        {
            if (_value is not null)
                return _value;

            lock (loader_lock)
            {
                if (_loader is not null)
                {
                    lock (value_lock)
                    {
                        try
                        {
                            _value = _loader();
                        }
                        catch (Exception e)
                        {
                            // ignored
                            loading_failed = true;
                            exception = e;
                        }
                    }

                    if (_value is not null)
                        // We don't need the loader anymore
                        _loader = null!;
                    // otherwise
                    // we still returns null as it's up to user to decide what to do with the exception
                }
            }

            return _value!;
        }
    }

    public bool HasValue => _value is not null;

    /// <summary>
    /// Returns the raw value of the instance
    /// </summary>
    /// <remarks>
    /// try to access the value without having to create an instance of <typeparamref name="TValue"/>
    /// but it will return null if the instance is not created yet
    /// </remarks>
    public TValue? RawValue => _value;

    /// <summary>
    /// Returns the value of the instance
    /// </summary>
    /// <remarks>
    /// useful when you want to pass an anonymous function to a method that requires a <see cref="Func{T}"/> but you don't want to create an instance of <typeparamref name="TValue"/> yet
    /// </remarks>
    public TValue GetValue() => Value;

    /// <summary>
    /// Returns whether we can create an instance of <typeparamref name="TValue"/> with loader  <see cref="Func{T}"/>
    /// </summary>
    public bool IsLoaderNull
        => _loader is null;

    public bool IsLoadingFailed
        => loading_failed;

    public Exception? LoadingException
        => exception;

    /// <summary>
    /// Try to create an instance of <typeparamref name="TValue"/> with loader <see cref="Func{T}"/>
    /// </summary>
    /// <returns>whether created successfully</returns>
    /// <remarks>
    /// Value may still be null if the loader returned null
    /// </remarks>
    public bool TryLoadValue()
    {
        if (IsLoaderNull)
            return false;

        lock (loader_lock)
        {
            lock (value_lock)
            {
                _value = _loader();
            }
        }

        return true;
    }

    public bool AssignValue(TValue lazyValue, bool force = false)
    {
        if (_value is not null && !force)
            return false;

        lock (value_lock)
        {
            _value = lazyValue;
        }

        lock (loader_lock)
        {
            if (_loader is not null)
            {
                _loader = null!;
                loading_failed = false;
                exception = null;
            }
        }

        return true;
    }

    /// <summary>
    /// Reset the loader to a new one and clear the value
    /// </summary>
    /// <param name="loader">the loader to create a value</param>
    public void Reset(Func<TValue> loader)
    {
        lock (value_lock)
        {
            _value = null;
        }

        lock (loader_lock)
        {
            _loader = loader;
            loading_failed = false;
            exception = null;
        }
    }

    public LazyValue(Func<TValue> loader)
    {
        _loader = loader;
    }

    /// <summary>
    /// You may use this constructor for compatibility reason
    /// </summary>
    /// <param name="value"></param>
    public LazyValue(TValue value)
    {
        _value = value;
    }

    public LazyValue(LazyValue<TValue> lazyValue)
    {
        if (lazyValue.HasValue)
        {
            _value = lazyValue.Value;
        }
        else
        {
            _loader = lazyValue._loader;
        }
    }

    public static explicit operator LazyValue<TValue>(TValue value) => new(value);

    public static implicit operator TValue(LazyValue<TValue> lazyValue) => lazyValue.Value;

    public static implicit operator LazyValue<TValue>(Func<TValue> loader) => new(loader);
}

public static class LazyValue
{
    public static LazyValue<TBase> ToBase<TBase, TSub>(this LazyValue<TSub> lazyValue)
        where TSub : class, TBase
        where TBase : class
    {
        if (lazyValue.HasValue)
            return new LazyValue<TBase>(lazyValue.Value);

        return new LazyValue<TBase>(lazyValue.GetValue);
    }

    public static LazyValue<T> Create<T>()
        where T : class, new()
    {
        return new LazyValue<T>(() => new T());
    }
}
