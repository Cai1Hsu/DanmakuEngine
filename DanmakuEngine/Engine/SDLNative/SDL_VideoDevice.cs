using System.Runtime.InteropServices;

namespace DanmakuEngine.Engine.SDLNative;

/// <summary>
/// We didn't implement SDL_VideoDevice completely
/// Don't use this unless you know what you're doing.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct SDL_VideoDevice
{
    public int num_displays;
    public SDL_VideoDisplay* displays;
    public SDL_Window* windows;
    public SDL_Window* grabbed_window;
    public byte window_magic;
}
