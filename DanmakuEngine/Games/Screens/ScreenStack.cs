using DanmakuEngine.Dependency;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Games.Screens;

public class ScreenStack
{
    private Stack<Screen> screens = new();

    public void Push(Screen screen)
    {
        if (screen is IInjectable injectable)
            injectable.AutoInject();
        
        screen.Load();

        screens.Push(screen);

#if DEBUG
        Logger.Debug($"ScreenStack: Pushed {screen.GetType().Name}");
#endif

        screen.Start();
    }

    public Screen Peek()
        => screens.Peek();

    public Screen Pop()
    {
        var screen = screens.Pop();

#if DEBUG
        Logger.Debug($"ScreenStack: Popped {screen.GetType().Name}");
#endif

        return screen;
    }

    public bool Empty()
        => screens.Count == 0;
}