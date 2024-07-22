using DanmakuEngine.Engine;
using DanmakuEngine.Input.Keybards;

namespace DanmakuEngine.Input.States;

public class InputState
{
    public MouseState Mouse { get; }

    public KeyboardState Keyboard { get; }

    public InputState(MouseState? mouse = null, KeyboardState? keyboard = null)
    {
        Mouse = mouse ?? new MouseState();
        Keyboard = keyboard ?? new KeyboardState();
    }

    public void Initialize(GameHost host)
    {
        Mouse.Initialize(host);
        Keyboard.Initialize();
    }
}
