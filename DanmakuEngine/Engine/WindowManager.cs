using Silk.NET.SDL;

namespace DanmakuEngine.Engine;

public class WindowManager
{
    private Window window;
    public WindowManager(Window window)
    {
        this.window = window;
    }

    public void HandleWindowEvent(WindowEvent windowEvent)
    {
        switch ((WindowEventID)windowEvent.Event)
        {
            case WindowEventID.Shown:
                WindowShown?.Invoke();
                break;

            case WindowEventID.Hidden:
                WindowHidden?.Invoke();
                break;

            case WindowEventID.Exposed:
                WindowExposed?.Invoke();
                break;

            case WindowEventID.Moved:
                WindowMoved?.Invoke(windowEvent.Data1, windowEvent.Data2);
                break;

            case WindowEventID.Resized:
                WindowResized?.Invoke(windowEvent.Data1, windowEvent.Data2);
                break;

            // This handles both Resized and SizeChanged
            case WindowEventID.SizeChanged:
                WindowSizeChanged?.Invoke(windowEvent.Data1, windowEvent.Data2);
                break;

            case WindowEventID.Minimized:
                WindowMinimized?.Invoke();
                break;

            case WindowEventID.Maximized:
                WindowMaximized?.Invoke();
                break;

            case WindowEventID.Restored:
                WindowRestored?.Invoke();
                break;

            case WindowEventID.Enter:
                WindowEnter?.Invoke();
                break;

            case WindowEventID.Leave:
                WindowLeave?.Invoke();
                break;

            case WindowEventID.FocusGained:
                WindowFocusGained?.Invoke();
                break;

            case WindowEventID.FocusLost:
                WindowFocusLost?.Invoke();
                break;

            case WindowEventID.Close:
                WindowClose?.Invoke();
                break;

            case WindowEventID.TakeFocus:
                WindowTakeFocus?.Invoke();
                break;

            case WindowEventID.HitTest:
                WindowHitTest?.Invoke();
                break;

            case WindowEventID.IccprofChanged:
                WindowIccprofChanged?.Invoke();
                break;

            case WindowEventID.DisplayChanged:
                WindowDisplayChanged?.Invoke();
                break;
        }
    }

    public void HandleAppEvent(uint eventType)
    {
        switch (eventType)
        {
            case (uint)EventType.AppWillenterbackground:
                AppWillenterbackground?.Invoke();
                break;
            case (uint)EventType.AppWillenterforeground:
                AppWillenterforeground?.Invoke();
                break;
            case (uint)EventType.AppDidenterbackground:
                AppDidenterbackground?.Invoke();
                break;
            case (uint)EventType.AppDidenterforeground:
                AppDidenterforeground?.Invoke();
                break;
        }
    }

    public event Action WindowShown = null!;

    public event Action WindowHidden = null!;

    public event Action WindowExposed = null!;

    public event Action<int, int> WindowMoved = null!;

    public event Action<int, int> WindowResized = null!;

    public event Action<int, int> WindowSizeChanged = null!;

    public event Action WindowMinimized = null!;

    public event Action WindowMaximized = null!;

    public event Action WindowRestored = null!;

    public event Action WindowEnter = null!;

    public event Action WindowLeave = null!;

    public event Action WindowFocusGained = null!;

    public event Action WindowFocusLost = null!;

    public event Action WindowClose = null!;

    public event Action WindowTakeFocus = null!;

    public event Action WindowHitTest = null!;

    public event Action WindowIccprofChanged = null!;

    public event Action WindowDisplayChanged = null!;

    public event Action AppWillenterbackground = null!;

    public event Action AppWillenterforeground = null!;

    public event Action AppDidenterbackground = null!;

    public event Action AppDidenterforeground = null!;
}