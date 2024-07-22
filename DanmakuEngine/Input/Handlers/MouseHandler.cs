using DanmakuEngine.Bindables;
using DanmakuEngine.Engine;
using DanmakuEngine.Input.Events.Mouse;
using Silk.NET.Maths;
using Silk.NET.SDL;
using SDL_MouseButton = Veldrid.Sdl2.SDL_MouseButton;

namespace DanmakuEngine.Input.Handlers;

public partial class MouseHandler : InputHandlerBase, IHasSensitivity<float>
{
    public Bindable<float> HorizontalSensitivityBindable { get; private set; } = new Bindable<float>(1f);

    public Bindable<float> VerticalSensitivityBindable { get; private set; } = new Bindable<float>(1f);

    /// <summary>
    /// Whether to use raw input.
    /// </summary>
    public Bindable<bool> RawInput { get; } = new Bindable<bool>(true);

    /// <summary>
    /// Whether to show the OS cursor.
    /// </summary>
    public Bindable<bool> ShowCursor { get; } = new Bindable<bool>(true);

    /// <summary>
    /// Will ignore all mouse move event if set to true.
    /// </summary>
    public Bindable<bool> LockCursor { get; } = new Bindable<bool>(false);

    public Bindable<bool> InvertVertical { get; } = new Bindable<bool>(false);

    // I know some people like inverting vertical, but will anyone like this?
    public Bindable<bool> InvertHorizontal { get; } = new Bindable<bool>(false);

    /// <summary>
    /// Whether to capture the mouse input in Relative mode.
    /// </summary>
    public Bindable<bool> UseRelativeMode { get; } = new Bindable<bool>(true);

    /// <summary>
    /// Indicates whether the cursor is in the window.
    /// </summary>
    public Bindable<bool> CursorInWindow { get; } = new Bindable<bool>(true);

    private Sdl _sdl = null!;

    public override void Register(GameHost host)
    {
        _sdl = SDL.Api;

        UseRelativeMode.Value = _sdl.GetRelativeMouseMode() == SdlBool.True;
        UseRelativeMode.BindValueChanged(_ => updateRelativeMode(true));

        RawInput.BindValueChanged(v =>
        {
            if (v.NewValue == v.OldValue)
                return;

            _lastRawX = null;
            _lastRawY = null;
        });

        Enabled.BindValueChanged(e =>
        {
            // let the `RelativeMode` decide.
            updateRelativeMode(true);

            if (e.NewValue)
            {
                host.MouseMove += HandleMouseMove;
                host.MouseScroll += HandleMouseScroll;
                host.MouseButtonDown += handleMouseButtonDown;
                host.MouseButtonUp += handleMouseButtonUp;
            }
            else
            {
                host.MouseMove += HandleMouseMove;
                host.MouseScroll -= HandleMouseScroll;
                host.MouseButtonDown -= handleMouseButtonDown;
                host.MouseButtonUp -= handleMouseButtonUp;
            }
        }, true);

        // Query the current cursor state withouth changing it.
        ShowCursor.Value = _sdl.ShowCursor(-1) == 1;

        ShowCursor.BindValueChanged(v =>
        {
            if (v.NewValue)
                _sdl.ShowCursor(1);
            else
                _sdl.ShowCursor(0);
        }, true);

        CursorInWindow.BindTo(host.window.IsMouseInWindow);
    }

    private void updateRelativeMode(bool value)
    {
        var enableRelative = value && Enabled.Value && UseRelativeMode.Value;

        if (enableRelative)
            _sdl.SetRelativeMouseMode(SdlBool.True);
        else
            _sdl.SetRelativeMouseMode(SdlBool.False);
    }

    private float? _lastRawX = null;
    private float? _lastRawY = null;
    public void HandleMouseMove(MouseMotionEvent e)
    {
        if (LockCursor.Value)
            return;

        float deltaX, deltaY;

        if (RawInput.Value)
        {
            deltaX = e.Xrel;
            deltaY = e.Yrel;
        }
        else
        {
            if (_sdl.GetRelativeMouseMode() == SdlBool.True)
            {
                deltaX = e.X;
                deltaY = e.Y;
            }
            else
            {
                float rawX = e.X, rawY = e.Y;

                if (_lastRawX is null || _lastRawY is null)
                {
                    _lastRawX = rawX;
                    _lastRawY = rawY;
                    return;
                }

                deltaX = rawX - _lastRawX.Value;
                deltaY = rawY - _lastRawY.Value;

                _lastRawX = rawX;
                _lastRawY = rawY;
            }
        }

        if (InvertHorizontal.Value)
            deltaX = -deltaX;

        if (InvertVertical.Value)
            deltaY = -deltaY;

        deltaX *= HorizontalSensitivityBindable.Value;
        deltaY *= VerticalSensitivityBindable.Value;

        enqueueEvent(new MouseMoveEvent
        {
            Timestamp = e.Timestamp,
            DeviceId = e.Which,
            Delta = new Vector2D<float>(deltaX, deltaY)
        });
    }

    private void handleMouseButtonUp(MouseButtonEvent e)
        => HandleMouseButton(e, false);

    private void handleMouseButtonDown(MouseButtonEvent e)
        => HandleMouseButton(e, true);

    public void HandleMouseButton(MouseButtonEvent e, bool keyDown)
    {
        bool doubleClick = e.Clicks == 2;

        var button = e.Button;

        enqueueEvent(keyDown switch
        {
            true => new MouseDownEvent
            {
                Timestamp = e.Timestamp,
                DeviceId = e.Which,
                Button = translateButton(button),
                IsDoubleClick = doubleClick
            },
            false => new MouseUpEvent
            {
                Timestamp = e.Timestamp,
                DeviceId = e.Which,
                Button = translateButton(button)
            }
        });
    }

    private MouseButton translateButton(byte button)
    {
        return button switch
        {
            (byte)SDL_MouseButton.Left => MouseButton.Left,
            (byte)SDL_MouseButton.Middle => MouseButton.Middle,
            (byte)SDL_MouseButton.Right => MouseButton.Right,
            (byte)SDL_MouseButton.X1 => MouseButton.Button1,
            (byte)SDL_MouseButton.X2 => MouseButton.Button2,
            _ => MouseButton.Left
        };
    }

    private uint _lastPreciseScroll = 0;
    private const uint scroll_debounce = 100;
    public void HandleMouseScroll(MouseWheelEvent e)
    {
        const uint flipped = 1;

        var (offsetX, offsetY) = (e.PreciseX, e.PreciseY);

        if (e.Direction == flipped)
        {
            offsetX = -offsetX;
            offsetY = -offsetY;
        }

        if ((e.PreciseX != e.X) || (e.PreciseY != e.Y) || (e.PreciseX % 1 != 0) || (e.PreciseY % 1 != 0))
            _lastPreciseScroll = e.Timestamp;

        bool precise = e.Timestamp - _lastPreciseScroll < scroll_debounce;

        enqueueEvent(new MouseScrollEvent
        {
            Timestamp = e.Timestamp,
            DeviceId = e.Which,
            Delta = new Vector2D<float>(offsetX, offsetY),
            Precise = precise
        });
    }

    private void enqueueEvent(IMouseEvent e)
        => InputQueue.Enqueue(e);
}
