namespace DanmakuEngine.Allocations;

/// <summary>
/// A lazy load value that will create an instance of <typeparamref name="TValue"/> when <see cref="Value"/> is first accessed
/// </summary>
/// <typeparam name="TValue">The type of the instance</typeparam>
/// <remarks>
/// Create a lazy load instance of <typeparamref name="TValue"/> with parameters for the constructor
/// </remarks>
public class LazyLoadValue<TValue>(Func<TValue> loader)
    where TValue : class
{
    private TValue? _value;

    private Func<TValue> _loader = loader;

    private readonly object _lock = new();

    public TValue Value
    {
        get
        {
            if (_value is null)
            {
                lock (_lock)
                {
                    _value = _loader();

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

    public static implicit operator TValue(LazyLoadValue<TValue> lazyLoadValue) => lazyLoadValue.Value;

    private class LazyLoadValueException(string message)
        : Exception(message)
    {
    }
}