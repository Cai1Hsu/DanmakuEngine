using System.Runtime.InteropServices;

namespace DanmakuEngine.Engine.Platform;

public static class RuntimeInfo
{
#pragma warning disable IDE1006
    public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static readonly bool IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static readonly bool IsUnix = IsLinux || IsMacOS;
#pragma warning restore IDE1006
}
