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

    // Since SDL_Delay provides high resolution(at least on Linux), we use it as default
    // On Windows, although SDL_Delay provides almost same resolution as Windows FILETIME, it's has better capability, while FILETIME provides lower standard deviation.
    // So we use SDL_Delay as a fallback.
    private static bool _preferSDL = false;

    public static bool PreferSDL
    {
        get => _preferSDL;
        set
        {
            if (_preferSDL == value)
                return;

            _preferSDL = value;

            if (_preferSDL)
            {
                _platformInstance = null!;

                _sdlInstance ??= new SDLWaitHandler();
            }
            else
            {
                if (_platformInstance is SDLWaitHandler)
                {
                    _platformInstance = Create(false);
                }
            }
        }
    }

    private static IWaitHandler? _platformInstance;

    private static SDLWaitHandler _sdlInstance = null!;

    public static SDLWaitHandler SDLWaitHandler => _sdlInstance;

    public static IWaitHandler WaitHandler
    {
        get
        {
            if (_platformInstance is not null)
                return _platformInstance;

            if (_sdlInstance is not null)
                return _sdlInstance;

            return Create(PreferSDL);
        }
    }

    public static IWaitHandler Create(bool preferSDL = false)
    {
        _sdlInstance ??= new SDLWaitHandler();

        if (preferSDL)
            return _platformInstance = _sdlInstance;

        if (_platformInstance is not null)
            return _platformInstance;

        if (RuntimeInfo.IsWindows)
        {
            _platformInstance = new WindowsWaitHandler();
            _platformInstance.Register();

            if (WaitHandler.IsHighResolution)
                return _platformInstance;

            _platformInstance = null!;
        }
        else if (RuntimeInfo.IsLinux)
        {
            _platformInstance = new LinuxWaitHandler();
        }

        return _sdlInstance = new SDLWaitHandler();
    }
}
