namespace DanmakuEngine.Allocations;

/// <summary>
/// Represents a value that is allocated on the stack. Similar to <see cref="Span{T}"/>, but for single values.
/// </summary>
/// <typeparam name="T">The type of value</typeparam>
/// see https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct
/// and https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines for more info.
public unsafe ref struct StackValue<T>
    where T : unmanaged
{
    private readonly ref T _value;
    public readonly ref T Value => ref _value;

    /// <summary>
    /// Creates a new <see cref="StackValue{T}"/> from a <see cref="Span{T}"/>.
    /// Only the first element of the span is used.
    /// </summary>
    /// <param name="span"></param>
    public StackValue(Span<T> span)
    {
        _value = ref span[0];
    }

    public StackValue(ref T value)
    {
        _value = ref value;
    }

    public StackValue(T* value)
    {
        _value = ref *value;
    }

    public static implicit operator StackValue<T>(T* value) => new(value);

    public static implicit operator T(StackValue<T> value) => value.Value;
    public static implicit operator T*(StackValue<T> value) => &value._value;
    public static implicit operator Span<T>(StackValue<T> value) => new(&value._value, 1);
}
