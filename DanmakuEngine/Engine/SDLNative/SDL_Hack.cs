using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using DanmakuEngine.Engine.Platform;
using DanmakuEngine.Logging;
using Silk.NET.SDL;
using SDL_DisplayMode = Silk.NET.SDL.DisplayMode;
using SDL_FullscreenMode = Veldrid.Sdl2.SDL_FullscreenMode;
using SDL_WindowFlags = Silk.NET.SDL.WindowFlags;

namespace DanmakuEngine.Engine.SDLNative;

internal static unsafe partial class SDL_Hack
{
    private static SDL_VideoDevice* getVideoDevice(void* p_magic)
    {
        SDL_VideoDevice device = default;

        int offset = (int)((nint)(&device.window_magic) - (nint)(&device));

        // var offset = Marshal.OffsetOf<SDL_VideoDevice>("window_magic");

        var p_device = (SDL_VideoDevice*)(((IntPtr)p_magic) - offset);

        var ndisplays = p_device->num_displays;

        // Make sure we pointed to the correct memory. 10 displays should be enough for everyone
        // Although this is not a gurantee
        Debug.Assert(ndisplays < 10);

        return p_device;
    }

    internal static void FixSdlDisplayModes(void* p_magic, int displayIndex, IList<SDL_DisplayMode> corrected)
    {
        var p_device = getVideoDevice(p_magic);

        var p_display = &p_device->displays[displayIndex];

        int nmodes = corrected.Count;

        SDL_DisplayMode* modes = p_display->display_modes;

        // Since some display modes were discared due to the incorrectly calculated refresh rate
        // we have to realloc the display modes and sort them again

        // step1: realloc the display modes
        // enlarge display->display_modes by 32 * k
        if (nmodes > p_display->max_display_modes)
        {
            // find the k
            int k = 1;
            while (nmodes > p_display->max_display_modes + (32 * k))
                k++;

            var new_count = p_display->max_display_modes + (32 * k);

            // realloc the modes
            modes = (SDL_DisplayMode*)Marshal.ReAllocHGlobal((IntPtr)modes,
                                    new_count * sizeof(SDL_DisplayMode));

            p_display->display_modes = modes;
            p_display->max_display_modes += 32 * k;
        }

        // step2: copy and sort the display modes
        var copy = corrected.ToList();
        copy.Sort(cmpmodes);

        // step3: replace the display modes
        for (int i = 0; i < nmodes; i++)
            p_display->display_modes[i] = copy[i];

        // step4: change the number of display modes
        p_display->num_display_modes = nmodes;
    }

    private static unsafe int cmpmodes(SDL_DisplayMode A, SDL_DisplayMode B)
    {
        SDL_DisplayMode* a = &A, b = &B;

        if (a == b)
        {
            return 0;
        }
        else if (a->W != b->W)
        {
            return b->W - a->W;
        }
        else if (a->H != b->H)
        {
            return b->H - a->H;
        }
        else if (SDL_BITSPERPIXEL(a->Format) != SDL_BITSPERPIXEL(b->Format))
        {
            return (int)(SDL_BITSPERPIXEL(b->Format) - SDL_BITSPERPIXEL(a->Format));
        }
        else if (SDL_PIXELLAYOUT(a->Format) != SDL_PIXELLAYOUT(b->Format))
        {
            return (int)(SDL_PIXELLAYOUT(b->Format) - SDL_PIXELLAYOUT(a->Format));
        }
        else if (a->RefreshRate != b->RefreshRate)
        {
            return b->RefreshRate - a->RefreshRate;
        }
        else if (a->Driverdata != b->Driverdata)
        {
            // Not sure if we should compare the address directly
            // but it's definitely better than nothing
            return (int)((nint)b->Driverdata - (nint)a->Driverdata);
        }
        return 0;

        static uint SDL_PIXELLAYOUT(uint X) => ((X) >> 16) & 0x0F;
        static uint SDL_BITSPERPIXEL(uint X) => ((X) >> 8) & 0xFF;
    }
}
