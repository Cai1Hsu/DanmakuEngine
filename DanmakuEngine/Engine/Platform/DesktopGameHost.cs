using System.Runtime.InteropServices;

namespace DanmakuEngine.Engine.Platform;

public class DesktopGameHost : GameHost
{
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsUnix => IsLinux || IsMacOS;

    public static DesktopGameHost GetSuitableHost()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsGameHost();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxGameHost();

        // Since openGL on macOS is deprecated,
        // we will not support it currently.

        throw new PlatformNotSupportedException("The current platform is not supported.");
    }
}