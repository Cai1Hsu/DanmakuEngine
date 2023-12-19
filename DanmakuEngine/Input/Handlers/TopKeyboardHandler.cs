using System.Diagnostics;
using DanmakuEngine.Dependency;
using DanmakuEngine.Engine;
using DanmakuEngine.Games.Screens;
using DanmakuEngine.Input.Keybards;
using DanmakuEngine.Logging;
using Silk.NET.SDL;

namespace DanmakuEngine.Input.Handlers;

public partial class TopKeyboardHandler : IInputHandler
{
    [Inject]
    private ScreenStack screens = null!;

    private IEnumerable<IKeyboardHandler?> handlers()
    {
        // TODO: add more handlers that has higher priority
        // for example, when we are focus on a text box, we should not handle the keyboard event

        yield return screens.Peek()?.keyboardHandler!;
    }

    public void Register(GameHost host)
    {
        // check that if we inject the screen stack successfully
        Debug.Assert(screens != null);

        host.KeyEvent += HandleEvent;
    }

    public void HandleEvent(KeyboardEvent e)
    {
        foreach (var h in handlers())
        {
            if (h is not null && h.HandleEvent(e))
                return;
        }

        // enqueue to a queue
        Keyboard.Enqueue((KeyCode)e.Keysym.Sym, e.Timestamp);
    }
}
