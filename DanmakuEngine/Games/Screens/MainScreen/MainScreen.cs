using DanmakuEngine.Dependency;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Games.Screens.MainScreen;

public class MainScreen : Screen, IInjectable,IAutoloadable
{
    public MainScreen()
    {
        keyboardHandler = new MainMenuKeyBoardHandler();
    }

    public void OnLoad()
    {
        ((IInjectable)keyboardHandler).AutoInject();
    }

    public override void Start()
    {
        Logger.Info("Keyboard is handled in this screen.");
        Logger.Info("You can exit the game by pressing ESC.");
    }

    public override void Update(double delta)
    {
        
    }
}