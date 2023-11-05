using DanmakuEngine.Graphics;
using Silk.NET.Input;

namespace DanmakuEngine.Input.Handlers;

public interface IKeyboardHandler
{
    public abstract void KeyDown(IKeyboard arg1, Key arg2, int arg3);

    public abstract void KeyUp(IKeyboard arg1, Key arg2, int arg3);
}