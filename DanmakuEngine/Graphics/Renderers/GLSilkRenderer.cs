using System.Runtime.InteropServices;
using DanmakuEngine.Dependency;
using DanmakuEngine.Engine;
using DanmakuEngine.Engine.Windowing;
using DanmakuEngine.Extensions;
using DanmakuEngine.Logging;
using Silk.NET.Core.Contexts;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using Silk.NET.SDL;

namespace DanmakuEngine.Graphics.Renderers;

public sealed unsafe class GLSilkRenderer : Renderer, IGLContextSource
{
    public GL Gl => _gl;

    public IGLContext? GLContext => _glContext;

    private Window* window => (Window*)_window.Handle;

    private Sdl _sdl;

    private GL _gl = null!;

    private IGLContext _glContext = null!;

    private Sdl2Window _window = null!;

    public override bool VSync
    {
        get => _sdl.GLGetSwapInterval() == 0;
        set => _sdl.GLSetSwapInterval(value ? 1 : 0);
    }

    public static GLSilkRenderer Create(Sdl2Window window)
    {
        var renderer = new GLSilkRenderer(window);

        renderer.Initialize();

        return renderer;
    }

    private GLSilkRenderer(Sdl2Window window)
    {
        this._window = window;
        this._sdl = SDL.Api;
    }

    public override void Initialize()
    {
        if (Initialized)
            return;

        Logger.Debug("Initializing Silk.NET OpenGL Renderer");

        var glApiFetch = new Func<GL>[]
        {
            // created GLContext in constructor
            () => GL.GetApi(_glContext = new SdlGlWindowContext(_sdl, window, this)),
            () =>
            {
                _glContext = new SdlContext(_sdl, window, this);

                // Of course it is.
                if (_glContext is SdlContext sdlContext)
                {
                    // don't know if this actually works
                    sdlContext.Create();
                }

                return GL.GetApi(_glContext);
            },
        };

        glApiFetch.Any(glFetch =>
        {
            try
            {
                return (_gl = glFetch()) != null;
            }
            catch (Exception e)
            {
                Logger.Debug($"Failed to fetch GL Api: {e.Message}");
            }

            return false;
        });

        if (_glContext.Handle == 0)
            throw new Exception("Could not create GL Context");

        if (_gl == null)
            throw new Exception("Could not fetch GL Api");

        // Do we need this?
        _sdl.GLSetAttribute(GLattr.ContextProfileMask, (int)GLprofile.Core);

        _sdl.GLSetAttribute(GLattr.ContextMajorVersion, 3);
        // The ImGui requires at least 3.3
        _sdl.GLSetAttribute(GLattr.ContextMinorVersion, 3);

        _sdl.GLSetAttribute(GLattr.RedSize, 8);
        _sdl.GLSetAttribute(GLattr.GreenSize, 8);
        _sdl.GLSetAttribute(GLattr.BlueSize, 8);
        _sdl.GLSetAttribute(GLattr.AccumAlphaSize, 0);
        _sdl.GLSetAttribute(GLattr.DepthSize, 16);

        _sdl.GLSetAttribute(GLattr.StencilSize, 8);

        Logger.Debug($@"Graphics Info:");
        Logger.Debug($@"  API     :    OpenGL");
        Logger.Debug($@"  Version :    {Marshal.PtrToStringUTF8((nint)_gl.GetString(StringName.Version))}");
        Logger.Debug($@"  Device  :    {Marshal.PtrToStringUTF8((nint)_gl.GetString(StringName.Renderer))}");
        Logger.Debug($@"  Vendor  :    {Marshal.PtrToStringUTF8((nint)_gl.GetString(StringName.Vendor))}");
        Logger.Debug($@"  GLSL    :    {Marshal.PtrToStringUTF8((nint)_gl.GetString(StringName.ShadingLanguageVersion))}");
        Logger.Debug($@"  Binding :    Silk.NET");

        Initialized = true;
    }

    public override void UnbindCurrent()
    {
        _glContext.Clear();
    }

    public override void MakeCurrent()
        => _glContext.MakeCurrent();

    public override void SwapBuffers()
        => _glContext.SwapBuffers();

    public override Texture CreateTexture(int width, int height)
    {
        throw new NotImplementedException();
    }

    public override void BindTexture(Texture texture)
    {
        _gl.BindTexture(TextureTarget.Texture2D, texture.Handle);
    }

    public override void BeginFrame()
    {
    }

    public override void EndFrame()
    {
    }

    protected override void WaitForVSyncInternal()
    {
        _gl.WaitSync(0, SyncBehaviorFlags.None, 0);
    }

    public override void ClearScreen()
    {
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit));
    }

    public override void SetClearColor(float r, float g, float b, float a)
    {
        _gl.ClearColor(r, g, b, a);
    }

    public override Texture[] GetAllTextures()
    {
        throw new NotImplementedException();
    }

    public override void ClearScreen(float r, float g, float b, float a)
    {
        SetClearColor(r, g, b, a);
        ClearScreen();
    }

    public override void Viewport(int x, int y, int width, int height)
        => _gl.Viewport(x, y, (uint)width, (uint)height);

    public override void Dispose()
    {
        _gl.Dispose();
        _glContext.Dispose();
    }
}
