using DanmakuEngine.Dependency;

namespace DanmakuEngine.Games.Screens;

public class ScreenStack
{
    private Stack<Screen> screens = new();

    public void Push(Screen screen)
    {
        screens.Push(screen);

        if (screen is IAutoloadable autoloadable)
            autoloadable.AutoInject();
    }

    public Screen Peek()
        => screens.Peek();
}