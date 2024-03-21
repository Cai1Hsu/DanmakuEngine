using System.Runtime.InteropServices;

using SDL_DisplayMode = Silk.NET.SDL.DisplayMode;
using SDL_DisplayOrientation = Silk.NET.SDL.DisplayOrientation;

namespace DanmakuEngine.Engine.SDLNative;

/// <summary>
/// SDL's internal structure for video display.
/// Do not use this unless you know what you are doing.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct SDL_VideoDisplay
{
    public char* name;
    public int max_display_modes;
    public int num_display_modes;
    public SDL_DisplayMode* display_modes;
    public SDL_DisplayMode desktop_mode;
    public SDL_DisplayMode current_mode;
    public SDL_DisplayOrientation orientation;

    public SDL_Window* fullscreen_window;

    /// <summary>
    /// Although this points to the actual video device in SDL, 
    /// we didn't implement SDL_VideoDevice completely, so please don't convert this to our `VideoDevice` struct.
    /// </summary>
    public void* device;

    public void* driverdata;
}
