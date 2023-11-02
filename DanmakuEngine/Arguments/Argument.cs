// All keys must be lower-cased
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DanmakuEngine.Arguments;

public class Argument
{
    public bool HasValue => this.TValue != null && this.Value != null;

    public readonly string Key;
    public readonly Type TValue = null!;
    private readonly object Value = null!;

    public Action<Argument> Operation = null!;

    // public Func<string, object> CustomConvertor = null!;

    public Argument(string key)
    {
        this.Key = key.ToLower();
    }
    public Argument(string key, Type TValue, object value)
    {
        this.Key = key.ToLower();
        this.TValue = TValue;
        this.Value = Convert(value);
    }

    public Argument(string key, Type valueType, object value, Action<Argument> operation = null!/*, Func<string, object> convertor = null!*/)
    {
        this.Key = key.ToLower();

        this.TValue = valueType;
        this.Value = Convert(value);

        this.Operation = operation;
        // this.CustomConvertor = convertor;
    }

    private object Convert(object value)
    {
        if (this.TValue == typeof(string)
            && value.GetType() == typeof(string))
            return ((string)value).Trim('\"');

        if (value.GetType() == TValue)
            return value;

        if (value is not string v)
            throw new NotSupportedException($"Unsupported value type: {this.TValue}");

        if (this.TValue == typeof(int))
            return int.Parse(v);

        if (this.TValue == typeof(double))
            return double.Parse(v);

        if (this.TValue == typeof(float))
            return float.Parse(v);

        if (this.TValue == typeof(bool))
            return bool.Parse(v.ToLower());

        // To support custom convertor is not worthwhile.
        // users can parse arguments later.

        // if (this.CustomConvertor == null)
        throw new NotSupportedException($"Unsupported value type for flag \"{this.Key}\": {this.TValue}");

        // var converted = CustomConvertor.Invoke(v);

        // if (converted.GetType() != TValue)
        //     throw new ValidationException($"Return type does NOT match the give TValue, expect: {TValue}, but was: {converted.GetType()}");

        // return converted;
    }

    public Argument(string key, Action<Argument> action)
    {
        this.Key = key.ToLower();
        this.Operation = action;
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