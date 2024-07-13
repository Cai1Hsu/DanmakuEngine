using System.Collections.Concurrent;
using DanmakuEngine.Bindables;
using DanmakuEngine.Engine;
using DanmakuEngine.Input.Events;

namespace DanmakuEngine.Input.Handlers;

public abstract partial class InputHandlerBase : IInputHandler, IDisposable
{
    public Bindable<bool> Enabled { get; } = new(true);

    public abstract void Register(GameHost host);

    private object _inputQueueLock = new();

    private ConcurrentQueue<IInputEvent> _inputQueue = new();
    protected ConcurrentQueue<IInputEvent> InputQueue => _inputQueue;

    public virtual void GetEvents(ref Queue<IInputEvent> queue)
    {
        lock (_inputQueueLock)
        {
            while (_inputQueue.TryDequeue(out var e))
                queue.Enqueue(e);
        }
    }

    private bool _disposed = false;

    public void Dispose()
    {
        if (_disposed)
            return;

        dispose();
        GC.SuppressFinalize(this);
    }

    private void dispose()
    {
        Enabled.Value = false;
        _disposed = true;
    }

    ~InputHandlerBase()
    {
        if (!_disposed)
            dispose();
    }
}
