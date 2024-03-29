using Silk.NET.OpenGL;
using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using PixelFormat = Silk.NET.OpenGL.PixelFormat;
using PixelType = Silk.NET.OpenGL.PixelType;

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

    public uint Handle { get; private set; }

    public TextureMinFilter MinFilter = TextureMinFilter.Nearest;

    public TextureMagFilter MagFilter = TextureMagFilter.Nearest;

    public GL _gl;

    /// <summary>
    /// Create a texture object
    /// Call this method in a using statement or you will leak memory
    /// </summary>
    /// <param name="gl">gl context</param>
    /// <param name="img">source image</param>
    public unsafe Texture(GL gl, Image<Rgba32> img, bool mipmap = true)
    {
        this._gl = gl;

        Handle = _gl.GenTexture();
        // TODO: Bind()
        // Bind the texture so that the next few commands affect this texture
        Bind();

        // allocate memory for the image
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
            (uint)img.Width, (uint)img.Height,
            0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

        img.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                fixed (void* data = accessor.GetRowSpan(y))
                {
                    // update buffer
                    gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0,
                        y, (uint)img.Height,
                        1, PixelFormat.Rgba, PixelType.UnsignedByte, data
                    );
                }
            }
        });

        SetParameters(mipmap);

        this.Width = img.Width;
        this.Height = img.Height;
    }

    public void Bind(TextureUnit slot = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(slot);
        _gl.BindTexture(TextureTarget.Texture2D, this.Handle);
    }

    public void SetFilter(TextureMinFilter min = TextureMinFilter.Nearest, TextureMagFilter mag = TextureMagFilter.Nearest)
    {
        this.MinFilter = min;
        this.MagFilter = mag;

        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)MinFilter);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)MagFilter);
    }

    public void SetParameters(bool mipmap)
    {
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

        SetFilter();

        if (!mipmap)
            return;

        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
    }

    public void Dispose()
    {
        _gl.DeleteTexture(this.Handle);
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
