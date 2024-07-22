using DanmakuEngine.Input.Events.Mouse;

namespace DanmakuEngine.Input.EventReceivers;

public interface IReceiveMouseInput : IReceiveEvent
{
    /// <summary>
    /// Called when a mouse button is pressed.
    /// </summary>
    /// <param name="e">Event arguments</param>
    /// <param name="handled">set to true to prevent further processing</param>
    public void OnMouseButtonDown(MouseDownEvent e, ref bool handled);

    /// <summary>
    /// Called when a mouse button is released.
    /// </summary>
    /// <param name="e">Event arguments</param>
    /// <param name="handled">set to true to prevent further processing</param>
    public void OnMouseButtonUp(MouseUpEvent e, ref bool handled);

    /// <summary>
    /// Called when the mouse is moved.
    /// This event can not be cancelled.
    /// </summary>
    /// <param name="e">Event arguments</param>
    public void OnMouseMove(MouseMoveEvent e);

    /// <summary>
    /// Called when the mouse is scrolled.
    /// This event can not be cancelled.
    /// </summary>
    /// <param name="e">Event arguments</param>
    public void OnMouseScroll(MouseScrollEvent e);
}
