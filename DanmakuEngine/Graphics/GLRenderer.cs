using DanmakuEngine.Graphics.Primitives;
using Silk.NET.Maths;
using Silk.NET.SDL;
using Silk.NET.OpenGL;
using DanmakuEngine.Logging;
using DanmakuEngine.Extensions;

namespace DanmakuEngine.Graphics;

public unsafe class GLRenderer : IRenderer
{
    private GL gl;

    private void* glContext;

    private Sdl _sdl;

    private Window* window;

    public GLRenderer(GL gl, void* glContext, Sdl _sdl, Window* window)
    {
        this.gl = gl;
        this.glContext = glContext;
        this._sdl = _sdl;
        this.window = window;

        Logger.Debug(
        $@"OpenGL Info:
            Version:    {ByteExtensions.BytesToString(gl.GetString(StringName.Version))}
            Renderer:   {ByteExtensions.BytesToString(gl.GetString(StringName.Renderer))}
            Vendor:     {ByteExtensions.BytesToString(gl.GetString(StringName.Vendor))}
            GLSL:       {ByteExtensions.BytesToString(gl.GetString(StringName.ShadingLanguageVersion))}
            ");
    }

    public void SwapBuffers()
        => _sdl.GLSwapWindow(window);
}
