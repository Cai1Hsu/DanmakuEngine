namespace DanmakuEngine.DearImgui.Graphics;

/// <summary>
/// The source code of the vertex shader and fragment shader for ImGui program.
/// </summary>
public partial class ImguiProgram
{
    #region Vertex Shader
    private const string vs = @"#version 330 core
layout (location = 0) in vec2 vPos;
layout (location = 1) in vec2 vUV;
layout (location = 2) in vec4 vColor;

uniform mat4 uProjMtx;

out vec2 fUV;
out vec4 fColor;

void main()
{
    fUV = vUV;
    fColor = vColor;
    gl_Position = uProjMtx * vec4(vPos.xy, 0, 1);
}
";

    #endregion

    #region Fragment Shader
    private const string fs = @"#version 330 core
in vec2 fUV;
in vec4 fColor;

uniform sampler2D uTexture;

layout (location = 0) out vec4 oColor;

void main()
{
    oColor = fColor * texture(uTexture, fUV.st);
}
";

    #endregion
}
