using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using DanmakuEngine.Bindables;
using DanmakuEngine.Extensions;
using DanmakuEngine.Logging;
using Silk.NET.Maths;
using Silk.NET.SDL;

using Imgui = DanmakuEngine.DearImgui.Imgui;

namespace DanmakuEngine.Engine.Windowing;

// TODO: This class still not fully decoupled windowing from the engine
public unsafe class Sdl2Window : IWindow
{
    private Sdl _sdl;
    public Sdl Sdl => _sdl;

    private volatile bool exists = true;

    public bool WindowExists => exists;

    private Window* _window;

    public Window* Window => _window;

    public IntPtr Handle => (IntPtr)_window;

    private SysWMInfo? _sysWMInfo = null;

    public SysWMInfo WMInfo
    {
        get
        {
            if (!_sysWMInfo.HasValue)
            {
                SysWMInfo info = default!;

                _sdl.GetVersion(ref info.Version);
                _sdl.GetWindowWMInfo(_window, &info);

                _sysWMInfo = info;
            }

            return _sysWMInfo.Value;
        }
    }

    public DisplayMode DisplayMode
    {
        get
        {
            DisplayMode mode = new();

            if (_sdl.GetWindowDisplayMode(_window, &mode) != 0)
                throw new Exception("Failed to get display mode.");

            return mode;
        }
        set
        {
            if (_sdl.SetWindowDisplayMode(_window, &value) != 0)
                throw new Exception("Failed to set display mode.");
        }
    }

    public string Title
    {
        get => ByteExtensions.ToString(_sdl.GetWindowTitle(_window));
        set => _sdl.SetWindowTitle(_window, value);
    }

    public Vector2D<int> Position
    {
        get
        {
            var pos = Vector2D<int>.Zero;
            _sdl.GetWindowPosition(_window, ref pos.X, ref pos.Y);
            return pos;
        }
        set => _sdl.SetWindowPosition(_window, value.X, value.Y);
    }

    public Vector2D<int> Size
    {
        get
        {
            var size = Vector2D<int>.Zero;
            _sdl.GetWindowSize(_window, ref size.X, ref size.Y);
            return size;
        }
        set => _sdl.SetWindowSize(_window, value.X, value.Y);
    }

    private Bindable<bool>? _isMouseInWindow = null;

    public Bindable<bool> IsMouseInWindow
    {
        get
        {
            if (_isMouseInWindow is null)
            {
                var windowSize = Size;
                var windowPosition = Position;
                var mousePosition = Vector2D<int>.Zero;

                _sdl.GetMouseState(ref mousePosition.X, ref mousePosition.Y);

                var isInWindow = mousePosition.X >= windowPosition.X &&
                                mousePosition.Y >= windowPosition.Y &&
                                mousePosition.X <= windowPosition.X + windowSize.X &&
                                mousePosition.Y <= windowPosition.Y + windowSize.Y;

                _isMouseInWindow = new Bindable<bool>(isInWindow);
            }

            return _isMouseInWindow;
        }
    }

    public Sdl2Window(string title,
                      int x, int y,
                      int width, int height,
                      uint flag)
    {
        this._sdl = SDL.Api;

        this._window = _sdl.CreateWindow(title,
                x, y,
                width, height,
                flag);

        if (_window is null)
            throw new SdlException("Window not created");
    }

    public void Dispose()
    {
        RequestClose();

        if (_window is null)
            return;

        _sdl.DestroyWindow(_window);
        _window = null!;
    }

    public void DoEvents(bool usePoll = true)
    {
        if (!usePoll)
            _sdl.PumpEvents();
        else
            PumpEvents();
    }

    public void RunWhile(Action onFrame, Func<bool> condition, bool doEvents = true)
    {
        OnLoad?.Invoke();

        do
        {
            if (doEvents)
                PumpEvents();

            onFrame.Invoke();
        } while (exists && condition.Invoke());

        OnExit?.Invoke();
    }

    public void RequestClose()
    {
        if (!exists)
            return;

        Logger.Debug("Requesting close");

        exists = false;
    }

    private bool checkImGuiMagic()
        => Imgui.Initialized && Imgui.WindowMagic == Handle;

    public unsafe void PumpEvents()
    {
        Event* e = stackalloc Event[1];

        bool imguiMagic = checkImGuiMagic();

        while (_sdl.PollEvent(e) != 0)
        {
            switch (e->Type)
            {
                case (uint)EventType.Firstevent:
                    // This is not reliable, so we ignore it
                    break;

                case (uint)EventType.AppTerminating:
                case (uint)EventType.Quit:
                    if (OnClose is null)
                        RequestClose();
                    else if (!OnClose.Invoke())
                        RequestClose();
                    break;

                // we should only handle the event once
                // and KeyDown(KeyUp) should has higher priority than KeyEvent as it is Engine level
                case (uint)EventType.Keydown:
                {
                    if (imguiMagic && Imgui.EatKeyboardEvents(e->Key))
                        break;
                    KeyDown?.Invoke(e->Key);
                }
                break;

                case (uint)EventType.Keyup:
                {
                    if (imguiMagic && Imgui.EatKeyboardEvents(e->Key))
                        break;
                    KeyUp?.Invoke(e->Key);
                }
                break;

                case (uint)EventType.Textinput:
                    if (!imguiMagic || !Imgui.EatTextInputEvents(e->Text))
                        TextInput?.Invoke(e->Text);
                    break;

                case (uint)EventType.Textediting:
                    TextEditing?.Invoke(e->Edit);
                    break;

                case (uint)EventType.Clipboardupdate:
                    ClipboardUpdate?.Invoke();
                    break;

                case (uint)EventType.Dropbegin:
                case (uint)EventType.Dropfile:
                case (uint)EventType.Dropcomplete:
                case (uint)EventType.Droptext:
                    DropEvent?.Invoke(e->Drop);
                    break;

                case (uint)EventType.Mousebuttondown:
                    if (!imguiMagic || !Imgui.EatMouseButtonEvents(e->Button))
                        MouseButtonDown?.Invoke(e->Button);
                    break;

                case (uint)EventType.Mousebuttonup:
                    if (!imguiMagic || !Imgui.EatMouseButtonEvents(e->Button))
                        MouseButtonUp?.Invoke(e->Button);
                    break;

                case (uint)EventType.Mousemotion:
                    MouseMove?.Invoke(e->Motion);
                    break;

                case (uint)EventType.Mousewheel:
                    if (!imguiMagic || !Imgui.EatMouseWheelEvents(e->Wheel))
                        MouseScroll?.Invoke(e->Wheel);
                    break;

                case SDL_EVENT_WINDOW_MOUSE_ENTER:
                    if (_isMouseInWindow is not null)
                        _isMouseInWindow.Value = true;

                    MouseEnteredWindow?.Invoke();
                    break;

                case SDL_EVENT_WINDOW_MOUSE_LEAVE:
                    if (_isMouseInWindow is not null)
                        _isMouseInWindow.Value = false;

                    MouseLeftWindow?.Invoke();
                    break;

                case (uint)EventType.Windowevent:
                    pumpWindowEvent(e->Window);
                    break;

                case (uint)EventType.AppWillenterbackground:
                case (uint)EventType.AppWillenterforeground:
                case (uint)EventType.AppDidenterforeground:
                case (uint)EventType.AppDidenterbackground:
                    pumpAppEvent(e->Type);
                    break;
            }
        }
    }

    private void pumpWindowEvent(WindowEvent windowEvent)
    {
        switch (windowEvent.Event)
        {
            case (byte)WindowEventID.Shown:
                WindowShown?.Invoke();
                break;

            case (byte)WindowEventID.Hidden:
                WindowHidden?.Invoke();
                break;

            case (byte)WindowEventID.Exposed:
                WindowExposed?.Invoke();
                break;

            case (byte)WindowEventID.Moved:
                WindowMoved?.Invoke(windowEvent.Data1, windowEvent.Data2);
                break;

            case (byte)WindowEventID.Resized:
                WindowResized?.Invoke(windowEvent.Data1, windowEvent.Data2);
                break;

            // This handles both Resized and SizeChanged
            case (byte)WindowEventID.SizeChanged:
                WindowSizeChanged?.Invoke(windowEvent.Data1, windowEvent.Data2);
                break;

            case (byte)WindowEventID.Minimized:
                WindowMinimized?.Invoke();
                break;

            case (byte)WindowEventID.Maximized:
                WindowMaximized?.Invoke();
                break;

            case (byte)WindowEventID.Restored:
                WindowRestored?.Invoke();
                break;

            case (byte)WindowEventID.Enter:
                WindowEnter?.Invoke();
                break;

            case (byte)WindowEventID.Leave:
                WindowLeave?.Invoke();
                break;

            case (byte)WindowEventID.FocusGained:
                WindowFocusGained?.Invoke();
                break;

            case (byte)WindowEventID.FocusLost:
                WindowFocusLost?.Invoke();
                break;

            case (byte)WindowEventID.Close:
                WindowClose?.Invoke();
                break;

            case (byte)WindowEventID.TakeFocus:
                WindowTakeFocus?.Invoke();
                break;

            case (byte)WindowEventID.HitTest:
                WindowHitTest?.Invoke();
                break;

            case (byte)WindowEventID.IccprofChanged:
                WindowIccprofChanged?.Invoke();
                break;

            case (byte)WindowEventID.DisplayChanged:
                WindowDisplayChanged?.Invoke();
                break;
        }
    }

    public void pumpAppEvent(uint eventType)
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

    public event Action? OnLoad = null!;

    // Currently not used
    // public event Action<double>? OnUpdate = null!;

    // public event Action<double>? OnDraw = null!;

    /// <summary>
    /// This event is called when the window is about to be closed
    /// </summary>
    public event Action? OnExit = null!;

    /// <summary>
    /// Is called when a close event is received. return true to block the close event
    /// </summary>
    public Func<bool>? OnClose { get; set; } = null!;

    #region Input Events

    /// <summary>
    /// Return true to prevent the event from being passed to the next handler
    /// </summary>
    public event Action<KeyboardEvent> KeyDown = null!;

    /// <summary>
    /// Return true to prevent the event from being passed to the next handler
    /// </summary>
    public event Action<KeyboardEvent> KeyUp = null!;

    public event Action<MouseButtonEvent> MouseButtonDown = null!;

    public event Action<MouseButtonEvent> MouseButtonUp = null!;

    public event Action<MouseMotionEvent> MouseMove = null!;

    public event Action<MouseWheelEvent> MouseScroll = null!;

    public event Action MouseEnteredWindow = null!;

    public event Action MouseLeftWindow = null!;

    public event Action<TextInputEvent> TextInput = null!;

    public event Action<TextEditingEvent> TextEditing = null!;

    /// <summary>
    /// Must fetch the text yourself
    /// </summary>
    public event Action ClipboardUpdate = null!;

    public event Action<DropEvent> DropEvent = null!;


    #endregion // Input Events

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

#pragma warning disable IDE1006 // Naming Styles
    private const uint SDL_EVENT_WINDOW_MOUSE_ENTER = 523;
    private const uint SDL_EVENT_WINDOW_MOUSE_LEAVE = 524;
#pragma warning restore IDE1006
}
