using DanmakuEngine.Dependency;
using DanmakuEngine.Engine;
using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Logging;
using Silk.NET.SDL;

namespace DanmakuEngine.Games.Screens.MainMenu;

public partial class MainMenuKeyBoardHandler : UserKeyboardHandler
{
    [Inject]
    private ScreenStack _screens = null!;

    [Inject]
    private GameHost _host = null!;

    public SecretCodeHandler secretCodeHandler = null!;

    private bool cheating = false;

    protected override void RegisterKeys()
    {
        // DEMO: Pressing escape closes the game
        Register(KeyCode.KEscape).OnDown += (_, _) =>
        {
            _host.Scheduler.ScheduleTask(() =>
            {
                while (!_screens.Empty())
                    _screens.Pop();

                _host.RequestClose();
            });
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
                   secretCodeHandler.HandleKey(e.Keysym);

        return handled;
    }
}
