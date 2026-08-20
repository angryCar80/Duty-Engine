using Engine.Math;
using System.Numerics;
using Vector3 = System.Numerics.Vector3;

namespace Engine.Rendering3D;

public sealed class Camera
{
    public Vector3 Position { get; set; } = new(0, 0, 3);
    public Vector3 Target { get; set; } = Vector3.Zero;
    public float FovDegrees { get; set; } = 60f;
    public float Near { get; set; } = 0.1f;
    public float Far { get; set; } = 100f;

    public Matrix4x4 View
        => Matrix4x4.CreateLookAt(Position, Target, Vector3.UnitY);

    public Matrix4x4 Projection(float aspect)
        => Matrix4x4.CreatePerspectiveFieldOfView(FovDegrees * MathHelper.Deg2Rad, aspect, Near, Far);

    public Matrix4x4 ViewProjection(float aspect)
        => View * Projection(aspect);
}
