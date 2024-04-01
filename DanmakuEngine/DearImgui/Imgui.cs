using System.Diagnostics;
using System.Runtime.InteropServices;
using DanmakuEngine.Engine;
using DanmakuEngine.Graphics.Renderers;
using DanmakuEngine.Logging;
using ImGuiNET;
using Silk.NET.Maths;
using Silk.NET.SDL;

using Renderer = DanmakuEngine.Graphics.Renderers.Renderer;
using Sdl2Window = DanmakuEngine.Engine.Windowing.Sdl2Window;

namespace DanmakuEngine.DearImgui;

public static partial class Imgui
{
    private static bool _initialized;
    public static bool Initialized => _initialized;

    private static nint _context;
    internal static nint Context => _context;

    private static ImGuiIOPtr _io;
    public static ImGuiIOPtr IO => _io;

    private static Sdl _sdl = null!;

    public static event Action? OnUpdate = null!;

    /// <summary>
    /// The window handle that ImGui uses to identify the window.
    /// It is not SDL's window magic in SDL_Window struct.
    /// </summary>
    private static IntPtr window_magic;
    internal static IntPtr WindowMagic => window_magic;

    private static bool _dockingEnabled = false;
    public static bool DockingEnabled
    {
        get => _dockingEnabled;
        set
        {
            if (value)
                _io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            else
                _io.ConfigFlags &= ~ImGuiConfigFlags.DockingEnable;

            _dockingEnabled = value;
        }
    }

    public static bool DrawMouseCursor
    {
        get => _initialized && _io.MouseDrawCursor;
        set
        {
            if (_initialized)
                _io.MouseDrawCursor = value;
        }
    }

    private static ImguiStateObject StateObject = null!;

    public static void Initialize(Sdl2Window? window, Renderer renderer)
    {
        if (window is null)
            return;

        if (_initialized)
            return;

        if (renderer is not GLSilkRenderer silkRenderer)
        {
            Logger.Warn("Dear ImGui only supports Silk.NET renderer.");
            return;
        }

        _initialized = true;

        var gl = silkRenderer.Gl;

        StateObject = new ImguiStateObject(_context, gl);

        _sdl = SDL.Api;

        _context = ImGui.CreateContext();
        ImGui.SetCurrentContext(_context);

        _io = ImGui.GetIO();

        _io.SetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(set_clipboard_text_fn_delegate);
        _io.GetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(get_clipboard_text_fn_delegate);

        loadFonts();

        _io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        if (DockingEnabled)
            _io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        // _io.MouseDrawCursor = true;

        window_magic = window.Handle;
        window.MouseMove += MouseMotionEvent;

        window.WindowSizeChanged +=
            static (w, h) => OnWindowResized(w, h);

        var window_size = window.Size;

        OnWindowResized(window_size.X, window_size.Y);

        initializeGraphics(gl);

        ImGui.StyleColorsDark();
    }

    internal static void Shutdown()
    {
        if (!_initialized)
            return;

        ImGui.DestroyContext(_context);

        _drawDataBuffers.Dispose();

#if DEBUG
        Logger.Debug($"Remained {ImguiUtils.AllocRecords.Sum(a => a.Value.Size)} bytes unreleased for ImGui");
        foreach (var (_, record) in ImguiUtils.AllocRecords)
        {
            Logger.Warn($"[Memory Leak Detected] Address: {record.Address:X}, size {record.Size}, callsite {record.Callsite}");
        }
#endif

        // Do not dispose GL resources here. The GL is managed in Render Thread.
        // disposeGLResources();
        _initialized = false;
    }
}
