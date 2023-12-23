using DanmakuEngine.Engine.Platform;
using DanmakuEngine.Engine.Platform.Linux;
using DanmakuEngine.Engine.Platform.Windows;

namespace DanmakuEngine.Engine.Sleeping;

public interface IWaitHandler
{
    public void Register();

    public void Wait(double milliseconds);

    public void Wait(TimeSpan timeSpan);

    public bool IsHighResolution { get; }

    private static IWaitHandler? _instance;

    public static IWaitHandler WaitHandler
    {
        get
        {
            _instance ??= Create();

            return _instance;
        }
    }

    public static IWaitHandler Create(bool ForceUseSpin = false)
    {
        if (_instance is not null)
            return _instance;

        if (ForceUseSpin)
            _instance = new SpinWaitHandler();
        else
        {
            if (DesktopGameHost.IsWindows)
                _instance = new WindowsWaitHandler();

            if (DesktopGameHost.IsLinux)
                _instance = new LinuxWaitHandler();
        }

        // we need to try registering, so that we can check if high resolution is available on Windows
        _instance?.Register();

        // if high resolution is not available, we will fall back to SpinWaitHandler
        if (_instance is null ||
            (!_instance.IsHighResolution && _instance is WindowsWaitHandler))
        {
            // don't need this since we didn't create a waitable timer successfully
            // w.Unregister();

            _instance = new SpinWaitHandler();
            _instance.Register();
        }

        return _instance;
    }
}
