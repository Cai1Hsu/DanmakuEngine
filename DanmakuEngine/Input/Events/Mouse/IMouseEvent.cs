namespace DanmakuEngine.Input.Events.Mouse;

public interface IMouseEvent : IInputEvent
{
    public const uint TOUCH_DEVICE_ID = unchecked((uint)-1);

    public uint DeviceId { get; }

    public virtual bool IsTouch => DeviceId == TOUCH_DEVICE_ID;
}
