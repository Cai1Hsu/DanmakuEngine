using System.Runtime.InteropServices;
using DanmakuEngine.Engine.Platform.Linux;
using DanmakuEngine.Engine.Platform.Windows;

namespace DanmakuEngine.Engine.Platform;

public class DesktopGameHost : GameHost
{
    public static DesktopGameHost GetSuitableHost()
    {
        if (RuntimeInfo.IsWindows)
            return new WindowsGameHost();

        if (RuntimeInfo.IsLinux)
            return new LinuxGameHost();

        // Since openGL on macOS is deprecated,
        // we will not support it currently.

        throw new PlatformNotSupportedException("The current platform is not supported.");
    }
}
