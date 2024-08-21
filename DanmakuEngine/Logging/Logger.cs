using System.Collections.Frozen;
using DanmakuEngine.Configuration;
using DanmakuEngine.Dependency;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Logging;

public partial class Logger : IInjectable, IAutoloadable
{
    [Inject]
    private ConfigManager ConfigManager = null!;

    private static readonly Logger instance;

    private static readonly object syncqueue = new();
    private static readonly object syncconsole = new();

    private static string log_directory;

    private static readonly string log_file;

    private static string fullLogFile => Path.Combine(log_directory, log_file);

    private const string game = @"DanmakuEngine";

    private LogLevel printLevel = LogLevel.Verbose;

    public static Logger GetLogger() => instance;

    public static void SetPrintLevel(LogLevel logLevel)
    {
        instance.printLevel = logLevel;

        Logger.Debug($"Print level updated, current: {instance.printLevel}");
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

        logLevel ??= logger.logLevel;

        var log = new Log(DateTime.UtcNow, message, logLevel.Value);

        printLog(log);

        lock (syncqueue)
        {
            instance.pendingWrite.Enqueue(log);
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

    private static void printLog(Log log)
    {
        if (!ConfigManager.HasConsole)
            return;

        if (log.level < instance.printLevel)
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

        lock (syncconsole)
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
        lock (syncqueue)
        {
            var pendings = instance.pendingWrite;

            using var fs = new StreamWriter(fullLogFile, true);
            while (pendings.TryDequeue(out Log log))
            {
                fs.WriteLine(log);
            }
        }
    }

    /// <summary>
    /// actually it's every 100ms
    /// </summary>
    /// <returns></returns>
    private static int getTimeStamp()
        => (int)(Time.AppTimer.ElapsedMilliseconds * 100);

    public static void SetLogLevel(LogLevel logLevel)
    {
        instance.logLevel = logLevel;

        Logger.Log($"Log level updated, current: {instance.logLevel}");
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

    private Queue<Log> pendingWrite = new();

    static Logger()
    {
        instance = new Logger();

        log_directory = Path.Combine(Environment.CurrentDirectory, "logs");

        if (!Directory.Exists(log_directory))
            Directory.CreateDirectory(log_directory);

        // Generate a 4 digit unique id
        // This prevents the log file from being overwritten
        var unique_id = Guid.NewGuid().ToString().Substring(0, 4);

        log_file = $"{game}-{DateTime.Now:yy-MM-dd-HH-mm}-{unique_id}.log";

        Logger.Debug($"Log file: {fullLogFile}");

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

    public override string ToString()
#if DEBUG
        => $"{time:yy-MM-dd HH:mm:ss}.{time.Millisecond:D3} [{level}]: {message}";
#else
        => $"{time:yy-MM-dd HH:mm:ss} [{level}]: {message}";
#endif
}

public enum LogLevel : byte
{
    Silent = 1 << ((sizeof(byte) * 8) - 1),
    Error = 1 << 4,
    Warning = 1 << 3,
    Verbose = 1 << 2,
    Debug = 1 << 1,
    Info = 1,
}
