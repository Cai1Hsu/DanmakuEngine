using DanmakuEngine.Dependency;
using DanmakuEngine.Games.Screens.MainScreen;
using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Logging;
using Silk.NET.Input;

namespace DanmakuEngine.Games.Screens;

public class MainMenuKeyBoardHandler : IKeyboardHandler, IInjectable
{
    [Inject]
    private GameHost _host = null!;

    private SecretCodeHandler secretCodeHandler = new();

    private bool cheating = false;

    public void KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {
        // DEMO: Pressing escape closes the game
        if (arg2 == Key.Escape)
            _host.window.IsClosing = true;

        if (!cheating && secretCodeHandler.HandleKey(arg2))
        {
            Logger.Error("😠 You are cheating!");
            cheating = true;
        }
    }

    public void KeyUp(IKeyboard arg1, Key arg2, int arg3)
    {
        
    }
}