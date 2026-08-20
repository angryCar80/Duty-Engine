using Silk.NET.OpenGL;

namespace Engine.Rendering3D;

public sealed class Shader
{
    public uint Handle { get; private set; }
    private readonly GL _gl;

    private Shader(GL gl, uint handle)
    {
        _gl = gl;
        Handle = handle;
    }

    public static Shader Create(GL gl, string vertexSource, string fragmentSource)
    {
        uint vs = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vs, vertexSource);
        gl.CompileShader(vs);
        CheckShader(gl, vs, "vertex");

        uint fs = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fs, fragmentSource);
        gl.CompileShader(fs);
        CheckShader(gl, fs, "fragment");

        uint program = gl.CreateProgram();
        gl.AttachShader(program, vs);
        gl.AttachShader(program, fs);
        gl.LinkProgram(program);
        CheckProgram(gl, program);

        gl.DeleteShader(vs);
        gl.DeleteShader(fs);

        return new Shader(gl, program);
    }

    public int GetUniformLocation(string name) => _gl.GetUniformLocation(Handle, name);
    public void Use() => _gl.UseProgram(Handle);

    private static void CheckShader(GL gl, uint shader, string kind)
    {
        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        if (status == 0)
            throw new Exception($"{kind} shader compile failed: {gl.GetShaderInfoLog(shader)}");
    }
    private static void CheckProgram(GL gl, uint program)
    {
        gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int status);
        if (status == 0)
            throw new Exception($"program link failed: {gl.GetProgramInfoLog(program)}");
    }
}

