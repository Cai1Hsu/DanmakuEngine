using System.Collections.Concurrent;
using DanmakuEngine.Bindables;
using DanmakuEngine.Dependency;
using DanmakuEngine.Engine;
using DanmakuEngine.Input.Events;

namespace DanmakuEngine.Input.Handlers;

public interface IInputHandler
{
    public Bindable<bool> Enabled { get; }

    public void Register(GameHost host);

    public void GetEvents(ref Queue<IInputEvent> queue);
}
