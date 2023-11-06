using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace DanmakuEngine.Games.Platform;

public class WindowsGameHost : DesktopGameHost
{
    [DllImport("kernel32.dll")]
    static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    public static bool HasConsole()
    {
        if (!IsWindows)
            return true;

        return GetConsoleWindow() != IntPtr.Zero;
    }

    public static void CreateConsole()
    {
        if (GetConsoleWindow() != IntPtr.Zero)
            return;

        if (Configuration.ConfigManager.DebugBuild)
            return;

        if (!IsWindows)
            return;

        AllocConsole();

        Configuration.ConfigManager.UpdateConsoleStatus();
    }

    protected override void SetUpConsole()
    {
        if (!Configuration.ConfigManager.HasConsole)
            return;

        Console.CursorVisible = false;

        // TODO: Duplicate? 
        Console.CancelKeyPress += (_, e) =>
            window.IsClosing = e.Cancel = true;
        
        base.SetUpConsole();
    }
}