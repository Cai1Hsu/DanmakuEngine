using System.Runtime.InteropServices;

namespace DanmakuEngine.Engine.SDLNative;

[StructLayout(LayoutKind.Sequential)]
public struct SDL_DisplayModeData
{
    public ulong xrandr_mode;
}