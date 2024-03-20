using System.Runtime.InteropServices;

namespace DanmakuEngine.Engine.Platform.Linux.X11;

[StructLayout(LayoutKind.Sequential)]
public struct DisplayModeData
{
    public ulong xrandr_mode;
}