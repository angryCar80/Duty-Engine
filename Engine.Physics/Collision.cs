using Engine.Ecs;
using Engine.Math;

namespace Engine.Physics;

struct ColliderEntry
{
    public Entity Entity;
    public Rect Aabb;
    public BoxCollider Collider;
    public BodyType Type;
}

public struct CollisionEvent
{
    public Entity A, B;
    public Vector2 Normal;
    public float Penetration;
}

public struct TriggerEvent
{
    public Entity Trigger, Other;
    public bool Entered, Exited;
    public Vector2 Point;
}
public struct CollisionRect
{
    public float X, Y, Width, Height;
    public bool IsOneWay;

    public float Left => X;
    public float Right => X + Width;
    public float Top => Y;
    public float Bottom => Y + Height;

    public CollisionRect(float x, float y, float width, float height, bool isOneWay = false)
    {
        X = x; Y = y; Width = width; Height = height; IsOneWay = isOneWay;
    }

    public static implicit operator Rect(CollisionRect cr)
        => new(cr.X, cr.Y, cr.Width, cr.Height);
}

public class CollisionReport
{
    public List<CollisionEvent> Collisions = new();
    public List<TriggerEvent> Triggers = new();

    public void Clear()
    {
        Collisions.Clear();
        Triggers.Clear();
    }
}
