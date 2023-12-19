using System.Collections.Generic;
using DanmakuEngine.Dependency;
using DanmakuEngine.Input.Keybards;
using Silk.NET.SDL;

namespace DanmakuEngine.Input.Handlers;

public abstract partial class UserKeyboardHandler : KeyHander, IKeyboardHandler
{
    public UserKeyboardHandler()
        : base()
    {
        if (this is IInjectable injectable)
            injectable.AutoInject();
    }

    protected abstract void RegisterKeys();

    protected KeyStatus Register(KeyCode key)
        => keyStatuses[key];
}
