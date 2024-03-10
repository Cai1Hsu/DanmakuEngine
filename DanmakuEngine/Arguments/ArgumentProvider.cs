namespace DanmakuEngine.Arguments;

public class ArgumentProvider : IDisposable
{
    private ArgumentParser _argParser;
    private Dictionary<string, Argument> ArgumentMap;

    public ArgumentProvider(ArgumentParser parser, Dictionary<string, Argument> map)
    {
        this._argParser = parser;
        this.ArgumentMap = map;
    }

    public bool IsSupport(string key) => _argParser.IsSupport(key);

    public bool Find(string key) => _argParser.IsSupport(key) && this.ArgumentMap.ContainsKey(key);

    /// <summary>
    /// Try to get the value of the flag. If the user didn't pass the KV pair,
    /// we return the *Default* value.
    /// </summary>
    /// <typeparam name="TResult">The type of the value</typeparam>
    /// <param name="key">the flag</param>
    /// <returns>the value of the flag</returns>
    /// <exception cref="InvalidOperationException">When the flag is not supported</exception>
    public TResult GetValue<TResult>(string key) => (TResult)GetValue(key);

    public object GetValue(string key)
    {
        if (Find(key))
            return ArgumentMap[key].GetValue();

        if (_argParser.IsSupport(key))
            return _argParser.GetDefault(key);

        throw new InvalidOperationException($"Target argument does not exist: {key}");
    }

    public object GetDefault(string key)
        => _argParser.GetDefault(key);

    public T GetDefault<T>(string key)
        => (T)GetDefault(key);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        ArgumentMap = null!;
    }

    /// <summary>
    /// Please be aware that create default provider with this method will
    /// access the command line arguments.
    /// </summary>
    /// <param name="template"></param>
    /// <returns></returns>
    public static ArgumentProvider CreateDefaultProvider(Paramaters template)
        => new ArgumentParser(template).CreateProvider();

    public static ArgumentProvider CreateDefault(Paramaters template, string[] args)
=> new ArgumentParser(template, args).CreateProvider();
}
