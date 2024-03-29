using System.Runtime.CompilerServices;
using DanmakuEngine.DearImgui.Graphics;
using Silk.NET.OpenGL;

namespace DanmakuEngine.DearImgui;

/// <summary>
/// Used to create objects to restore ImGui context and gl states after some operations
/// </summary>
/// <param name="context"></param>
/// <param name="gl"></param>
internal class ImguiStateObject(nint context, GL gl)
{
    private readonly nint _context = context;
    private readonly GL _gl = gl;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ImguiGLState GLState()
        => new ImguiGLState(_gl);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ImguiContextState ContextState()
        => new ImguiContextState(_context);
}
