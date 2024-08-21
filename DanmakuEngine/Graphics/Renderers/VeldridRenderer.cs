using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using DanmakuEngine.Engine;
using DanmakuEngine.Engine.Platform;
using DanmakuEngine.Engine.Windowing;
using DanmakuEngine.Logging;
using OpenTK.Graphics.ES11;
using Silk.NET.SDL;
using Veldrid;
using Veldrid.OpenGL;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using PixelFormat = Veldrid.PixelFormat;
using Sdl = Silk.NET.SDL.Sdl;
using Sdl2Window = DanmakuEngine.Engine.Windowing.Sdl2Window;
using WMType = Silk.NET.SDL.SysWMType;


namespace DanmakuEngine.Graphics.Renderers;

public class VeldridRenderer : Renderer
{
    private Sdl2Window _window;
    private Sdl _sdl;

    private GraphicsBackend _prefferedBackend;

    private GraphicsDevice _device = null!;
    private Swapchain _swapchain = null!;
    private CommandList _commandList = null!;

    private GraphicsPipelineDescription _pipeline = new GraphicsPipelineDescription
    {
        RasterizerState = RasterizerStateDescription.CullNone,
        BlendState = BlendStateDescription.SingleOverrideBlend,
        ShaderSet = { VertexLayouts = new VertexLayoutDescription[1] }
    };

    private Swapchain swapchain => _device.MainSwapchain;

    private ResourceFactory factory => _device.ResourceFactory;

    public GraphicsBackend GraphicsBackend => _device.BackendType;

    public override bool VSync
    {
        get => swapchain.SyncToVerticalBlank;
        set => swapchain.SyncToVerticalBlank = value;
    }

    public static VeldridRenderer Create(Sdl2Window window, GraphicsBackend backend = GraphicsBackend.OpenGL)
    {
        Logger.Debug($"Creating Veldrid Graphics Device with backend {backend}");

        var renderer = new VeldridRenderer(window, backend);

        renderer.Initialize();

        return renderer;
    }

    private VeldridRenderer(Sdl2Window window, GraphicsBackend backend = GraphicsBackend.OpenGL)
    {
        _sdl = SDL.Api;
        _window = window;
        _prefferedBackend = backend;
    }

    public unsafe OpenGLPlatformInfo GetOpenGLPlatformInfo(GraphicsDeviceOptions options)
    {
        _sdl.ClearError();

        Debug.Assert(_prefferedBackend is GraphicsBackend.OpenGL or GraphicsBackend.OpenGLES);

        fixed (SysWMInfo* sysWMinfo = stackalloc SysWMInfo[1])
        {
            _sdl.GetVersion(&sysWMinfo->Version);
            _sdl.GetWindowWMInfo((Window*)_window.Handle, sysWMinfo);

            VeldridStartup.SetSDLGLContextAttributes(options, _prefferedBackend);

            var sdl2GlContext = new SdlGlWindowContext(_sdl, (Window*)_window.Handle, null!);

            return new OpenGLPlatformInfo(
                      openGLContextHandle: sdl2GlContext.Handle,
                      getProcAddress: sdl2GlContext.GetProcAddress,
                      makeCurrent: ctx => _sdl.GLMakeCurrent((Window*)_window.Handle, (void*)ctx),
                      getCurrentContext: sdl2GlContext.GetCurrentContext,
                      clearCurrentContext: sdl2GlContext.Clear,
                      deleteContext: ctx => _sdl.GLDeleteContext((void*)ctx),
                      swapBuffers: sdl2GlContext.SwapBuffers,
                      setSyncToVerticalBlank: v => sdl2GlContext.VSync = v);
        }
    }

    public static unsafe SwapchainSource GetSwapchainSource(IntPtr window, Sdl sdl)
    {
        IntPtr sdlWindowHandle = window;

        fixed (SysWMInfo* sysWMinfo = stackalloc SysWMInfo[1])
        {
            sdl.GetVersion(&sysWMinfo->Version);
            sdl.GetWindowWMInfo((Window*)window, sysWMinfo);

            switch (sysWMinfo->Subsystem)
            {
                case WMType.Windows:
                {
                    Win32WindowInfo win32WindowInfo = Unsafe.Read<Win32WindowInfo>(&sysWMinfo->Info);
                    return SwapchainSource.CreateWin32(win32WindowInfo.Sdl2Window, win32WindowInfo.hinstance);
                }

                case WMType.X11:
                {
                    X11WindowInfo x11WindowInfo = Unsafe.Read<X11WindowInfo>(&sysWMinfo->Info);
                    return SwapchainSource.CreateXlib(x11WindowInfo.display, x11WindowInfo.Sdl2Window);
                }

                case WMType.Wayland:
                {
                    WaylandWindowInfo waylandWindowInfo = Unsafe.Read<WaylandWindowInfo>(&sysWMinfo->Info);
                    return SwapchainSource.CreateWayland(waylandWindowInfo.display, waylandWindowInfo.surface);
                }

                default:
                {
                    if (RuntimeInfo.IsWindows)
                        goto case WMType.Windows;
                    else if (RuntimeInfo.IsLinux)
                    {
                        Logger.Warn("============== ATTENTION ==============");
                        Logger.Warn("Failed to determine WM, defaulting to X11.");
                        Logger.Warn("=======================================");

                        goto case WMType.X11;
                    }
                    else
                        throw new PlatformNotSupportedException($"How did you reach here? WM: {sysWMinfo->Subsystem}");
                }
            }
        }
    }

    public override void Initialize()
    {
        if (Initialized)
            return;

        Logger.Debug($"Initializing Veldrid({_prefferedBackend}) Renderer");

        var options = new GraphicsDeviceOptions
        {
            HasMainSwapchain = true,
            SwapchainDepthFormat = PixelFormat.R16_UNorm,
            SyncToVerticalBlank = true,
            ResourceBindingModel = ResourceBindingModel.Improved,
        };

        var size = _window.Size;

        var swapchain = new SwapchainDescription
        {
            Width = (uint)size.X,
            Height = (uint)size.Y,
            ColorSrgb = options.SwapchainSrgbFormat,
            DepthFormat = options.SwapchainDepthFormat,
            SyncToVerticalBlank = options.SyncToVerticalBlank,
            Source = GetSwapchainSource(_window.Handle, _sdl),
        };

        try
        {
            switch (_prefferedBackend)
            {
                case GraphicsBackend.OpenGL:
                    _device = GraphicsDevice.CreateOpenGL(options, GetOpenGLPlatformInfo(options),
                                                        swapchain.Width, swapchain.Height);
                    break;

                case GraphicsBackend.Vulkan:
                    if (_window.WMInfo.Subsystem is WMType.X11)
                    {
                        Logger.Warn("============== ATTENTION ==============");
                        Logger.Warn("Vulkan support on X11 may be unuseable");
                        Logger.Warn("=======================================");
                    }

                    _device = GraphicsDevice.CreateVulkan(options, swapchain);
                    break;

                case GraphicsBackend.Direct3D11:
                    _device = GraphicsDevice.CreateD3D11(options, swapchain);
                    break;

                default:
                    _device = GraphicsDevice.CreateVulkan(options, swapchain);
                    break;
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to create {GraphicsBackend}({_window.WMInfo.Subsystem}) Graphics Device: {e.Message}");
            throw;
        }

        _swapchain = _device.MainSwapchain;

        _commandList = factory.CreateCommandList();

        // BufferUpdateCommands = factory.CreateCommandList();
        // TextureUpdateCommands = factory.CreateCommandList();

        _pipeline.Outputs = _device.SwapchainFramebuffer.OutputDescription;

        Logger.Debug($@"Graphics Info:");
        Logger.Debug($@"  API     :    {_device.BackendType}");
        Logger.Debug($@"  Version :    {_device.ApiVersion}");
        Logger.Debug($@"  Device  :    {_device.DeviceName}");
        Logger.Debug($@"  Vendor  :    {_device.VendorName}");
        if (_device.BackendType is GraphicsBackend.OpenGL or GraphicsBackend.OpenGLES)
            Logger.Debug($@"  GLSL    :    {_device.GetOpenGLInfo().ShadingLanguageVersion}");
        Logger.Debug($@"  Binding :    Veldrid");

        Initialized = true;
    }

    public override void UnbindCurrent()
    {
        // Handled by Veldrid internally
    }

    public override void MakeCurrent()
    {
        _commandList.SetFramebuffer(_device.SwapchainFramebuffer);
        // Handled by Veldrid internally
    }

    public override void SwapBuffers()
        => _device.SwapBuffers();

    public override Texture CreateTexture(int width, int height)
    {
        throw new NotImplementedException();

        // TextureDescription textureDescription = new TextureDescription(
        //     (uint)width, (uint)height, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D);

        // var veldrid_texture = _device.ResourceFactory.CreateTexture(textureDescription);
    }

    public override void BindTexture(Texture texture)
    {
        // In Veldrid, textures are bound to the pipeline through resource sets, not directly.
        // So, this method should return a ResourceSet, not a bool.
        // This requires changing the method signature and possibly other parts of your code.
        // ResourceLayout layout = _device.ResourceFactory.CreateResourceLayout(
        //     new ResourceLayoutDescription(new TextureLayoutElement("Texture")));
        // ResourceSet set = _device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout, texture));
        // return set;

        throw new NotImplementedException();
    }

    public override void BeginFrame()
    {
        _commandList.Begin();

        // TODO: This is not correct
        // _commandList.SetFramebuffer(_swapchain.Framebuffer);
    }

    public override void EndFrame()
    {
        _commandList.End();

        _device.SubmitCommands(_commandList);

        // Called in the Draw Loop
        // SwapBuffers();
    }

    protected override void WaitForVSyncInternal()
    {
        // Veldrid does not have a concept of waiting for VSync.
        // it is handled by the Swapchain.
    }

    private RgbaFloat _clearColor = RgbaFloat.Black;

    public override void ClearScreen()
        => _commandList.ClearColorTarget(0, _clearColor);

    public override void SetClearColor(float r, float g, float b, float a)
        => _clearColor = new RgbaFloat(r, g, b, a);

    public override void ClearScreen(float r, float g, float b, float a)
    {
        _commandList.ClearColorTarget(0, new RgbaFloat(r, g, b, a));
    }

    public override Texture[] GetAllTextures()
    {
        throw new NotImplementedException();
    }

    public override void Viewport(int x, int y, int width, int height)
    {
        _commandList.SetViewport(0, new Viewport(x, y, width, height, -1, 1));
    }

    public override void Dispose()
    {
        _commandList.Dispose();

        // _pipeline?.Dispose();

        _device.Dispose();
    }
}
