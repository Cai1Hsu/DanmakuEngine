using DanmakuEngine.Dependency;
using DanmakuEngine.Engine;
using DanmakuEngine.Input.Keybards;
using DanmakuEngine.Logging;
using Silk.NET.SDL;

namespace DanmakuEngine.Games.Screens.MainMenu;

public partial class MainMenuKeyBoardHandler : KeyboardHandler
{
    [Inject]
    private ScreenStack _screens = null!;

    [Inject]
    private GameHost _host = null!;

    public SecretCodeHandler secretCodeHandler = null!;

    private bool cheating = false;

    public override void RegisterKeys()
    {
        // DEMO: Pressing escape closes the game
        Register(KeyCode.KEscape).OnDown += (_, _) =>
        {
            while (!_screens.Empty())
                _screens.Pop();

            _host.RequestClose();
        };

        secretCodeHandler.OnSecretCodeEntered += delegate
        {
            Logger.Error("😠 You are cheating!");
            cheating = true;
        };
    }

    public override bool HandleEvent(KeyboardEvent e)
    {
        var handled = base.HandleEvent(e);

        handled |= !cheating &&
                   secretCodeHandler is not null &&
                   IsKeyDown(e) &&
                   secretCodeHandler.HandleKey((KeyCode)e.Keysym.Sym);

        return handled;
    }
}
