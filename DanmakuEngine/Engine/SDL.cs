using DanmakuEngine.Logging;
using Silk.NET.SDL;

namespace DanmakuEngine.Engine;

public static class SDL
{
    private static Sdl? _sdl = null;

    public static bool IsInitialized => _sdl is not null;

    public static Sdl Api
    {
        get
        {
            if (_sdl is not null)
                return _sdl;

            try
            {
                return _sdl = Sdl.GetApi();
            }
            catch (Exception e)
            {
                Logger.Error($"Error happened when trying to init SDL: {e.Message}");

                throw;
            }
        }
    }

    public static void Quit()
    {
        _sdl?.Quit();

        _sdl = null;
    }
}
