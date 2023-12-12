using DanmakuEngine.Dependency;
using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Logging;
using Silk.NET.SDL;

namespace DanmakuEngine.Games.Screens.MainMenu;

public partial class MainMenuKeyBoardHandler : UserKeyboardHandler
{
    [Inject]
    private ScreenStack screens = null!;

    public SecretCodeHandler secretCodeHandler = null!;

    private bool cheating = false;

    public override void KeyDown(Keysym key, bool _)
    {
        // DEMO: Pressing escape closes the game

        if (key.Sym == (int)KeyCode.KEscape)
        {
            // TODO: add this action to Scheduler
            // We should handle actions in the Update loop
            // This is needed to avoid concurrency issues
            // as we are planning to use multiple threads
            while (!screens.Empty())
                screens.Pop();
        }

        if (secretCodeHandler != null)
        {
            if (!cheating && secretCodeHandler.HandleKey(key))
            {
                Logger.Error("😠 You are cheating!");
                cheating = true;
            }
        }
    }
}
