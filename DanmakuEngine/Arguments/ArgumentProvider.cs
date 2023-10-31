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

    public bool Find(string key) => _argParser.IsSupport(key) && this.ArgumentMap.ContainsKey(key);

    /// <summary>
    /// Try to get the value of the flag. If the user didn't pass the KV pair,
    /// we return the *Default* value. 
    /// </summary>
    /// <typeparam name="TResult">The type of the value</typeparam>
    /// <param name="key">the flag</param>
    /// <returns>the value of the flag</returns>
    /// <exception cref="InvalidOperationException">When the flag is not supported</exception>
    public TResult GetValue<TResult>(string key)
    {
        if (Find(key))
            return ArgumentMap[key].GetValue<TResult>();

        if (_argParser.IsSupport(key))
            return _argParser.GetDefault<TResult>(key);

        throw new InvalidOperationException("Target argument does not exist.");
    }

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
}
