using System.Diagnostics.CodeAnalysis;

namespace DanmakuEngine.Allocations;

/// <summary>
/// A lazy load value that will create an instance of <typeparamref name="T"/> when <see cref="Value"/> is first accessed
/// 
/// You must select and pass correct Parameters for the constructor of <typeparamref name="T"/> when creating an instance of this class
/// </summary>
/// <typeparam name="T">The type of the instance</typeparam>
public class LazyLoadValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]T>
{
    private T? _value;

    private object[] parameters;

    /// <summary>
    /// Create a lazy load instance of <typeparamref name="T"/> with no parameters
    /// </summary>
    private LazyLoadValue()
    {
        parameters = null!;
    }

    /// <summary>
    /// Create a lazy load instance of <typeparamref name="T"/> with parameters for the constructor
    /// </summary>
    /// <param name="parameters"></param>
    public LazyLoadValue(params object[] parameters)
    {
        this.parameters = parameters;
    }

    public T Value
    {
        get
        {
            if (_value is null)
            {
                if (parameters is null)
                    _value = Activator.CreateInstance<T>()!;
                else
                    _value = (T)Activator.CreateInstance(typeof(T), parameters)!;

                if (_value is null)
                    throw new LazyLoadValueException($"Failed to create instance of {typeof(T)}");

                // We don't need the parameters anymore
                parameters = null!;
            }

            return _value;
        }
    }

    private class LazyLoadValueException(string message)
        : Exception(message)
    {
    }
}