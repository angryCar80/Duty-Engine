using Engine.Math;

namespace Game;

struct Player
{
    public float Speed;
    public float JumpForce;
    public float Gravity;
    public bool Grounded;
    public bool FacingRight;

    public static Player Create() => new()
    {
        Speed = 250f,
        JumpForce = -420f,
        Gravity = 980f,
        Grounded = false,
        FacingRight = true
    };
}

struct Velocity
{
    public float VX;
    public float VY;
}

struct Collider
{
    public float Width;
    public float Height;

    public Rect GetRect(Vector2 position)
        => new(position.X - Width / 2, position.Y - Height, Width, Height);
}
