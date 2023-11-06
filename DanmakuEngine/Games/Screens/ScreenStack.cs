using DanmakuEngine.Dependency;
using DanmakuEngine.Graphics;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Games.Screens;

public class ScreenStack : CompositeDrawable
{
    private Stack<Screen> screens = new();
    private object _lock = new();

    public ScreenStack(CompositeDrawable parent) : base(parent)
    {
        this.load();
    }

    public void Switch(Screen screen)
    {
        if (Empty())
            throw new InvalidOperationException("Cannot switch to a screen when the stack is empty");

        if (screen is IInjectable injectable)
            injectable.AutoInject();

        screen.load();

        lock (_lock)
        {
            screens.Pop();
            screens.Push(screen);
        }

#if DEBUG
        Logger.Debug($"ScreenStack: Switchd to {screen.GetType().Name}");
#endif

        screen.start();
    }

    /// <summary>
    /// If you have to pop the screen, use Switch instead
    /// </summary>
    /// <param name="screen"></param>
    public void Push(Screen screen)
    {
        if (screen is IInjectable injectable)
            injectable.AutoInject();

        screen.load();

        lock (_lock)
        {
            screens.Push(screen);
        }

#if DEBUG
        Logger.Debug($"ScreenStack: Pushed {screen.GetType().Name}");
#endif

        screen.start();
    }

    public Screen? Peek()
    {
        if (Empty())
            return null;

        lock (_lock)
        {
            return (Screen?)screens.Peek();
        }
    }

    public Screen Pop()
    {
        var screen = screens.Pop();

#if DEBUG
        Logger.Debug($"ScreenStack: Popped {screen.GetType().Name}");
#endif

        return screen;
    }

    public bool Empty()
    {
        lock (_lock)
        {
            return screens.Count == 0;
        }
    }

    public override bool UpdateSubTree()
    {
        if (Empty())
            return true;

        if (Peek()!.updateSubTree())
            return true;

        return false;
    }
}