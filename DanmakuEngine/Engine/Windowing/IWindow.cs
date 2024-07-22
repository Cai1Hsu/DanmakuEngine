using Silk.NET.SDL;

namespace DanmakuEngine.Engine.Windowing;

public interface IWindow : IDisposable
{
    public IntPtr Handle { get; }

    public void DoEvents(bool includeInput = true);

    public void RunWhile(Action onFrame, Func<bool> condition, bool doEvents = true);

    public void RequestClose();

    public void PumpEvents();

    #region Lifecycle

    public event Action? OnLoad;

    // public event Action<double>? OnUpdate;

    // public event Action<double>? OnDraw;

    #endregion // Lifecycle

    #region Window Events

    public event Action WindowShown;

    public event Action WindowHidden;

    public event Action WindowExposed;

    public event Action<int, int> WindowMoved;

    public event Action<int, int> WindowResized;

    public event Action<int, int> WindowSizeChanged;

    public event Action WindowMinimized;

    public event Action WindowMaximized;

    public event Action WindowRestored;

    public event Action WindowEnter;

    public event Action WindowLeave;

    public event Action WindowFocusGained;

    public event Action WindowFocusLost;

    public event Action WindowClose;

    public event Action WindowTakeFocus;

    public event Action WindowHitTest;

    public event Action WindowIccprofChanged;

    public event Action WindowDisplayChanged;

    #endregion // Window Events

    #region App Events
    public event Action AppWillenterbackground;

    public event Action AppWillenterforeground;

    public event Action AppDidenterbackground;

    public event Action AppDidenterforeground;
    #endregion // App Events

    #region Input Events

    /// <summary>
    /// Return true to prevent the event from being passed to the next handler
    /// </summary>
    public event Action<KeyboardEvent> KeyDown;

    /// <summary>
    /// Return true to prevent the event from being passed to the next handler
    /// </summary>
    public event Action<KeyboardEvent> KeyUp;

    public event Action<MouseButtonEvent> MouseButtonDown;

    public event Action<MouseButtonEvent> MouseButtonUp;

    public event Action<MouseMotionEvent> MouseMove;

    public event Action<MouseWheelEvent> MouseScroll;

    #endregion // Input Events
}
