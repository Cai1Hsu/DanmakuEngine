namespace DanmakuEngine.Logging;

public class Logger
{
    private static readonly Logger _instence = null!;

    private static readonly object _synclog = new();
    private static readonly object _syncqueue = new();

    private static readonly string log_file;

    private const string game = @"DanmakuEngine";

    public static Logger GetLogger() => _instence;

    public static void Info(string message) => Log(message, LogLevel.Info);

    public static void Verbose(string message) => Log(message, LogLevel.Verbose);

    public static void Debug(string message) => Log(message, LogLevel.Debug);

    public static void Warning(string message) => Log(message, LogLevel.Warning);

    public static void Error(string message) => Log(message, LogLevel.Error);

    public static void Log(string message) => Log(message, null);

    private static void Log(string message, LogLevel? logLevel = null!)
    {
        var logger = GetLogger();

        System.Diagnostics.Debug.Assert(logger != null);

        logLevel ??= logger.logLevel;

        var log = new Log(DateTime.UtcNow, message, logLevel.Value);

        PrintLog(log);

        lock (_synclog)
        {
            _instence.logs.AddLast(log);
        }

        lock (_syncqueue)
        {
            _instence.pendingWrite.Enqueue(log);
        }

        QueueAsyncSave();
    }

    private static int last_save = -1;
    private static void QueueAsyncSave()
    {
        var this_save = getTimeStamp();

        if (this_save != last_save)
        {
            PeriodSave();

            return;
        }

        Task.Delay(100).ContinueWith(_ =>
        {
            if (this_save == last_save)
                return;

            PeriodSave();

            last_save = getTimeStamp();
        });
    }

    private static readonly Dictionary<LogLevel, Action> colorMap = new()
    {
        {LogLevel.Error, () => Console.ForegroundColor = ConsoleColor.Red},
        {LogLevel.Warning, () => Console.ForegroundColor = ConsoleColor.Yellow},
        {LogLevel.Debug, () => Console.ForegroundColor = ConsoleColor.Green},
        {LogLevel.Verbose, () => Console.ResetColor()},
        {LogLevel.Info, () => Console.ForegroundColor = ConsoleColor.DarkGray}
    };

    private static void PrintLog(Log log)
    {
        colorMap[log.level].Invoke();

        Console.WriteLine(log.ToString());

        // TODO: Whether we should reset the color
        // Only needed when we develop the TUI debugging
        // However this causes performance issue.
        // Maybe we should do it asyncly
    }

    private static void PeriodSave()
    {
        lock (_syncqueue)
        {
            var pendings = _instence.pendingWrite;

            using var fs = new StreamWriter(log_file, true);
            while (pendings.TryDequeue(out Log log))
            {
                fs.WriteLine(log);
            }
        }
    }

    private static readonly DateTime begin = new(1970, 1, 1, 0, 0, 0, 0);

    /// <summary>
    /// actually it's every 100ms
    /// </summary>
    /// <returns></returns>
    private static int getTimeStamp()
        => (int)((DateTime.UtcNow - begin).TotalSeconds * 10);

    public static void SetLogLevel(LogLevel logLevel) => _instence.logLevel = logLevel;

    /// <summary>
    /// Default log level if not specified.
    /// </summary>
    private LogLevel logLevel = LogLevel.Verbose;

    private LinkedList<Log> logs = new();

    private Queue<Log> pendingWrite = new();

    static Logger()
    {
        _instence = new Logger();

        log_file = $"{game}-{DateTime.Now:yy-MM-dd-HH:mm}.log";
    }
}

public readonly struct Log
{
    public readonly DateTime time;

    public readonly LogLevel level;

    public readonly string message;

    public Log(DateTime time, string message, LogLevel level)
    {
        this.time = time;
        this.message = message;
        this.level = level;
    }

    public override string ToString() => $"{time:yy-MM-dd HH:mm:ss} [{level}]: {message}";
}

// Converter
public enum LogLevel
{
    Error,
    Warning,
    Debug,
    Verbose,
    Info
}