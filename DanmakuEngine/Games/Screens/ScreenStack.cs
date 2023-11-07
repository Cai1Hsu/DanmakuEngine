using DanmakuEngine.Graphics;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Games.Screens;

public class ScreenStack : CompositeDrawable
{
    private Stack<Screen> screens = new();
    private object _lock = new();

    protected override ICollection<Drawable> Children { get => (ICollection<Drawable>)screens; }

    protected override bool AlwaysPresent => true;

    public ScreenStack(CompositeDrawable parent) : base(parent)
    {
        this.load();
    }

    public void Switch(Screen screen)
    {
        if (Empty())
            throw new InvalidOperationException("Cannot switch to a screen when the stack is empty");

#if DEBUG
        Screen last = null!;
#endif

        lock (_lock)
        {
#if DEBUG
            last =
#endif
            screens.Pop();
            screens.Push(screen);
        }

#if DEBUG
        Logger.Debug($"ScreenStack: Switchd to(poped {last.GetType()} and pushed) {screen.GetType().Name}, current depth: {screens.Count}");
#endif
    }

    /// <summary>
    /// If you have to pop the screen, use <see cref="Switch"/> instead
    /// </summary>
    /// <param name="screen">Screen to push</param>
    public void Push(Screen screen)
    {
        lock (_lock)
        {
            screens.Push(screen);
        }

#if DEBUG
        Logger.Debug($"ScreenStack: Pushed {screen.GetType().Name}, current depth: {screens.Count}");
#endif
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
        Logger.Debug($"ScreenStack: Popped {screen.GetType().Name}, current depth: {screens.Count}");
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