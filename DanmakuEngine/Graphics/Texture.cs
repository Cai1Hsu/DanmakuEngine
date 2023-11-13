using Silk.NET.SDL;

namespace DanmakuEngine.Graphics;

public class Texture : IDisposable
{
    /// <summary>
    /// The name of the texture, used for lookup in the texture store
    /// </summary>
    public string Key { get; } = string.Empty;
    public string Name { get; } = string.Empty;

    public float Width { get; }

    public float Height { get; }

    public float Scale { get; } = 1;

    public float DisplayWidth => Width * Scale;

    public float DisplayHeight => Height * Scale;

    /// <summary>
    /// usually, a texture contains transparency, and we want to render it with transparency
    /// </summary>
    public OpacityType Opacity { get; } = OpacityType.Translucent;

    public void Dispose()
    {
        
    }
}

public enum OpacityType
{
    /// <summary>
    /// The texture is opaque, meaning it is not transparent at all
    /// </summary>
    Opaque,
    /// <summary>
    /// The texture is transparent, meaning it is completely transparent
    /// </summary>
    Transparent,
    /// <summary>
    /// The texture is translucent, meaning it is partially transparent
    /// </summary>
    Translucent
}