using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;

using Shader = DanmakuEngine.Graphics.Shaders.Shader;

namespace DanmakuEngine.DearImgui.Graphics;

/// <summary>
/// Represent a shader pragrom for ImGui.
/// </summary>
public partial class ImguiProgram : Shader
{
    internal readonly int uProjMtxLocation;
    internal readonly int uTextureLocation;

    internal readonly uint vPosLocation;
    internal readonly uint vUVLocation;
    internal readonly uint vColorLocation;

#pragma warning disable
    private const string projMtx = "uProjMtx";
    private const string texture = "uTexture";
    private const string vPos = "vPos";
    private const string vUV = "vUV";
    private const string vColor = "vColor";
#pragma warning restore

    public ImguiProgram(GL gl)
        : base(gl, vs, fs)
    {
        uProjMtxLocation = _gl.GetUniformLocation(_handle, projMtx);
        uTextureLocation = _gl.GetUniformLocation(_handle, texture);

        vPosLocation = (uint)_gl.GetAttribLocation(_handle, vPos);
        vUVLocation = (uint)_gl.GetAttribLocation(_handle, vUV);
        vColorLocation = (uint)_gl.GetAttribLocation(_handle, vColor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe void UniformProjectionMatrix(float* projMtx)
        => _gl.UniformMatrix4(uProjMtxLocation, 1, false, projMtx);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UniformTexture(int textureUnit = 0)
        => _gl.Uniform1(uTextureLocation, textureUnit);
}
