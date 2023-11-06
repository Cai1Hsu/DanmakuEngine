using DanmakuEngine.Dependency;
using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Logging;
using Silk.NET.Input;

namespace DanmakuEngine.Games.Screens;

public class MainMenuKeyBoardHandler : UserKeyboardHandler, IKeyboardHandler, IInjectable
{
    [Inject]
    private GameHost _host = null!;

    public SecretCodeHandler secretCodeHandler = null!;

    private bool cheating = false;

    public override void KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {
        // DEMO: Pressing escape closes the game
        if (arg2 == Key.Escape)
            _host.window.IsClosing = true;

        if (secretCodeHandler != null)
        {
            if (!cheating && secretCodeHandler.HandleKey(arg2))
            {
                Logger.Error("😠 You are cheating!");
                cheating = true;
            }
        }

    }

    public override void KeyUp(IKeyboard arg1, Key arg2, int arg3)
    {

    }
}