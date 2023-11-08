using System.Diagnostics;
using DanmakuEngine.Dependency;
using DanmakuEngine.Engine;
using DanmakuEngine.Games.Screens;
using Silk.NET.SDL;

namespace DanmakuEngine.Input.Handlers;

public class TopKeyboardHandler : IInputHandler
{
    [Inject]
    private ScreenStack screens = null!;

    public TopKeyboardHandler()
    {
        DependencyContainer.AutoInject(this);
    }

    public void KeyDown(KeyboardEvent e)
    {
        Debug.Assert(e.Type == (uint)EventType.Keydown);

        var repeat = e.Repeat != 0;
        var keysym = e.Keysym;

        screens.Peek()?.keyboardHandler?.KeyDown(keysym, repeat);
    }

    public void KeyUp(KeyboardEvent e)
    {
        Debug.Assert(e.Type == (uint)EventType.Keyup);

        var repeat = e.Repeat != 0;
        var keysym = e.Keysym;

        screens.Peek()?.keyboardHandler?.KeyUp(keysym, repeat);
    }

    public void Register(GameHost host)
    {
        host.KeyDown += KeyDown;
        host.KeyUp += KeyUp;
    }
}