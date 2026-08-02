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

public enum BodyType { Dynamic, Static, Kinematic }

public struct RigidBody
{
    public BodyType Type;
    public float Mass,
        GravityScale,
        Bounce,
        Friction;
    public bool UseGravity;
    public float ForceX, ForceY;
}

public struct Grounded { public bool Value; }
