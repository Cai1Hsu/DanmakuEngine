using DanmakuEngine.Logging;

namespace DanmakuEngine.Games.Screens.MainScreen;

public class MainScreen : Screen
{
    public MainScreen(ScreenStack parent) : base(parent)
    {
        keyboardHandler = new MainMenuKeyBoardHandler()
        {
            secretCodeHandler = new(Clock),
        };
    }

    public override void Load()
    {
        base.Load();
    }

    public override void Start()
    {
        base.Start();

        Logger.Info("Keyboard is handled in this screen.");
        Logger.Info("You can exit the game by pressing ESC.");
    }

    public override void Update()
    {
        
    }
}