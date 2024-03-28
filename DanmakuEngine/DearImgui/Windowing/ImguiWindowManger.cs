namespace DanmakuEngine.DearImgui.Windowing;

public static class ImguiWindowManager
{
    private static LinkedList<ImguiWindowBase> _windows = [];

    internal static void RegisterWindow(ImguiWindowBase window)
        => _windows.AddLast(window);

    internal static void UnregisterWindow(ImguiWindowBase window)
        => _windows.Remove(window);

    internal static void UpdateWindows()
    {
        foreach (var window in _windows)
            window.UpdateUI();
    }
}
