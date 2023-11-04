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
        Logger.Info("Keyboard is handled in this screen.");
        Logger.Info("You can exit the game by pressing ESC.");
    }
}