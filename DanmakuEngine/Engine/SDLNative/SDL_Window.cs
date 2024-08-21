using System.Runtime.InteropServices;
using Silk.NET.SDL;

using SDL_Surface = Silk.NET.SDL.Surface;

namespace DanmakuEngine.Engine.SDLNative;

/// <summary>
/// Do not change the value of this struct unless you know what you're doing.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct SDL_Window
{
    public readonly void* magic;
    public uint id;
    public char* title;
    public SDL_Surface* icon;
    public int x, y;
    public int w, h;
    public int min_w, min_h;
    public int max_w, max_h;
    public uint flags;
    public uint last_fullscreen_flags;
    public uint display_index;

    /* Stored position and size for windowed mode */
    public SDL_Rect windowed;

    public DisplayMode fullscreen_mode;

    public float opacity;

    public float brightness;
    public ushort* gamma;

    // just offset into gamma
    public ushort* saved_gamma;

    public SDL_Surface* surface;

    public SdlBool surface_valid;

    public SdlBool is_hiding;
    public SdlBool is_destroying;
    public SdlBool is_dropping; /* drag/drop in progress, expecting SDL_SendDropComplete(). */

    public SDL_Rect mouse_rect;
}
