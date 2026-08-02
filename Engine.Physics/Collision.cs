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
