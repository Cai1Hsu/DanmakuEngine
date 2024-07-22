using Silk.NET.Maths;

namespace DanmakuEngine.Input.EventReceivers;

public interface IReceiveMousePosition : IReceiveEvent
{
    /// <summary>
    /// Called when the mouse position is received.
    /// Only called when the mouse is moved.
    /// </summary>
    /// <param name="viewportPosition">The position of the mouse in the viewport</param>
    /// <param name="worldPosition">The position of the mouse in the world</param>
    public void OnReceiveMousePosition(Vector2D<float> viewportPosition, Vector2D<float> worldPosition);
}
