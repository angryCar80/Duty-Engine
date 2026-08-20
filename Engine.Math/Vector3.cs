namespace Engine.Math;

public readonly struct Vector3 : IEquatable<Vector3>
{
    public readonly float X;
    public readonly float Y;
    public readonly float Z;

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    // ─── Static Constants ───────────────────────────────────────────

    public static Vector3 Zero => new(0, 0, 0);
    public static Vector3 One => new(1, 1, 1);
    public static Vector3 Up => new(0, -1, 0);
    public static Vector3 Down => new(0, 1, 0);
    public static Vector3 Left => new(-1, 0, 0);
    public static Vector3 Right => new(1, 0, 0);
    public static Vector3 Forward => new(0, 0, -1);
    public static Vector3 Back => new(0, 0, 1);

    // ─── Operators ──────────────────────────────────────────────────

    public static Vector3 operator +(Vector3 a, Vector3 b)
        => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3 operator -(Vector3 a, Vector3 b)
        => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3 operator *(Vector3 v, float s)
        => new(v.X * s, v.Y * s, v.Z * s);

    public static Vector3 operator *(float s, Vector3 v)
        => new(v.X * s, v.Y * s, v.Z * s);

    public static Vector3 operator /(Vector3 v, float s)
        => new(v.X / s, v.Y / s, v.Z / s);

    public static Vector3 operator -(Vector3 v)
        => new(-v.X, -v.Y, -v.Z);

    public static bool operator ==(Vector3 a, Vector3 b)
        => a.X == b.X && a.Y == b.Y && a.Z == b.Z;

    public static bool operator !=(Vector3 a, Vector3 b)
        => !(a == b);

    // ─── Methods ────────────────────────────────────────────────────

    public float Length()
        => MathF.Sqrt(X * X + Y * Y + Z * Z);

    public float LengthSquared()
        => X * X + Y * Y + Z * Z;

    public Vector3 Normalized()
    {
        float len = Length();
        if (len < 0.0001f) return Zero;
        return new(X / len, Y / len, Z / len);
    }

    // ─── Static Methods ─────────────────────────────────────────────

    public static float Distance(Vector3 a, Vector3 b)
        => (a - b).Length();

    public static float DistanceSquared(Vector3 a, Vector3 b)
        => (a - b).LengthSquared();

    public static float Dot(Vector3 a, Vector3 b)
        => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vector3 Cross(Vector3 a, Vector3 b)
        => new(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);

    public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        => a + (b - a) * t;

    public static Vector3 Min(Vector3 a, Vector3 b)
        => new(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y), MathF.Min(a.Z, b.Z));

    public static Vector3 Max(Vector3 a, Vector3 b)
        => new(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y), MathF.Max(a.Z, b.Z));

    // ─── Equality ───────────────────────────────────────────────────

    public bool Equals(Vector3 other)
        => X == other.X && Y == other.Y && Z == other.Z;

    public override bool Equals(object? obj)
        => obj is Vector3 v && Equals(v);

    public override int GetHashCode()
        => HashCode.Combine(X, Y, Z);

    public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
}
