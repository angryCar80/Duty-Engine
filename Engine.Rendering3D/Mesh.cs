using Silk.NET.OpenGL;

namespace Engine.Rendering3D;

public sealed class Mesh
{
    public uint Vao { get; private set; }
    public uint IndexCount { get; private set; }

    public static Mesh CreateTriangle(GL gl)
    {
        float[] vertices =
            {
            // position               // color
             0.0f,  0.5f, 0.0f,       1.0f, 0.2f, 0.3f,
            -0.5f, -0.5f, 0.0f,       0.2f, 1.0f, 0.3f,
             0.5f, -0.5f, 0.0f,       0.3f, 0.4f, 1.0f,
        };
        uint[] indices = { 0, 1, 2 };
        return Create(gl, vertices, indices);
    }
    public static Mesh Create(GL gl, float[] vertices, uint[] indices)
    {
        var mesh = new Mesh { IndexCount = (uint)indices.Length };
        mesh.Vao = gl.GenVertexArray();
        gl.BindVertexArray(mesh.Vao);

        uint vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        gl.BufferData(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.StaticDraw);

        uint ebo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        gl.BufferData(BufferTargetARB.ElementArrayBuffer, indices, BufferUsageARB.StaticDraw);

        const uint stride = 6 * sizeof(uint);

        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, IntPtr.Zero);

        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, new IntPtr(3 * sizeof(float)));

        gl.BindVertexArray(0);
        return mesh;
    }

    public unsafe void Draw(GL gl)
    {
        gl.BindVertexArray(Vao);
        gl.DrawElements(PrimitiveType.Triangles, IndexCount, DrawElementsType.UnsignedInt, (void*)0);
    }
}
