using Silk.NET.Input;

namespace DanmakuEngine.Input.Handlers;

public interface IKeyboardHandler
{
    public void KeyDown(IKeyboard arg1, Key arg2, int arg3);

    public void KeyUp(IKeyboard arg1, Key arg2, int arg3);
}