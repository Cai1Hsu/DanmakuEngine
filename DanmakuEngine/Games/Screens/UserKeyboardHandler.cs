using DanmakuEngine.Dependency;
using Silk.NET.Input;

namespace DanmakuEngine.Input.Handlers;

public class UserKeyboardHandler : IKeyboardHandler, IInjectable
{
    public UserKeyboardHandler()
    {
        ((IInjectable)this).AutoInject();
    }

    public virtual void KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {

    }

    public virtual void KeyUp(IKeyboard arg1, Key arg2, int arg3)
    {
    }
}