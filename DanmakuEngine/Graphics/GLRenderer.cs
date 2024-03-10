using System.Numerics;
using DanmakuEngine.Extensions;
using DanmakuEngine.Graphics.Primitives;
using DanmakuEngine.Logging;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.SDL;

namespace DanmakuEngine.Graphics;

public unsafe class GLRenderer : IRenderer
{
    private GL gl;

    private void* glContext;

    private Sdl _sdl;

    private Window* window;

    /// <summary>
    /// Represent the width of the graph to draw
    /// </summary>
    public uint Width { get; private set; }

    /// <summary>
    /// Represent the height of the graph to draw
    /// </summary>
    public uint Height { get; private set; }

    public GLRenderer(GL gl, void* glContext, Sdl _sdl, Window* window, Vector2D<int> size)
    {
        this.gl = gl;
        this.glContext = glContext;
        this._sdl = _sdl;
        this.window = window;

        Logger.Debug(
        $@"OpenGL Info:
            Version:    {ByteExtensions.ToString(gl.GetString(StringName.Version))}
            Renderer:   {ByteExtensions.ToString(gl.GetString(StringName.Renderer))}
            Vendor:     {ByteExtensions.ToString(gl.GetString(StringName.Vendor))}
            GLSL:       {ByteExtensions.ToString(gl.GetString(StringName.ShadingLanguageVersion))}
            ");
    }

    public void SwapBuffers()
        => _sdl.GLSwapWindow(window);

    public void OnResized(uint width, uint height)
        => (this.Width, this.Height) = (width, height);
}
