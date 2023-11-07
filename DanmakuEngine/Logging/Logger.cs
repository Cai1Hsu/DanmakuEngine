using DanmakuEngine.Configuration;
using DanmakuEngine.Dependency;

namespace DanmakuEngine.Logging;

public class Logger : IInjectable
{
    [Inject]
    private ConfigManager ConfigManager = null!;

    private static readonly Logger _instence = null!;

    private static readonly object _synclog = new();
    private static readonly object _syncqueue = new();
    private static readonly object _syncconsole = new();

    private static string log_directory;

    private static readonly string log_file;

    private static string FullLogFile => Path.Combine(log_directory, log_file);

    private const string game = @"DanmakuEngine";

    public static Logger GetLogger() => _instence;

    public static void SetLogDirectory(string directory)
    {
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"{directory}");

        log_directory = directory;
    }

    public static void Info(string message) => Log(message, LogLevel.Info);

    public static void Verbose(string message) => Log(message, LogLevel.Verbose);

    public static void Debug(string message)
    {
        var logger = GetLogger();

        if (!ConfigManager.DebugBuild)
            return;

        if (logger.ConfigManager != null
            && !logger.ConfigManager.DebugMode)
            return;

        Log(message, LogLevel.Debug);
    }

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

    private static bool defaultcolor = true;
    private static object _synccolor = new();

    private static void PrintLog(Log log)
    {
        if (!ConfigManager.HasConsole)
            return;

        lock (_synccolor)
        {
            colorMap[log.level].Invoke();

            defaultcolor = false;

            Write(log.ToString(), false, true);
        }

        // TODO: Whether we should reset the color
        // Only needed when we develop the TUI debugging
        // However this causes performance issue.
        // Maybe we should do it async
    }

    public static void Write(string str, bool resetColor = false, bool writeLine = false)
    {
        if (!ConfigManager.HasConsole)
            return;

        lock (_syncconsole)
        {
            if (resetColor && !defaultcolor)
            {
                lock (_synccolor)
                {
                    Console.ResetColor();

                    defaultcolor = true;
                }
            }

            if (writeLine)
                WriteLine(str);
            else
                Write(str);
        }
    }

    public static void Write(string str) => Console.Write(str);

    public static void WriteLine(string str) => Console.WriteLine(str);

    private static void PeriodSave()
    {
        lock (_syncqueue)
        {
            var pendings = _instence.pendingWrite;

            using var fs = new StreamWriter(FullLogFile, true);
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

    public static void SetLogLevel(LogLevel logLevel)
    {
        _instence.logLevel = logLevel;

        Logger.Log($"Log level updated, current: {_instence.logLevel}");
    }

    /// <summary>
    /// Default log level if not specified.
    /// </summary>
    private LogLevel logLevel = LogLevel.Verbose;

    private LinkedList<Log> logs = new();

    private Queue<Log> pendingWrite = new();

    static Logger()
    {
        _instence = new Logger();

        log_directory = Environment.CurrentDirectory;
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