using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace DanmakuEngine.Engine.Platform.Windows;

public class WindowsGameHost : DesktopGameHost
{
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [SupportedOSPlatform("Windows")]
    public new static bool HasConsole()
    {
        if (!RuntimeInfo.IsWindows)
            throw new InvalidOperationException("This method is only supported on Windows");

        return GetConsoleWindow() != IntPtr.Zero;
    }

    [SupportedOSPlatform("Windows")]
    public static void CreateConsole()
    {
        if (GetConsoleWindow() != IntPtr.Zero)
            return;

        if (Configuration.ConfigManager.DebugBuild)
            return;

        if (!RuntimeInfo.IsWindows)
            return;

        AllocConsole();

        Configuration.ConfigManager.UpdateConsoleStatus(true);
    }

    protected override void SetUpDebugConsole()
    {
        if (!Configuration.ConfigManager.HasConsole)
            return;

        base.SetUpDebugConsole();
    }
}
