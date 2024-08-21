using System.Runtime.Versioning;
using DanmakuEngine.Allocations;
using DanmakuEngine.Engine.Platform.Environments;
using DanmakuEngine.Engine.Platform.Environments.Xdg;
using DanmakuEngine.Engine.Platform.Linux.X11;
using DanmakuEngine.Engine.SDLNative;
using DanmakuEngine.Extensions;
using DanmakuEngine.Logging;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SDL_DisplayMode = Silk.NET.SDL.DisplayMode;
using SDL_Window = DanmakuEngine.Engine.SDLNative.SDL_Window;
using WMType = Silk.NET.SDL.SysWMType;

namespace DanmakuEngine.Engine.Platform.Linux;

public unsafe partial class LinuxGameHost : DesktopGameHost
{
    private LinuxGraphicsPlatform? _graphicsPlatform;

    /// <summary>
    /// The graphics platform that the game is running on.
    /// </summary>
    public LinuxGraphicsPlatform GraphicsPlatform
    {
        get
        {
            if (!_graphicsPlatform.HasValue)
                _graphicsPlatform = Env.Get<XdgSessionTypeEnv, LinuxGraphicsPlatform>()!;

            return _graphicsPlatform.Value;
        }
    }

    [SupportedOSPlatform("linux")]
    protected override IList<SDL_DisplayMode> GetDisplayModes(int displayIndex)
    {
        switch (window.WMInfo.Subsystem)
        {
            // Even if you are running on wayland, SDL2 will still return X11 if SDL_VIDEODRIVER was not set to wayland
            case WMType.Wayland:
                return base.GetDisplayModes(displayIndex);

            default:
            {
                if (GraphicsPlatform is LinuxGraphicsPlatform.Wayland)
                    goto case WMType.Wayland;

                ArgumentOutOfRangeException
                    .ThrowIfNegative(displayIndex, nameof(displayIndex));

                var sdl = SDL.Api;

                var display_count = sdl.GetNumVideoDisplays();

                ArgumentOutOfRangeException
                    .ThrowIfGreaterThanOrEqual(displayIndex, display_count, nameof(displayIndex));

                // SDL requires all windows and all display modes use the same format
                // so we just copy the first display mode's format
                // see https://github.com/libsdl-org/SDL/blob/SDL2/src/video/x11/SDL_x11modes.c#L662-L667
                SDL_DisplayMode mode = default;
                SDL.Api.GetDisplayMode(displayIndex, 0, ref mode);

                var modes = getDisplayModesX11(displayIndex, mode.Format);

                var magic = ((SDL_Window*)window.Window)->magic;

                SDL_Hack.FixSdlDisplayModes(magic, displayIndex, modes);

#if DEBUG
                base.GetDisplayModes(displayIndex);
#endif

                return modes;
            }
        }
    }

    private IList<SDL_DisplayMode> getDisplayModesX11(int displayIndex, uint format)
    {
        // in SDL2, they don't check if the DisplayMode we passed,
        // so we can directly send our correct data without having to hack the SDL2's memory
        /* in SDL2/SDL_video.c:
            int SDL_SetWindowDisplayMode(SDL_Window *window, const SDL_DisplayMode *mode)
            {
                CHECK_WINDOW_MAGIC(window, -1);

                if (mode) {
                    window->fullscreen_mode = *mode;
                }
                // other code
            }
        */

        // as for driverdata, it's basically a pointer to <DisplayModeData> which contains the modeID.

        IList<SDL_DisplayMode> modes = [];

        using (var display = new X11Display(displayIndex == 0 ? null : SDL.Api.GetDisplayNameS(displayIndex)))
        using (var screen = new X11Screen(display))
        {
            for (var i = 0; i < screen.Resources->noutput; i++)
            {
                var output_info = Xrandr.XRRGetOutputInfo(display, screen.Resources, screen.Resources->outputs[i]);
                if (output_info != null)
                {
                    if (output_info->connection == 1)
                        continue;

                    for (int j = 0; j < screen.Resources->nmode; j++)
                    {
                        var mode = new SDL_DisplayMode
                        {
                            Format = format,
                            Driverdata = (SDL_DisplayModeData*)NativeMemory.Alloc((nuint)sizeof(SDL_DisplayModeData)),
                        };

                        if (!setXRandRModeInfo(display, screen, output_info->crtc,
                            output_info->modes[j], ref mode))
                        {
                            NativeMemory.Free(mode.Driverdata);
                            continue;
                        }

                        modes.Add(mode);
                    }
                }
                Xrandr.XRRFreeOutputInfo(output_info);
            }
        }

        return modes;
    }

    private static bool setXRandRModeInfo(X11Display display, X11Screen screen, ulong crtc,
                                  ulong modeID, ref SDL_DisplayMode mode)
    {
        for (int i = 0; i < screen.Resources->nmode; i++)
        {
            XRRModeInfo* info = &screen.Resources->modes[i];

            if (info->id != modeID)
                continue;

            ushort rotation = 0;
            int scale_w = 0x10000, scale_h = 0x10000;

            XRRCrtcInfo* crtcinfo = Xrandr.XRRGetCrtcInfo(display, screen.Resources, crtc);
            if (crtcinfo != null)
            {
                rotation = crtcinfo->rotation;
                Xrandr.XRRFreeCrtcInfo(crtcinfo);
            }

            XRRCrtcTransformAttributes* attr;
            if (Xrandr.XRRGetCrtcTransform(display, crtc, &attr) != 0 && attr != null)
            {
                scale_w = attr->currentTransform.matrix[0][0];
                scale_h = attr->currentTransform.matrix[1][1];
                Xlib.XFree(attr);
            }

            if ((rotation & (XRANDR_ROTATION_LEFT | XRANDR_ROTATION_RIGHT)) != 0)
            {
                mode.W = (int)(((info->height * scale_w) + 0xffff) >> 16);
                mode.H = (int)(((info->width * scale_h) + 0xffff) >> 16);
            }
            else
            {
                mode.W = (int)(((info->width * scale_w) + 0xffff) >> 16);
                mode.H = (int)(((info->height * scale_h) + 0xffff) >> 16);
            }

            mode.RefreshRate = (int)Math.Round(calculateXRandRRefreshRate(info));

#pragma warning disable CA1416
            ((SDL_DisplayModeData*)mode.Driverdata)->xrandr_mode = modeID;
#pragma warning restore CA1416
            return true;
        }

        return false;
    }

#pragma warning disable IDE1006
    private const int RR_Interlace = 0x00000010;
    private const int RR_DoubleScan = 0x00000020;

    private const int XRANDR_ROTATION_LEFT = 1 << 1;
    private const int XRANDR_ROTATION_RIGHT = 1 << 3;
#pragma warning restore IDE1006

    private static float calculateXRandRRefreshRate(XRRModeInfo* info)
    {
        double vTotal = info->vTotal;

        if ((info->modeFlags & RR_DoubleScan) != 0)
        {
            /* doublescan doubles the number of lines */
            vTotal *= 2;
        }

        if ((info->modeFlags & RR_Interlace) != 0)
        {
            /* interlace splits the frame into two fields */
            /* the field rate is what is typically reported by monitors */
            vTotal /= 2;
        }

        if (info->hTotal == 0 || vTotal == 0)
            return 0.0f;

        return (float)(100 * (long)info->dotClock / (info->hTotal * vTotal) / 100.0f);
    }
}
