using DanmakuEngine.Dependency;
using Silk.NET.SDL;

namespace DanmakuEngine.Input.Handlers;

public abstract partial class UserKeyboardHandler
{
    public UserKeyboardHandler()
    {
        if (this is IInjectable injectable)
            injectable.AutoInject();
    }

    /// <summary>
    /// This method is called when a key is pressed down.
    /// the event is transferred by the <see cref="TopKeyboardHandler"/>.
    /// when handling a key down event, you should always uses sym instead of scancode
    /// because scancode is platform dependent.
    /// </summary>
    /// <param name="keysym">the SDL_Keysym representing the key that was pressed</param>
    /// <param name="repeat">non-zero if this is a key repeat</param>
    public virtual void KeyDown(Keysym keysym, bool repeat)
    {

    }

    /// <summary>
    /// This method is called when a key is released.
    /// the event is transferred by the <see cref="TopKeyboardHandler"/>
    /// see <see cref="KeyDown"/> for more information.
    /// </summary>
    /// <param name="keysym">the SDL_Keysym representing the key that was released</param>
    /// <param name="repeat">non-zero if this is a key repeat</param>
    public virtual void KeyUp(Keysym keysym, bool repeat)
    {

    }
}
