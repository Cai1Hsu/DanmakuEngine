using System.Numerics;
using System.Runtime.InteropServices;
using DanmakuEngine.Logging;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace DanmakuEngine.DearImgui.Graphics;

/// <summary>
/// A near zero-cost abstraction for OpenGL state management.
/// Do your opeations in a using block to ensure the state is restored.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe ref struct ImguiGLState
{
    private bool _disposed = false;
    private readonly GL _gl;
    private readonly int lastActiveTexture;
    private readonly int lastProgram;
    private readonly int lastTexture;
    private readonly int lastSampler;
    private readonly int lastArrayBuffer;
    private readonly int lastVertexArrayObject;
    private readonly Vector4D<int> lastScissorBox;
    private readonly Vector2D<int> lastPolygonMode;
    private readonly int lastBlendSrcRgb;
    private readonly int lastBlendDstRgb;
    private readonly int lastBlendSrcAlpha;
    private readonly int lastBlendDstAlpha;
    private readonly int lastBlendEquationRgb;
    private readonly int lastBlendEquationAlpha;
    private readonly bool lastEnableBlend;
    private readonly bool lastEnableCullFace;
    private readonly bool lastEnableDepthTest;
    private readonly bool lastEnableStencilTest;
    private readonly bool lastEnableScissorTest;
    private readonly bool lastEnablePrimitiveRestart;

    internal ImguiGLState(GL gl)
    {
        _gl = gl;

        _gl.GetInteger(GLEnum.ActiveTexture, out lastActiveTexture);
        _gl.ActiveTexture(GLEnum.Texture0);

        _gl.GetInteger(GLEnum.CurrentProgram, out lastProgram);
        _gl.GetInteger(GLEnum.TextureBinding2D, out lastTexture);

        _gl.GetInteger(GLEnum.SamplerBinding, out lastSampler);

        _gl.GetInteger(GLEnum.ArrayBufferBinding, out lastArrayBuffer);
        _gl.GetInteger(GLEnum.VertexArrayBinding, out lastVertexArrayObject);

        _gl.GetInteger(GLEnum.PolygonMode, out lastPolygonMode.X);

        _gl.GetInteger(GLEnum.ScissorBox, out lastScissorBox.X);

        _gl.GetInteger(GLEnum.BlendSrcRgb, out lastBlendSrcRgb);
        _gl.GetInteger(GLEnum.BlendDstRgb, out lastBlendDstRgb);

        _gl.GetInteger(GLEnum.BlendSrcAlpha, out lastBlendSrcAlpha);
        _gl.GetInteger(GLEnum.BlendDstAlpha, out lastBlendDstAlpha);

        _gl.GetInteger(GLEnum.BlendEquationRgb, out lastBlendEquationRgb);
        _gl.GetInteger(GLEnum.BlendEquationAlpha, out lastBlendEquationAlpha);

        lastEnableBlend = _gl.IsEnabled(GLEnum.Blend);
        lastEnableCullFace = _gl.IsEnabled(GLEnum.CullFace);
        lastEnableDepthTest = _gl.IsEnabled(GLEnum.DepthTest);
        lastEnableStencilTest = _gl.IsEnabled(GLEnum.StencilTest);
        lastEnableScissorTest = _gl.IsEnabled(GLEnum.ScissorTest);
        lastEnablePrimitiveRestart = _gl.IsEnabled(GLEnum.PrimitiveRestart);
    }

    public void Dispose()
    {
        if (_gl is null || _disposed)
            return;

        _disposed = true;

        _gl.UseProgram((uint)lastProgram);
        _gl.BindTexture(GLEnum.Texture2D, (uint)lastTexture);

        _gl.BindSampler(0, (uint)lastSampler);

        _gl.ActiveTexture((GLEnum)lastActiveTexture);

        _gl.BindVertexArray((uint)lastVertexArrayObject);

        _gl.BindBuffer(GLEnum.ArrayBuffer, (uint)lastArrayBuffer);
        _gl.BlendEquationSeparate((GLEnum)lastBlendEquationRgb, (GLEnum)lastBlendEquationAlpha);
        _gl.BlendFuncSeparate((GLEnum)lastBlendSrcRgb, (GLEnum)lastBlendDstRgb, (GLEnum)lastBlendSrcAlpha, (GLEnum)lastBlendDstAlpha);

        if (lastEnableBlend)
            _gl.Enable(GLEnum.Blend);
        else
            _gl.Disable(GLEnum.Blend);

        if (lastEnableCullFace)
            _gl.Enable(GLEnum.CullFace);
        else
            _gl.Disable(GLEnum.CullFace);

        if (lastEnableDepthTest)
            _gl.Enable(GLEnum.DepthTest);
        else
            _gl.Disable(GLEnum.DepthTest);
        if (lastEnableStencilTest)
            _gl.Enable(GLEnum.StencilTest);
        else
            _gl.Disable(GLEnum.StencilTest);

        if (lastEnableScissorTest)
            _gl.Enable(GLEnum.ScissorTest);
        else
            _gl.Disable(GLEnum.ScissorTest);

        if (lastEnablePrimitiveRestart)
            _gl.Enable(GLEnum.PrimitiveRestart);
        else
            _gl.Disable(GLEnum.PrimitiveRestart);

        _gl.PolygonMode(GLEnum.FrontAndBack, (GLEnum)lastPolygonMode.X);

        _gl.Scissor(lastScissorBox.X, lastScissorBox.Y, (uint)lastScissorBox.Z, (uint)lastScissorBox.W);
    }
}
