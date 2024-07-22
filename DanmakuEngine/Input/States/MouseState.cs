using DanmakuEngine.Bindables;
using DanmakuEngine.Engine;
using DanmakuEngine.Input.Events.Mouse;
using Silk.NET.Maths;
using Silk.NET.SDL;
using SDL_MouseButton = Veldrid.Sdl2.SDL_MouseButton;

namespace DanmakuEngine.Input.States;

public class MouseState
{
    public readonly ButtonStates<MouseButton> Buttons = new ButtonStates<MouseButton>();

    public Vector2D<float> Scroll { get; set; }

    public Vector2D<float> Position { get; set; }

    public Bindable<bool> CursorInWindow { get; private set; } = null!;

    public IMouseEvent? LastEvent { get; internal set; }

    public bool IsPressed(MouseButton button)
        => Buttons.IsPressed(button);

    public bool SetPressed(MouseButton button, bool pressed)
        => Buttons.SetPressed(button, pressed);

    private Sdl? _sdl = null;

    public void Initialize(GameHost host)
    {
        _sdl = SDL.Api;

        int x = -1, y = -1;
        uint buttons = _sdl.GetMouseState(ref x, ref y);

        Scroll = new Vector2D<float>(0, 0);
        Position = new Vector2D<float>(x, y);

        Buttons.Clear();
        if (isPressed(SDL_MouseButton.Left))
            Buttons.SetPressed(MouseButton.Left, true);

        if (isPressed(SDL_MouseButton.Middle))
            Buttons.SetPressed(MouseButton.Middle, true);

        if (isPressed(SDL_MouseButton.Right))
            Buttons.SetPressed(MouseButton.Right, true);

        if (isPressed(SDL_MouseButton.X1))
            Buttons.SetPressed(MouseButton.Button1, true);

        if (isPressed(SDL_MouseButton.X2))
            Buttons.SetPressed(MouseButton.Button2, true);

        CursorInWindow = host.window.IsMouseInWindow.GetBoundCopy();

        CursorInWindow.BindValueChanged(v =>
        {
            if (v.NewValue)
            {
                int x = -1, y = -1;

                _sdl.GetMouseState(ref x, ref y);
                Position = new Vector2D<float>(x, y);
            }
        });

        bool isPressed(SDL_MouseButton button)
            => (buttons & (uint)button) is not 0;
    }
}
