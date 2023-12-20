using DanmakuEngine.Graphics;
using Silk.NET.Input;
using Silk.NET.SDL;

namespace DanmakuEngine.Input.Handlers;

public interface IKeyboardHandler
{
    bool HandleEvent(KeyboardEvent e);
}
