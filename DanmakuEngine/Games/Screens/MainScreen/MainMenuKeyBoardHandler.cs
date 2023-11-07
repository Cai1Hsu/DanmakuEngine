using DanmakuEngine.Dependency;
using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Logging;
using Silk.NET.Input;

namespace DanmakuEngine.Games.Screens;

public class MainMenuKeyBoardHandler : UserKeyboardHandler, IKeyboardHandler, IInjectable
{
    [Inject]
    private ScreenStack screens = null!;

    public SecretCodeHandler secretCodeHandler = null!;

    private bool cheating = false;

    public override void KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {
        // DEMO: Pressing escape closes the game
        if (arg2 == Key.Escape)
        {
            while (!screens.Empty())
                screens.Pop();
        }

        if (secretCodeHandler != null)
        {
            if (!cheating && secretCodeHandler.HandleKey(arg2))
            {
                Logger.Error("😠 You are cheating!");
                cheating = true;
            }
        }
    }
}