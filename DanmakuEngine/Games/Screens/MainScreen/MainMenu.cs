using DanmakuEngine.Dependency;

namespace DanmakuEngine.Games.Screens;

public class MainMenu : Screen, IInjectable, IAutoloadable
{
    public MainMenu()
    {
        keyboardHandler = new MainMenuKeyBoardHandler();
    }

    public void OnLoad()
    {
        ((IInjectable)keyboardHandler).AutoInject();
    }
}