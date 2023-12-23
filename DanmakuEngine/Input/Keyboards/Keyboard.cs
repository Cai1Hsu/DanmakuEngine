using Silk.NET.SDL;
using Silk.NET.Vulkan;

namespace DanmakuEngine.Input.Keybards;

/// <summary>
/// This class stores key input that is not handled within in the frame.
/// </summary>
public static class Keyboard
{
    private static readonly Queue<EnqueuedKey> queue = new();

    /// <summary>
    /// Call this method when finished updating a frame
    /// </summary>
    public static void Clear()
    {
        while (queue.Count > 0)
            queue.Dequeue();
    }

    public static void Enqueue(EnqueuedKey key)
    {
        queue.Enqueue(key);
    }

    public static void Enqueue(KeyCode key, double timeStamp)
    {
        Enqueue(new(key, timeStamp));
    }

    public static void Dequeue()
        => queue.Dequeue();

    public static int Count()
        => queue.Count;

    public static bool Empty()
    {
        return queue.Count > 0;
    }
}
