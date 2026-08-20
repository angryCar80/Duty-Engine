using System.Numerics;
using Engine.Core;
using Silk.NET.OpenGL;

namespace Engine.Rendering3D;

public sealed class Renderer3d
{
    private readonly GL _gl;
    private readonly Shader _shader;
    private readonly Mesh _triangle;
    private readonly Camera _camera = new();
    private int _width;
    private int _height;
    private readonly float[] _mvp = new float[16];

    public Renderer3d(EngineApp engine, int width, int height)
    {
        _width = width;
        _height = height;

        _gl = GL.GetApi(engine.GetGLProcAddress);
        _gl.Viewport(0, 0, (uint)width, (uint)height);
        _gl.Enable(EnableCap.DepthTest);

        _shader = Shader.Create(_gl, VertexShaderSource, FragmentShaderSource);
        _triangle = Mesh.CreateTriangle(_gl);
    }

    public void SetViewport(int width, int height)
    {
        _width = width;
        _height = height;
        _gl.Viewport(0, 0, (uint)width, (uint)height);
    }

    public void Clear(float r, float g, float b)
    {
        _gl.ClearColor(r, g, b, 1f);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public void DrawTriangle(float angle)
    {
        float aspect = (float)_width / _height;
        Matrix4x4 mvp = Matrix4x4.CreateRotationZ(angle) * _camera.ViewProjection(aspect);

        _shader.Use();
        WriteMatrix(_mvp, mvp);
        _gl.UniformMatrix4(_shader.GetUniformLocation("uMVP"), true, _mvp);

        _triangle.Draw(_gl);
    }

    private static void WriteMatrix(float[] dst, in Matrix4x4 m)
    {
        dst[0] = m.M11; dst[1] = m.M12; dst[2] = m.M13; dst[3] = m.M14;
        dst[4] = m.M21; dst[5] = m.M22; dst[6] = m.M23; dst[7] = m.M24;
        dst[8] = m.M31; dst[9] = m.M32; dst[10] = m.M33; dst[11] = m.M34;
        dst[12] = m.M41; dst[13] = m.M42; dst[14] = m.M43; dst[15] = m.M44;
    }
    private const string VertexShaderSource = """
        #version 410 core

        layout(location = 0) in vec3 aPosition;
        layout(location = 1) in vec3 aColor;

        uniform mat4 uMVP;

        out vec3 vColor;

        void main()
        {
            vColor = aColor;
            gl_Position = uMVP * vec4(aPosition, 1.0);
        }
        """;

    private const string FragmentShaderSource = """
        #version 410 core

        in vec3 vColor;
        out vec4 fragColor;

        void main()
        {
            fragColor = vec4(vColor, 1.0);
        }
        """;

}
