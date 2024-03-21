using System.Runtime.InteropServices;
using DanmakuEngine.Engine.Platform;
using DanmakuEngine.Logging;
using Silk.NET.SDL;
using Veldrid.Sdl2;

namespace DanmakuEngine.Engine.SDLNative;

public static unsafe partial class SDL_Hack
{
    public static bool SetWindowDisplayMode(SDL_Window* window, DisplayMode* mode, bool force = false)
    {
        var sdl = SDL.Api;

        sdl.SetWindowDisplayMode((Window*)window, mode);

        if (!force)
            return true;

        if (!RuntimeInfo.IsLinux)
            return true;

        DisplayMode applied = default;
        sdl.GetWindowDisplayMode((Window*)window, &applied);

        // On Linux, we have to do some hacking to ensure the XID is updated
        var target_xid = ((SDL_DisplayModeData*)mode->Driverdata)->xrandr_mode;
        if (((SDL_DisplayModeData*)applied.Driverdata)->xrandr_mode == target_xid)
            return true;

        ((SDL_DisplayModeData*)window->fullscreen_mode.Driverdata)->xrandr_mode = target_xid;

        // SDL_GetWindowDisplayMode()
        /*
            int SDL_GetWindowDisplayMode(SDL_Window *window, SDL_DisplayMode *mode)
            {
                // assign the current display mode to the window and do some checking

                // if in desktop size mode, just return the size of the desktop
                if ((window->flags & SDL_WINDOW_FULLSCREEN_DESKTOP) == SDL_WINDOW_FULLSCREEN_DESKTOP) {
                    fullscreen_mode = display->desktop_mode;
                } else if (!SDL_GetClosestDisplayModeForDisplay(SDL_GetDisplayForWindow(window),
                                                                &fullscreen_mode,
                                                                &fullscreen_mode)) {
                    SDL_zerop(mode);
                    return SDL_SetError("Couldn't find display mode match");
                }
                *mode = fullscreen_mode;
            }
        */

        // SDL ingores our changes and uses SDL_GetClosestDisplayModeForDisplay() to get the display mode
        // However if the user added a custom mode, it's XID is always in the last position
        // This means SDL still uses the old mode(incorrect one).

        // Check that if the window is fullscreen
        // if so, we have to toggle it off and on again to trigger the change

        if ((sdl.GetWindowFlags((Window*)window)
            & (uint)SDL_WindowFlags.Fullscreen) == (uint)SDL_WindowFlags.Fullscreen)
        {
            sdl.SetWindowFullscreen((Window*)window, (uint)SDL_FullscreenMode.Windowed);
            sdl.SetWindowFullscreen((Window*)window, (uint)SDL_FullscreenMode.Fullscreen);
        }

        // Let's check that if our hack worked
        sdl.GetWindowDisplayMode((Window*)window, &applied);

        var applied_XID = ((SDL_DisplayModeData*)applied.Driverdata)->xrandr_mode;

        Logger.Debug($"[SDL_Hack]Applied display mode: {applied.W}x{applied.H}@{applied.RefreshRate} XID: {applied_XID} Target XID: {target_xid}");

        return applied_XID == target_xid;
    }
}
