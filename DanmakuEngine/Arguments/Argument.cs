// All keys must be lower-cased
namespace DanmakuEngine.Arguments;

public class Argument
{
    public bool HasValue = false;

    public readonly string Key;
    public readonly Type TValue = null!;
    private readonly object Value = null!;

    public Action<Argument> Action = null!;

    public Argument(string key, Type TValue, object value)
    {
        this.Key = key.ToLower();
        this.TValue = TValue;
        this.Value = Convert(value);

        this.HasValue = true;
    }

    public Argument(string key)
    {
        this.Key = key.ToLower();
    }

    public Argument(string key, Type valueType, object value, Action<Argument> action)
    {
        this.Key = key.ToLower();

        this.TValue = valueType;
        this.Value = Convert(value);

        this.Action = action;

        this.HasValue = true;
    }

    private object Convert(object value)
    {
        if (value.GetType() == TValue)
            return value;

        if (this.TValue == typeof(string))
            return value;

        if (value is not string v)
            throw new NotSupportedException($"Unsupported argument type: {this.TValue}");

        if (this.TValue == typeof(int))
            return int.Parse(v);

        if (this.TValue == typeof(double))
            return double.Parse(v);

        if (this.TValue == typeof(float))
            return float.Parse(v);

        throw new NotSupportedException($"Unsupported argument type: {this.TValue}");
    }

    public Argument(string key, Action<Argument> action)
    {
        this.Key = key.ToLower();
        this.Action = action;
    }

    public TValue GetValue<TValue>()
    {
        if (!HasValue)
            throw new InvalidOperationException("This argument does NOT contain a value.");

        if (this.TValue != typeof(TValue))
            throw new InvalidOperationException($"Type does NOT match, expect: {this.TValue}, found {typeof(TValue)}");

        return (TValue)Value;
    }

    /// <summary>
    /// Get the string version of the value or the whole class
    /// </summary>
    /// <param name="format">defines whether to return the value or the whole class</param>
    /// <returns>the string version of the value or the whole class</returns>
    /// <exception cref="NotImplementedException"></exception>

    public string ToString(bool format = false)
    {
        if (!format)
            return Value.ToString()!;

        throw new NotImplementedException();
    }
}