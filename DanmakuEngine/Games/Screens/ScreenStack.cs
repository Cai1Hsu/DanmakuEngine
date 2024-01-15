using DanmakuEngine.Configuration;
using DanmakuEngine.Graphics;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Games.Screens;

public class ScreenStack : CompositeDrawable
{
    private Stack<Screen> screens = new();

    private object _lock = new();

    protected override IEnumerable<Drawable> Children => screens;

    protected override bool AlwaysPresent => true;

    public ScreenStack(CompositeDrawable parent) : base(parent)
    {
    }

    public void Switch(Screen screen)
    {
        if (Empty())
            throw new InvalidOperationException("Cannot switch to a screen when the stack is empty");

        Screen last = null!;

        screen.SetParent(this);

        lock (_lock)
        {
            last =
            screens.Pop();
            screens.Push(screen);
        }

        updateAnotherFrame = true;

        if (ConfigManager.HasConsole)
            Logger.Debug($"ScreenStack: Switchd to(poped {last.GetType()} and pushed) {screen.GetType().Name}, current depth: {screens.Count}");
    }

    /// <summary>
    /// If you have to pop the screen, use <see cref="Switch"/> instead
    /// </summary>
    /// <param name="screen">Screen to push</param>
    public void Push(Screen screen)
    {
        screen.SetParent(this);

        lock (_lock)
        {
            screens.Push(screen);
        }

        updateAnotherFrame = true;

        if (ConfigManager.HasConsole)
            Logger.Debug($"ScreenStack: Pushed {screen.GetType().Name}, current depth: {screens.Count}");
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

        if (ConfigManager.HasConsole)
            Logger.Debug($"ScreenStack: Popped {screen.GetType().Name}, current depth: {screens.Count}");

        return screen;
    }

    public bool Empty()
    {
        lock (_lock)
        {
            return screens.Count == 0;
        }
    }

    private bool updateAnotherFrame = false;

    protected override bool UpdateChildren(bool fixedUpdate = false)
    {
        do
        {
            updateAnotherFrame = false;

            var peek = Peek();

            if (peek is null)
                return true;

            // We assume that the user won't change the screen stack in the FixedUpdate
            if (peek.UpdateSubTree(fixedUpdate) && !updateAnotherFrame)
                return true;

        } while (updateAnotherFrame);

        return false;
    }
}
