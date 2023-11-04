using DanmakuEngine.Dependency;
using DanmakuEngine.Input.Handlers;
using Silk.NET.Input;

namespace DanmakuEngine.Games.Screens;

public class MainMenuKeyBoardHandler : IKeyboardHandler, IInjectable
{
    [Inject]
    public GameHost _host = null!;

    public void KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {
        // DEMO: Pressing escape closes the game
        if (arg2 == Key.Escape)
            _host.window.IsClosing = true;
    }

    public void KeyUp(IKeyboard arg1, Key arg2, int arg3)
    {
        
    }
}