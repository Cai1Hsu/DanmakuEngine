using System.Collections.Frozen;
using DanmakuEngine.Configuration;
using DanmakuEngine.Dependency;

namespace DanmakuEngine.Logging;

public partial class Logger : IInjectable, IAutoloadable
{
    [Inject]
    private ConfigManager ConfigManager = null!;

    private static readonly Logger _instance = null!;

    private static readonly object _synclog = new();
    private static readonly object _syncqueue = new();
    private static readonly object _syncconsole = new();

    private static string log_directory;

    private static readonly string log_file;

    private static string FullLogFile => Path.Combine(log_directory, log_file);

    private const string game = @"DanmakuEngine";

    private LogLevel printLevel = LogLevel.Verbose;

    public static Logger GetLogger() => _instance;

    public static void SetPrintLevel(LogLevel logLevel)
    {
        _instance.printLevel = logLevel;

        Logger.Debug($"Print level updated, current: {_instance.printLevel}");
    }

    public static void SetLogDirectory(string directory)
    {
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"{directory}");

        log_directory = directory;
    }

    public static void Info(string message) => Log(message, LogLevel.Info);

    public static void Verbose(string message) => Log(message, LogLevel.Verbose);

    public static void Debug(string message) => Log(message, LogLevel.Debug);

    public static void Warn(string message) => Log(message, LogLevel.Warning);

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
            _instance.logs.AddLast(log);
        }

        lock (_syncqueue)
        {
            _instance.pendingWrite.Enqueue(log);
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

    private static readonly FrozenDictionary<LogLevel, Action> colorMap = new Dictionary<LogLevel, Action>()
    {
        {LogLevel.Error, () => Console.ForegroundColor = ConsoleColor.Red},
        {LogLevel.Warning, () => Console.ForegroundColor = ConsoleColor.Yellow},
        {LogLevel.Debug, () => Console.ForegroundColor = ConsoleColor.Green},
        {LogLevel.Verbose, () => Console.ResetColor()},
        {LogLevel.Info, () => Console.ForegroundColor = ConsoleColor.DarkGray}
    }.ToFrozenDictionary();

    private static bool defaultcolor = true;
    private static object _synccolor = new();

    private static void PrintLog(Log log)
    {
        if (!ConfigManager.HasConsole)
            return;

        if (log.level < _instance.printLevel)
            return;

        lock (_synccolor)
        {
            colorMap[log.level].Invoke();

            defaultcolor = false;

            Write(log.ToString(), false, true);

            Console.ResetColor();
            defaultcolor = true;
        }
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

    private static void Write(string str) => Console.Write(str);

    private static void WriteLine(string str) => Console.WriteLine(str);

    private static void PeriodSave()
    {
        lock (_syncqueue)
        {
            var pendings = _instance.pendingWrite;

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
        _instance.logLevel = logLevel;

        Logger.Log($"Log level updated, current: {_instance.logLevel}");
    }

    public void OnInjected()
    {
        if (ConfigManager == null)
        {
            Logger.Debug("ConfigManager is not injected, DebugMode is always enabled");
        }
    }

    /// <summary>
    /// Default log level if not specified.
    /// </summary>
    private LogLevel logLevel = LogLevel.Verbose;

    private LinkedList<Log> logs = new();

    private Queue<Log> pendingWrite = new();

    static Logger()
    {
        _instance = new Logger();

        log_directory = Environment.CurrentDirectory;

        // Generate a 4 digit unique id
        // This prevents the log file from being overwritten
        var unique_id = Guid.NewGuid().ToString().Substring(0, 4);

        log_file = $"{game}-{DateTime.Now:yy-MM-dd-HH-mm}-{unique_id}.log";

        Logger.Debug($"Log file: {FullLogFile}");

        var ci_env = Environment.GetEnvironmentVariable("IS_CI_ENVIRONMENT");

        if (ci_env is not null
            && bool.TryParse(ci_env, out var is_ci_env) && is_ci_env)
        {
            // Disable console output in CI environment
            SetPrintLevel(LogLevel.Silent);
        }
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
    Silent = 0x7fffffff,
    Error = 5,
    Warning = 4,
    Debug = 3,
    Verbose = 2,
    Info = 1,
}
