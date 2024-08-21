using DanmakuEngine.Configuration;
using DanmakuEngine.Graphics;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Games.Screens;

public class ScreenStack : CompositeDrawable
{
    private Stack<Screen> _screens = new();

    private object _lock = new();

    public new List<Screen> Children => _screens.ToList();

    protected override bool AlwaysPresent => true;

    public ScreenStack(CompositeDrawable parent)
        : base(parent)
    {
    }

    /// <summary>
    /// Switch to another screen, pop the current screen and push the new screen
    /// This method is thread-safe, prevent game from exiting when popping the last screen
    /// </summary>
    /// <param name="screen"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Switch(Screen screen)
    {
        if (Empty())
            throw new InvalidOperationException("Cannot switch to a screen when the stack is empty");

        Screen last = null!;

        screen.SetParent(this);

        lock (_lock)
        {
            last = _screens.Pop();
            last.OnScreenLeaving();
            _screens.Push(screen);
            screen.OnScreenEntering();
        }

        updateAnotherFrame = true;

        if (ConfigManager.HasConsole)
            Logger.Debug($"ScreenStack: Switchd to(poped {last.GetType()} and pushed) {screen.GetType().Name}, current depth: {_screens.Count}");
    }

    /// <summary>
    /// Push a screen to the stack
    /// If you have to pop the screen, use <see cref="Switch"/> instead
    /// </summary>
    /// <param name="screen">Screen to push</param>
    public void Push(Screen screen)
    {
        screen.SetParent(this);

        lock (_lock)
        {
            _screens.Push(screen);
            screen.OnScreenEntering();
        }

        updateAnotherFrame = true;

        if (ConfigManager.HasConsole)
            Logger.Debug($"ScreenStack: Pushed {screen.GetType().Name}, current depth: {_screens.Count}");
    }

    public Screen? Peek()
    {
        if (Empty())
            return null;

        lock (_lock)
        {
            return (Screen?)_screens.Peek();
        }
    }

    public Screen Pop()
    {
        var screen = _screens.Pop();
        screen.OnScreenLeaving();

        if (ConfigManager.HasConsole)
            Logger.Debug($"ScreenStack: Popped {screen.GetType().Name}, current depth: {_screens.Count}");

        return screen;
    }

    public bool Empty()
    {
        lock (_lock)
        {
            return _screens.Count == 0;
        }
    }

    private bool updateAnotherFrame = false;

    protected override bool UpdateChildren()
    {
        do
        {
            updateAnotherFrame = false;

            var peek = Peek();

            if (peek is null)
                return true;

            if (peek.UpdateSubTree() && !updateAnotherFrame)
                return true;
        } while (updateAnotherFrame);

        return false;
    }

    protected override void FixedUpdateChildren()
    {
        do
        {
            updateAnotherFrame = false;

            var peek = Peek();

            if (peek is null)
                return;

            peek.FixedUpdateSubtree();
        } while (updateAnotherFrame);
    }
}
