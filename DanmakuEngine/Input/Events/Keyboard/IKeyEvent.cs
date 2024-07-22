namespace DanmakuEngine.Input.Events.Keyboard;

public interface IKeyEvent : IInputEvent
{
    /// <summary>
    /// SDL2 doesn't have a device ID for keyboard events
    /// But it was added in SDL3, this is a placeholder for future compatibility
    /// For now, all devices are 1 as 0 means virtual keyboard
    /// </summary>
    public uint DeviceId { get; }

    public bool IsUnknownOrVirtualDevice => DeviceId == 0;


    /// <summary>
    /// Indicates whether the input is a repeat input.
    /// </summary>
    public bool IsRepeatInput { get; }
}
