// from silk.NET

using Silk.NET.OpenGL;

namespace DanmakuEngine.Graphics.Shaders;
public class Shader : IDisposable
{
    //Our handle and the GL instance this class will use, these are private because they have no reason to be public.
    //Most of the time you would want to abstract items to make things like this invisible.
    protected uint _handle;
    protected GL _gl;

    public Shader(GL gl, string vertexSrc, string fragmentSrc)
    {
        _gl = gl;

        // Load the individual shaders.
        uint vertex = LoadShader(ShaderType.VertexShader, vertexSrc);
        uint fragment = LoadShader(ShaderType.FragmentShader, fragmentSrc);

        // Create the shader program.
        _handle = _gl.CreateProgram();

        // Attach the individual shaders.
        _gl.AttachShader(_handle, vertex);
        _gl.AttachShader(_handle, fragment);
        _gl.LinkProgram(_handle);

        // Check for linking errors.
        _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
            throw new Exception($"Program failed to link with error: {_gl.GetProgramInfoLog(_handle)}");

        // Detach and delete the shaders
        // Only leave the program
        _gl.DetachShader(_handle, vertex);
        _gl.DetachShader(_handle, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }

    /// <summary>
    /// Use the program
    /// </summary>
    public void Use()
        => _gl.UseProgram(_handle);

    //Uniforms are properties that applies to the entire geometry
    public void SetUniform(string name, int value)
    {
        //Setting a uniform on a shader using a name.
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1) //If GetUniformLocation returns -1 the uniform is not found.
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        _gl.Uniform1(location, value);
    }

    public void SetUniform(string name, float value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
            throw new Exception($"{name} uniform not found on shader.");

        _gl.Uniform1(location, value);
    }

    public void Dispose()
    {
        //Remember to delete the program when we are done.
        _gl.DeleteProgram(_handle);
    }

    /// <summary>
    /// Load and compile shader
    /// </summary>
    /// <param name="type">shader type</param>
    /// <param name="src">the source code of the shader</param>
    /// <returns>the handle of the shader</returns>
    /// <exception cref="Exception">Failed to compile the shader</exception>
    private uint LoadShader(ShaderType type, string src)
    {
        //To load a single shader we need to:
        //1) Load the shader from a file.
        //2) Create the handle.
        //3) Upload the source to opengl.
        //4) Compile the shader.
        //5) Check for errors.

        uint handle = _gl.CreateShader(type);

        _gl.ShaderSource(handle, src);

        _gl.CompileShader(handle);

        string infoLog = _gl.GetShaderInfoLog(handle);

        if (!string.IsNullOrWhiteSpace(infoLog))
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");

        return handle;
    }
}
