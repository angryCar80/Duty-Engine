namespace Engine.Math;

public struct Rect : IEquatable<Rect>
{
    public float X;
    public float Y;
    public float Width;
    public float Height;

    public Rect(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public Rect(Vector2 position, Vector2 size)
    {
        X = position.X;
        Y = position.Y;
        Width = size.X;
        Height = size.Y;
    }

    // ─── Properties ─────────────────────────────────────────────────

    public float Left => X;
    public float Right => X + Width;
    public float Top => Y;
    public float Bottom => Y + Height;

    public Vector2 Position
    {
        get => new(X, Y);
        set { X = value.X; Y = value.Y; }
    }

    public Vector2 Size
    {
        get => new(Width, Height);
        set { Width = value.X; Height = value.Y; }
    }

    public Vector2 Center => new(X + Width / 2, Y + Height / 2);

    // ─── Methods ────────────────────────────────────────────────────

    public bool Contains(Vector2 point)
        => point.X >= X && point.X <= X + Width
        && point.Y >= Y && point.Y <= Y + Height;

    public bool Contains(float px, float py)
        => px >= X && px <= X + Width
        && py >= Y && py <= Y + Height;

    public bool Intersects(Rect other)
        => X < other.X + other.Width
        && X + Width > other.X
        && Y < other.Y + other.Height
        && Y + Height > other.Y;

    public Rect Expand(float amount)
        => new(X - amount, Y - amount, Width + amount * 2, Height + amount * 2);

    public Rect Expand(float horizontal, float vertical)
        => new(X - horizontal, Y - vertical, Width + horizontal * 2, Height + vertical * 2);

    public Vector2 ClosestPoint(Vector2 point)
    {
        float cx = MathF.Max(X, MathF.Min(point.X, X + Width));
        float cy = MathF.Max(Y, MathF.Min(point.Y, Y + Height));
        return new Vector2(cx, cy);
    }

    public static Rect FromCenter(Vector2 center, Vector2 size)
        => new(center.X - size.X / 2, center.Y - size.Y / 2, size.X, size.Y);

    // ─── Equality ───────────────────────────────────────────────────

    public bool Equals(Rect other)
        => X == other.X && Y == other.Y
        && Width == other.Width && Height == other.Height;

    public override bool Equals(object? obj)
        => obj is Rect r && Equals(r);

    public override int GetHashCode()
        => HashCode.Combine(X, Y, Width, Height);

    public override string ToString()
        => $"Rect({X:F1}, {Y:F1}, {Width:F1}, {Height:F1})";

    public static bool operator ==(Rect a, Rect b) => a.Equals(b);
    public static bool operator !=(Rect a, Rect b) => !a.Equals(b);
}
