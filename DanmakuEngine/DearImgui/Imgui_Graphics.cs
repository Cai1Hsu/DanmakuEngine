using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using DanmakuEngine.DearImgui.Graphics;
using DanmakuEngine.Logging;
using ImGuiNET;
using Silk.NET.OpenGL;

namespace DanmakuEngine.DearImgui;

public static partial class Imgui
{
    private static uint _vertexArray;
    private static uint _vertexBuffer;
    private static uint _indexBuffer;

    private static ImguiProgram _program = null!;

    private static ImguiFontTexture _fontTexture = null!;

    private static GL _gl = null!;

    private static unsafe void initializeGraphics(GL gl)
    {
        _gl = gl;

        _initializedGraphics = true;

        _gl.GetInteger(GLEnum.TextureBinding2D, out int lastTexture);
        _gl.GetInteger(GLEnum.ArrayBufferBinding, out int lastArrayBuffer);
        _gl.GetInteger(GLEnum.VertexArrayBinding, out int lastVertexArray);
        {
            _program = new ImguiProgram(_gl);

            _vertexBuffer = _gl.GenBuffer();
            _indexBuffer = _gl.GenBuffer();

            recreateFontDeviceTexture();
        }
        _gl.BindTexture(GLEnum.Texture2D, (uint)lastTexture);
        _gl.BindBuffer(GLEnum.ArrayBuffer, (uint)lastArrayBuffer);
        _gl.BindVertexArray((uint)lastVertexArray);
    }

    private static void recreateFontDeviceTexture()
    {
        if (!_initializedGraphics)
            return;

        // Load as RGBA 32-bit (75% of the memory is wasted, but default font is so small)
        // because it is more likely to be compatible with user's existing shaders.
        // If your ImTextureId represent a higher-level concept than just a GL texture id,
        // consider calling GetTexDataAsAlpha8() instead to save on GPU memory.
        _io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height);

        int prevActiveTexture = _gl.GetInteger(GetPName.ActiveTexture);
        _gl.ActiveTexture(TextureUnit.Texture0);
        int prevTexture2D = _gl.GetInteger(GetPName.TextureBinding2D);
        {
            _fontTexture = new ImguiFontTexture(_gl, width, height, pixels);
            _fontTexture.Bind();
            _fontTexture.SetMagFilter(TextureMagFilter.Linear);
            _fontTexture.SetMinFilter(TextureMinFilter.Linear);
        }
        _gl.BindTexture(GLEnum.Texture2D, (uint)prevTexture2D);
        _gl.ActiveTexture((TextureUnit)prevActiveTexture);

        // Store our identifier
        _io.Fonts.SetTexID((IntPtr)_fontTexture.GlTexture);
        _io.Fonts.ClearTexData();
    }

    [Conditional("Debug")]
    private static void warn(string message)
        => Logger.Warn(message);

    private static unsafe void drawImGui(ImguiDrawDataBuffer buffer)
    {
        var drawData = buffer.DrawData;

        if (buffer.Count == 0)
            return;

        var framebufferWidth = (int)(drawData.DisplaySize.X * drawData.FramebufferScale.X);
        var framebufferHeight = (int)(drawData.DisplaySize.Y * drawData.FramebufferScale.Y);

        if (framebufferWidth <= 0 || framebufferHeight <= 0)
            return;

        // Backup GL state
        using (var _ = StateObject.GLState())
        {
            ensureRenderState(drawData, framebufferWidth, framebufferHeight);

            // Will project scissor/clipping rectangles into framebuffer space
            // (0,0) unless using multi-viewports
            Vector2 clipOff = drawData.DisplayPos;
            // (1,1) unless using retina display which are often (2,2)
            Vector2 clipScale = drawData.FramebufferScale;

            // Render command lists
            for (int i = 0; i < buffer.Count; i++)
            {
                ImDrawListPtr cmdListPtr = buffer.Lists[i];

                // Upload vertex/index buffers

                _gl.BufferData(GLEnum.ArrayBuffer, (nuint)(cmdListPtr.VtxBuffer.Size * sizeof(ImDrawVert)), (void*)cmdListPtr.VtxBuffer.Data, GLEnum.StreamDraw);
                CheckGLError($"Data Vert {i}");
                _gl.BufferData(GLEnum.ElementArrayBuffer, (nuint)(cmdListPtr.IdxBuffer.Size * sizeof(ushort)), (void*)cmdListPtr.IdxBuffer.Data, GLEnum.StreamDraw);
                CheckGLError($"Data Idx {i}");

                for (int cmd_i = 0; cmd_i < cmdListPtr.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr cmdPtr = cmdListPtr.CmdBuffer[cmd_i];

                    if (cmdPtr.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        Vector4 clipRect;
                        clipRect.X = (cmdPtr.ClipRect.X - clipOff.X) * clipScale.X;
                        clipRect.Y = (cmdPtr.ClipRect.Y - clipOff.Y) * clipScale.Y;
                        clipRect.Z = (cmdPtr.ClipRect.Z - clipOff.X) * clipScale.X;
                        clipRect.W = (cmdPtr.ClipRect.W - clipOff.Y) * clipScale.Y;

                        if (clipRect.X < framebufferWidth && clipRect.Y < framebufferHeight && clipRect.Z >= 0.0f && clipRect.W >= 0.0f)
                        {
                            // Apply scissor/clipping rectangle
                            _gl.Scissor((int)clipRect.X, (int)(framebufferHeight - clipRect.W), (uint)(clipRect.Z - clipRect.X), (uint)(clipRect.W - clipRect.Y));
                            CheckGLError("Scissor");

                            // Bind texture, Draw
                            _gl.BindTexture(GLEnum.Texture2D, (uint)cmdPtr.TextureId);
                            CheckGLError("Texture");

                            _gl.DrawElementsBaseVertex(GLEnum.Triangles, cmdPtr.ElemCount, GLEnum.UnsignedShort, (void*)(cmdPtr.IdxOffset * sizeof(ushort)), (int)cmdPtr.VtxOffset);
                            CheckGLError("Draw");
                        }
                    }
                }
            }

            // Destroy the temporary VAO
            _gl.DeleteVertexArray(_vertexArray);
            _vertexArray = 0;
        }
    }

    private static unsafe void ensureRenderState(ImDrawDataPtr drawData, int framebufferWidth, int framebufferHeight)
    {
        _gl.Enable(GLEnum.Blend);
        _gl.BlendEquation(GLEnum.FuncAdd);
        _gl.BlendFuncSeparate(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha, GLEnum.One, GLEnum.OneMinusSrcAlpha);
        _gl.Disable(GLEnum.CullFace);
        _gl.Disable(GLEnum.DepthTest);
        _gl.Disable(GLEnum.StencilTest);
        _gl.Enable(GLEnum.ScissorTest);

        _gl.Disable(GLEnum.PrimitiveRestart);
        _gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);

        float L = drawData.DisplayPos.X;
        float R = drawData.DisplayPos.X + drawData.DisplaySize.X;
        float T = drawData.DisplayPos.Y;
        float B = drawData.DisplayPos.Y + drawData.DisplaySize.Y;

        float* orthoProjection = stackalloc float[]
        {
            2.0f / (R - L)    , 0.0f              ,  0.0f , 0.0f,
            0.0f              , 2.0f / (T - B)    ,  0.0f , 0.0f,
            0.0f              , 0.0f              , -1.0f , 0.0f,
            (R + L) / (L - R) , (T + B) / (B - T) ,  0.0f , 1.0f,
        };

        _program.Use();
        _program.UniformTexture(0);
        _program.UniformProjectionMatrix(orthoProjection);

        CheckGLError("Use program");

        _gl.BindSampler(0, 0);

        // Setup desired GL state
        // Recreate the VAO every time (this is to easily allow multiple GL contexts to be rendered to. VAO are not shared among GL contexts)
        // The renderer would actually work without any VAO bound, but then our VertexAttrib calls would overwrite the default one currently bound.
        _vertexArray = _gl.GenVertexArray();
        _gl.BindVertexArray(_vertexArray);
        CheckGLError("VAO");

        _gl.BindBuffer(GLEnum.ArrayBuffer, _vertexBuffer);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, _indexBuffer);

        _gl.EnableVertexAttribArray(_program.vPosLocation);
        _gl.EnableVertexAttribArray(_program.vUVLocation);
        _gl.EnableVertexAttribArray(_program.vColorLocation);
        _gl.VertexAttribPointer(_program.vPosLocation, 2, GLEnum.Float, false, (uint)sizeof(ImDrawVert), (void*)0);
        _gl.VertexAttribPointer(_program.vUVLocation, 2, GLEnum.Float, false, (uint)sizeof(ImDrawVert), (void*)8);
        _gl.VertexAttribPointer(_program.vColorLocation, 4, GLEnum.UnsignedByte, true, (uint)sizeof(ImDrawVert), (void*)16);
    }

    private static volatile bool _initializedGraphics = false;

    internal static void DisposeGLResources()
    {
        if (_initializedGraphics)
            return;
        _initializedGraphics = false;

        using (var _ = StateObject.GLState())
        {
            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            _gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            _gl.BindVertexArray(0);

            _gl.DeleteBuffer(_vertexBuffer);
            _gl.DeleteBuffer(_indexBuffer);
            _gl.DeleteVertexArray(_vertexArray);

            _gl.BindTexture(GLEnum.Texture2D, 0);
            _fontTexture.Dispose();

            _gl.UseProgram(0);
            _program.Dispose();
        }
    }

#pragma warning disable
    [Conditional("DEBUG")]
    private static void CheckGLError(string message = "")
    {
        var error = _gl.GetError();
        if (error == GLEnum.NoError)
            return;

        var stackTrace = new StackTrace();

        Logger.Error($"{message}, OpenGL error: {error}\n{stackTrace}");
    }
#pragma warning restore
}
