using DanmakuEngine.Dependency;
using DanmakuEngine.Engine;
using DanmakuEngine.Input;
using DanmakuEngine.Input.EventReceivers;
using DanmakuEngine.Input.Events.Mouse;
using DanmakuEngine.Logging;
using Silk.NET.SDL;

namespace DanmakuEngine.Games.Screens.MainMenu;

public partial class MainMenuKeyBoardHandler : GameObject, IReceiveKeyboardEvent
{
    [Inject]
    private GameHost _host = null!;

    public SecretCodeHandler secretCodeHandler = null!;

    private bool cheating = false;

    public bool Active => true;

    public uint Priority => 100;

    public InputManager InputManager { get; set; } = null!;

    protected override void Start()
    {
        secretCodeHandler.OnSecretCodeEntered += delegate
        {
            Logger.Error("😠 You are cheating!");
            cheating = true;
        };
    }

    public void OnKeyDown(KeyDownEvent e, ref bool handled)
    {
        if (!cheating)
            handled |= secretCodeHandler.HandleKey(e.Button);
        if (e.Button is Keys.Escape)
            _host.RequestClose();

        Logger.Debug($"MainMenuKeyBoardHandler: Handled key: {e.Button}, {cheating}");
    }

    public void OnKeyUp(KeyUpEvent e, ref bool handled)
    {
    }
}
