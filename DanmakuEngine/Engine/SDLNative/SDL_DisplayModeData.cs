using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace DanmakuEngine.Engine.SDLNative;

[SupportedOSPlatform("linux")]
[StructLayout(LayoutKind.Sequential)]
internal struct SDL_DisplayModeData
{
    public ulong xrandr_mode;
}
