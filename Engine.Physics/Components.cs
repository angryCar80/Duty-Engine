using Engine.Math;

namespace Engine.Physics;

public struct Position { public Vector2 Value; }

public struct Velocity { public float VX, VY; }

public struct BoxCollider
{
    public float Width, Height, OffsetX, OffsetY;
    public bool IsTrigger;
    public bool IsOneWay;
}

public struct RigidBody
{
    public float Mass;
    public float GravityScale;
    public float Bounce;
    public float Friction;
    public bool UseGravity;
    public float ForceX, ForceY;
}

public struct Grounded { public bool Value; }
