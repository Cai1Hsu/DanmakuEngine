using DanmakuEngine.Input.Events.Keyboard;

namespace DanmakuEngine.Input.EventReceivers;

public interface IReceiveKeyboardEvent : IReceiveEvent
{
    /// <summary>
    /// Called when a key is pressed.
    /// </summary>
    /// <param name="e">Event arguments</param>
    /// <param name="handled">set to true to prevent further processing</param>
    public void OnKeyDown(KeyDownEvent e, ref bool handled);

    /// <summary>
    /// Called when a key is released.
    /// </summary>
    /// <param name="e">Event arguments</param>
    /// <param name="handled">set to true to prevent further processing</param>
    public void OnKeyUp(KeyUpEvent e, ref bool handled);
}
