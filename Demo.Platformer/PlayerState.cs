namespace Demo.Platformer;

struct PlayerState
{
    public float Speed;
    public float JumpForce;
    public bool FacingRight;
    public float JumpBufferTimer;

    public static PlayerState Create() => new()
    {
        Speed = 250f,
        JumpForce = -420f,
        FacingRight = true,
        JumpBufferTimer = 0f,
    };
}
