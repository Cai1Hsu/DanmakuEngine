using DanmakuEngine.Graphics;
using Silk.NET.Input;
using Silk.NET.SDL;

namespace DanmakuEngine.Input.Handlers;

public interface IKeyboardHandler
{
    bool HandleEvent(KeyboardEvent e);

    /// <summary>
    /// Invoked when a keydown event is not handled by any other handlers
    /// </summary>
    /// <param name="e">the event</param>
    /// <returns>whether you handled the event</returns>
    virtual bool KeyDown(KeyboardEvent e)
    {
        return false;
    }

    /// <summary>
    /// Invoked when a keyup event is not handled by any other handlers
    /// </summary>
    /// <param name="e">the event</param>
    /// <returns>whether you handled the event</returns>
    virtual bool KeyUp(KeyboardEvent e)
    {
        return false;
    }

}
