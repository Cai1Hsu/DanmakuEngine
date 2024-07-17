using DanmakuEngine.Allocations;
using DanmakuEngine.Dependency;
using DanmakuEngine.Engine;
using DanmakuEngine.Engine.Windowing;
using DanmakuEngine.Games;
using DanmakuEngine.Input.EventReceivers;
using DanmakuEngine.Input.Events;
using DanmakuEngine.Input.Events.Mouse;
using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Input.States;
using DanmakuEngine.Logging;
using Silk.NET.Maths;

namespace DanmakuEngine.Input;

public partial class InputManager : GameObject
{
    public IInputHandler[] Handlers { get; private set; }

    public readonly MouseManager Mouse = new MouseManager();

    public readonly InputState State = new InputState();

    // public Drawable? FocusedDrawable { get; internal set; } = null!;

    public static InputManager GetInputManager()
        => DependencyContainer.Get<InputManager>();

    public InputManager()
    {
        Handlers =
        [
            Mouse.Initialize(new MouseHandler()),
            new KeyboardHandler(),
        ];
    }

    internal void Register(GameHost host)
    {
        foreach (var h in Handlers)
        {
            Logger.Debug($"Registering {h.GetType()}");

            h.AutoInject();

            h.Register(host);
        }

        State.Initialize(host);
    }

    public void RegisterReceiver(IReceiveEvent receiver)
    {
        _receivers.Add(new WeakReference<IReceiveEvent>(receiver));

        Logger.Debug($"Registered Input Receiver: {receiver.GetType()}");
    }

    [Inject]
    private Sdl2Window _window = null!;

    private Vector2D<float>? _pendingWindowSize = null;
    private Vector2D<float> _windowSize = Vector2D<float>.Zero;
    protected override void Start()
    {
        _window.WindowSizeChanged += (x, y) => _pendingWindowSize = new Vector2D<float>(x, y);

        var windowSize = _window.Size;

        _windowSize = new Vector2D<float>(windowSize.X, windowSize.Y);
    }

    protected override void LateUpdate()
    {
        if (_pendingWindowSize.HasValue)
        {
            _windowSize = _pendingWindowSize.Value;
        }
    }

    private Vector2D<float> toWorldSpace(Vector2D<float> vector)
        => new Vector2D<float>(
            (vector.X - _windowSize.X / 2) * 640.0f / _windowSize.X,
            (_windowSize.Y / 2 - vector.Y) * 480.0f / _windowSize.Y
        );

    protected override void Update()
    {
        pollEventsFromHandlers();

        while (_events.TryDequeue(out var evt))
        {
            evt.Apply(State);
            bool handled = false;

            switch (evt)
            {
                case MouseDownEvent mouseDownEvent:
                    traverseReceivers(receiver =>
                    {
                        if (receiver is IReceiveMouseInput mouseInputReceiver)
                            mouseInputReceiver.OnMouseButtonDown(mouseDownEvent, ref handled);
                        return handled;
                    });
                    break;

                case MouseUpEvent mouseUpEvent:
                    traverseReceivers(receiver =>
                    {
                        if (receiver is IReceiveMouseInput mouseInputReceiver)
                            mouseInputReceiver.OnMouseButtonUp(mouseUpEvent, ref handled);
                        return handled;
                    });
                    break;

                case MouseMoveEvent mouseMoveEvent:
                    var mousePosition = State.Mouse.Position;
                    var worldPosition = toWorldSpace(mousePosition);
                    traverseReceivers(receiver =>
                    {
                        if (receiver is IReceiveMouseInput mouseInputReceiver)
                            mouseInputReceiver.OnMouseMove(mouseMoveEvent);
                        else if (receiver is IReceiveMousePosition mousePositionReceiver)
                            mousePositionReceiver.OnReceiveMousePosition(mousePosition, worldPosition);
                        return false;
                    });
                    break;

                case MouseScrollEvent mouseScrollEvent:
                    traverseReceivers(receiver =>
                    {
                        if (receiver is IReceiveMouseInput mouseInputReceiver)
                            mouseInputReceiver.OnMouseScroll(mouseScrollEvent);
                        return false;
                    });
                    break;

                case KeyDownEvent keyDownEvent:
                    traverseReceivers(receiver =>
                    {
                        if (receiver is IReceiveKeyboardEvent keyboardEventReceiver)
                            keyboardEventReceiver.OnKeyDown(keyDownEvent, ref handled);
                        return handled;
                    });
                    break;

                case KeyUpEvent keyUpEvent:
                    traverseReceivers(receiver =>
                    {
                        if (receiver is IReceiveKeyboardEvent keyboardEventReceiver)
                            keyboardEventReceiver.OnKeyUp(keyUpEvent, ref handled);
                        return handled;
                    });
                    break;
            }
        }
    }

    private void traverseReceivers(Func<IReceiveEvent, bool> action)
    {
        foreach (var receiverNode in _receivers)
        {
            if (!receiverNode.Value.TryGetTarget(out var receiver))
            {
                _removeList.Add(receiverNode);

                continue;
            }

            // if (receiver.Priority >= DrawableStartPriority)
            // {
            //     if (!handledFocusedDrawable && FocusedDrawable is not null)
            //     {
            //         action.Invoke(FocusedDrawable);
            //         handledFocusedDrawable = true;
            //     } else if (handledFocusedDrawable && receiver == FocusedDrawable)
            //         continue;
            // }

            if (action.Invoke(receiver))
                break;
        }

        if (_removeList.Count > 0)
        {
            foreach (var tobeRemoved in _removeList)
            {
                _receivers.Remove(tobeRemoved);
            }
        }
    }

    private List<RBTreeNode<WeakReference<EventReceivers.IReceiveEvent>>> _removeList = new();

    private Queue<IInputEvent> _events = new Queue<IInputEvent>();
    private void pollEventsFromHandlers()
    {
        foreach (var h in Handlers)
        {
            h.GetEvents(ref _events);
        }
    }

    private static int receiverComparer(WeakReference<IReceiveEvent> lhs, WeakReference<IReceiveEvent> rhs)
    {
        return (lhs.TryGetTarget(out var left) && left is not null,
            rhs.TryGetTarget(out var right) && right is not null) switch
        {
            (true, true) => (int)(left!.Priority - right!.Priority),
            // (false, true) => 1,
            // (true, false) => -1,
            _ => 0,
        };
    }

    private RBTree<WeakReference<IReceiveEvent>> _receivers = new(receiverComparer);
}
