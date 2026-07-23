namespace Engine.Math;

public readonly struct Vector2 : IEquatable<Vector2>
{
    public readonly float X;
    public readonly float Y;

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    // ─── Static Constants ───────────────────────────────────────────

    public static Vector2 Zero => new(0, 0);
    public static Vector2 One => new(1, 1);
    public static Vector2 Up => new(0, -1);
    public static Vector2 Down => new(0, 1);
    public static Vector2 Left => new(-1, 0);
    public static Vector2 Right => new(1, 0);

    // ─── Operators ──────────────────────────────────────────────────

    public static Vector2 operator +(Vector2 a, Vector2 b)
        => new(a.X + b.X, a.Y + b.Y);

    public static Vector2 operator -(Vector2 a, Vector2 b)
        => new(a.X - b.X, a.Y - b.Y);

    public static Vector2 operator *(Vector2 v, float s)
        => new(v.X * s, v.Y * s);

    public static Vector2 operator *(float s, Vector2 v)
        => new(v.X * s, v.Y * s);

    public static Vector2 operator /(Vector2 v, float s)
        => new(v.X / s, v.Y / s);

    public static Vector2 operator -(Vector2 v)
        => new(-v.X, -v.Y);

    public static bool operator ==(Vector2 a, Vector2 b)
        => a.X == b.X && a.Y == b.Y;

    public static bool operator !=(Vector2 a, Vector2 b)
        => !(a == b);

    // ─── Methods ────────────────────────────────────────────────────

    public float Length()
        => MathF.Sqrt(X * X + Y * Y);

    public float LengthSquared()
        => X * X + Y * Y;

    public Vector2 Normalized()
    {
        float len = Length();
        if (len < 0.0001f) return Zero;
        return new(X / len, Y / len);
    }

    // ─── Static Methods ─────────────────────────────────────────────

    public static float Distance(Vector2 a, Vector2 b)
        => (a - b).Length();

    public static float DistanceSquared(Vector2 a, Vector2 b)
        => (a - b).LengthSquared();

    public static float Dot(Vector2 a, Vector2 b)
        => a.X * b.X + a.Y * b.Y;

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        => a + (b - a) * t;

    public static Vector2 Min(Vector2 a, Vector2 b)
        => new(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y));

    public static Vector2 Max(Vector2 a, Vector2 b)
        => new(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y));

    // ─── Equality ───────────────────────────────────────────────────

    public bool Equals(Vector2 other)
        => X == other.X && Y == other.Y;

    public override bool Equals(object? obj)
        => obj is Vector2 v && Equals(v);

    public override int GetHashCode()
        => HashCode.Combine(X, Y);

    public override string ToString() => $"({X:F2}, {Y:F2})";
}
