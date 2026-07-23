namespace Engine.Math;

public struct Color : IEquatable<Color>
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public Color(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(float r, float g, float b, float a = 1.0f)
    {
        R = (byte)(r * 255);
        G = (byte)(g * 255);
        B = (byte)(b * 255);
        A = (byte)(a * 255);
    }

    // ─── Static Constants ───────────────────────────────────────────

    public static Color White => new(255, 255, 255);
    public static Color Black => new(0, 0, 0);
    public static Color Red => new(255, 0, 0);
    public static Color Green => new(0, 255, 0);
    public static Color Blue => new(0, 0, 255);
    public static Color Yellow => new(255, 255, 0);
    public static Color Cyan => new(0, 255, 255);
    public static Color Magenta => new(255, 0, 255);
    public static Color Transparent => new(0, 0, 0, 0);

    // ─── Methods ────────────────────────────────────────────────────

    public static Color FromHex(uint hex)
    {
        return new Color(
            (byte)((hex >> 24) & 0xFF),
            (byte)((hex >> 16) & 0xFF),
            (byte)((hex >> 8) & 0xFF),
            (byte)(hex & 0xFF)
        );
    }

    public static Color Lerp(Color a, Color b, float t)
    {
        return new Color(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t),
            (byte)(a.A + (b.A - a.A) * t)
        );
    }

    // ─── Equality ───────────────────────────────────────────────────

    public bool Equals(Color other)
        => R == other.R && G == other.G && B == other.B && A == other.A;

    public override bool Equals(object? obj)
        => obj is Color c && Equals(c);

    public override int GetHashCode()
        => HashCode.Combine(R, G, B, A);

    public override string ToString() => $"Color({R}, {G}, {B}, {A})";

    public static bool operator ==(Color a, Color b) => a.Equals(b);
    public static bool operator !=(Color a, Color b) => !a.Equals(b);
}
