using System.Numerics;

namespace Engine.Math;

public struct Transform
{
    public Vector2 Position;
    public float Rotation;
    public Vector2 Scale;

    public Transform(Vector2 position, float rotation = 0f, Vector2? scale = null)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale ?? Vector2.One;
    }

    public Transform(float x, float y, float rotation = 0f, float scaleX = 1f, float scaleY = 1f)
    {
        Position = new Vector2(x, y);
        Rotation = rotation;
        Scale = new Vector2(scaleX, scaleY);
    }

    // ─── Static Defaults ────────────────────────────────────────────

    public static Transform Default => new(Vector2.Zero, 0, Vector2.One);

    // ─── Methods ────────────────────────────────────────────────────

    public Vector2 Forward
    {
        get
        {
            float cos = MathF.Cos(Rotation);
            float sin = MathF.Sin(Rotation);
            return new Vector2(cos, sin);
        }
    }

    public Vector2 Right
    {
        get
        {
            float cos = MathF.Cos(Rotation);
            float sin = MathF.Sin(Rotation);
            return new Vector2(-sin, cos);
        }
    }

    public Matrix3x2 ToMatrix()
    {
        float cos = MathF.Cos(Rotation);
        float sin = MathF.Sin(Rotation);

        return new Matrix3x2(
            cos * Scale.X, sin * Scale.X,
            -sin * Scale.Y, cos * Scale.Y,
            Position.X, Position.Y
        );
    }

    public Vector2 TransformPoint(Vector2 point)
    {
        float cos = MathF.Cos(Rotation);
        float sin = MathF.Sin(Rotation);
        float rx = point.X * cos - point.Y * sin;
        float ry = point.X * sin + point.Y * cos;
        return new Vector2(rx * Scale.X + Position.X, ry * Scale.Y + Position.Y);
    }

    public Vector2 InverseTransformPoint(Vector2 point)
    {
        float dx = point.X - Position.X;
        float dy = point.Y - Position.Y;
        float cos = MathF.Cos(-Rotation);
        float sin = MathF.Sin(-Rotation);
        float rx = dx * cos - dy * sin;
        float ry = dx * sin + dy * cos;
        return new Vector2(rx / Scale.X, ry / Scale.Y);
    }

    public override string ToString()
        => $"Transform(Pos={Position}, Rot={Rotation * MathHelper.Rad2Deg:F1}deg, Scale={Scale})";
}
