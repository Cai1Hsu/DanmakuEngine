namespace DanmakuEngine.Arguments;

public abstract class Paramaters
{
    public object Convert(string key, Paramaters paramaters)
    {
        foreach (var fieldInfo in this.GetType().GetFields())
        {
            if (fieldInfo.FieldType != typeof(Argument))
                continue;

            var arg = (Argument)fieldInfo.GetValue(paramaters)!;

            if (arg.Key != key)
                continue;

            throw new NotImplementedException();
        }

        throw new NotSupportedException("");
    }

    public virtual Dictionary<Argument, string> Children { get; protected set; } = null!;
}
