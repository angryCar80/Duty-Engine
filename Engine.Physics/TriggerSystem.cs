using Engine.Ecs;
using Engine.Math;

namespace Engine.Physics;

public static class TriggerSystem
{
    private static readonly HashSet<(Entity, Entity)> Active = new();
    private static readonly List<ColliderEntry> Entries = new();

    public static void Update(World world, CollisionReport report)
    {
        Entries.Clear();

        world.Query<Position, BoxCollider>().ForEachEntity((entities, positions, colliders) =>
        {
            for (int i = 0; i < entities.Length; i++)
            {
                Entries.Add(new ColliderEntry
                {
                    Entity = entities[i],
                    Aabb = PhysicsSystem.GetAABB(positions[i].Value, colliders[i]),
                    Collider = colliders[i],
                    Type = BodyType.Static
                });
            }
        });

        var seen = new HashSet<(Entity, Entity)>();

        for (int i = 0; i < Entries.Count; i++)
        {
            for (int j = i + 1; j < Entries.Count; j++)
            {
                var a = Entries[i];
                var b = Entries[j];

                if (a.Collider.IsTrigger == b.Collider.IsTrigger) continue;
                if (!a.Aabb.Intersects(b.Aabb)) continue;

                var trigger = a.Collider.IsTrigger ? a : b;
                var other = a.Collider.IsTrigger ? b : a;

                var key = (trigger.Entity, other.Entity);
                seen.Add(key);

                if (!Active.Contains(key))
                {
                    report.Triggers.Add(new TriggerEvent
                    {
                        Trigger = trigger.Entity,
                        Other = other.Entity,
                        Entered = true,
                        Point = OverlapPoint(a.Aabb, b.Aabb)
                    });
                }
            }
        }

        foreach (var key in Active)
        {
            if (!seen.Contains(key))
            {
                report.Triggers.Add(new TriggerEvent
                {
                    Trigger = key.Item1,
                    Other = key.Item2,
                    Exited = true
                });
            }
        }

        Active.Clear();
        foreach (var key in seen)
            Active.Add(key);
    }

    private static Vector2 OverlapPoint(Rect a, Rect b)
    {
        float x = MathF.Max(a.Left, b.Left);
        float y = MathF.Max(a.Top, b.Top);
        float w = MathF.Min(a.Right, b.Right) - x;
        float h = MathF.Min(a.Bottom, b.Bottom) - y;
        return new Vector2(x + w / 2, y + h / 2);
    }
}
