using Silk.NET.SDL;

namespace DanmakuEngine.Input.Keybards;

public readonly struct EnqueuedKey(KeyCode key, double timeStamp)
{
    /// <summary>
    /// Keycode of the key
    /// </summary>
    public readonly KeyCode Key = key;

    /// <summary>
    /// Timestamp from SDL
    /// </summary>
    public readonly double TimeStamp = timeStamp;
}
