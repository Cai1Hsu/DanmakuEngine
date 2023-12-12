using DanmakuEngine.Graphics;
using Silk.NET.Input;
using Silk.NET.SDL;

namespace DanmakuEngine.Input.Handlers;

public interface IKeyboardHandler
{
    public abstract void KeyDown(KeyboardEvent e);

    public abstract void KeyUp(KeyboardEvent e);
}
