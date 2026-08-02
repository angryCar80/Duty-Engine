using Engine.Ecs;
using Engine.Math;

namespace Engine.Physics;

public static class PhysicsSystem
{
    private const float TerminalVelocity = 600f;
    private const float GroundSnap = 2f;

    private static readonly List<ColliderEntry> Entries = new();

    public static void Update(World world, float dt, List<Rect> staticColliders, CollisionReport report)
    {
        report.Clear();

        ApplyGravityAndForces(world, dt);
        MoveAndResolveX(world, dt, staticColliders);
        MoveAndResolveY(world, dt, staticColliders);
        ResolveEntityCollisions(world, report);
        TriggerSystem.Update(world, report);
        UpdateGrounded(world, staticColliders);
    }

    static void ApplyGravityAndForces(World world, float dt)
    {
        world.Query<Position, Velocity, RigidBody>().ForEach((positions, velocities, rigidBodies) =>
        {
            for (int i = 0; i < positions.Length; i++)
            {
                rigidBodies[i].ForceX = 0;
                rigidBodies[i].ForceY = 0;

                if (rigidBodies[i].Type != BodyType.Dynamic) continue;

                if (rigidBodies[i].UseGravity)
                    velocities[i].VY += 980f * rigidBodies[i].GravityScale * dt;

                if (velocities[i].VY > TerminalVelocity)
                    velocities[i].VY = TerminalVelocity;

                if (rigidBodies[i].Mass > 0)
                {
                    velocities[i].VX += rigidBodies[i].ForceX / rigidBodies[i].Mass * dt;
                    velocities[i].VY += rigidBodies[i].ForceY / rigidBodies[i].Mass * dt;
                }
            }
        });
    }

    static void MoveAndResolveX(World world, float dt, List<Rect> staticColliders)
    {
        world.Query<Position, Velocity, BoxCollider>().ForEach((positions, velocities, colliders) =>
        {
            for (int i = 0; i < positions.Length; i++)
            {
                if (colliders[i].IsTrigger) continue;

                positions[i].Value = new Vector2(
                    positions[i].Value.X + velocities[i].VX * dt,
                    positions[i].Value.Y
                );

                var aabb = GetAABB(positions[i].Value, colliders[i]);

                foreach (var plat in staticColliders)
                {
                    if (!aabb.Intersects(plat)) continue;

                    if (velocities[i].VX > 0)
                    {
                        positions[i].Value = new Vector2(
                            plat.Left - colliders[i].Width / 2 - colliders[i].OffsetX,
                            positions[i].Value.Y
                        );
                    }
                    else if (velocities[i].VX < 0)
                    {
                        positions[i].Value = new Vector2(
                            plat.Right + colliders[i].Width / 2 - colliders[i].OffsetX,
                            positions[i].Value.Y
                        );
                    }

                    velocities[i].VX = 0;
                    aabb = GetAABB(positions[i].Value, colliders[i]);
                }
            }
        });
    }

    static void MoveAndResolveY(World world, float dt, List<Rect> staticColliders)
    {
        world.Query<Position, Velocity, BoxCollider>().ForEach((positions, velocities, colliders) =>
        {
            for (int i = 0; i < positions.Length; i++)
            {
                if (colliders[i].IsTrigger) continue;

                float prevBottom = GetAABB(positions[i].Value, colliders[i]).Bottom;

                positions[i].Value = new Vector2(
                    positions[i].Value.X,
                    positions[i].Value.Y + velocities[i].VY * dt
                );

                var aabb = GetAABB(positions[i].Value, colliders[i]);
                bool foundGround = false;

                foreach (var plat in staticColliders)
                {
                    if (!aabb.Intersects(plat)) continue;

                    if (colliders[i].IsOneWay && prevBottom > plat.Top) continue;

                    if (velocities[i].VY >= 0)
                    {
                        positions[i].Value = new Vector2(
                            positions[i].Value.X,
                            plat.Top - colliders[i].OffsetY
                        );
                        velocities[i].VY = 0;
                        foundGround = true;
                    }
                    else if (velocities[i].VY < 0)
                    {
                        positions[i].Value = new Vector2(
                            positions[i].Value.X,
                            plat.Bottom + colliders[i].Height - colliders[i].OffsetY
                        );
                        velocities[i].VY = 0;
                    }
                }

                if (!foundGround)
                {
                    var belowRect = new Rect(
                        aabb.X, aabb.Y,
                        aabb.Width, aabb.Height + GroundSnap
                    );

                    foreach (var plat in staticColliders)
                    {
                        if (!belowRect.Intersects(plat)) continue;

                        if (velocities[i].VY >= 0)
                        {
                            positions[i].Value = new Vector2(
                                positions[i].Value.X,
                                plat.Top - colliders[i].OffsetY
                            );
                            velocities[i].VY = 0;
                        }
                        break;
                    }
                }
            }
        });
    }

    static void ResolveEntityCollisions(World world, CollisionReport report)
    {
        Entries.Clear();

        world.Query<Position, BoxCollider>().ForEachEntity((entities, positions, colliders) =>
        {
            for (int i = 0; i < entities.Length; i++)
            {
                var type = BodyType.Static;
                if (world.TryGetComponent<RigidBody>(entities[i], out var rb))
                    type = rb.Type;

                Entries.Add(new ColliderEntry
                {
                    Entity = entities[i],
                    Aabb = GetAABB(positions[i].Value, colliders[i]),
                    Collider = colliders[i],
                    Type = type
                });
            }
        });

        for (int i = 0; i < Entries.Count; i++)
        {
            for (int j = i + 1; j < Entries.Count; j++)
            {
                var a = Entries[i];
                var b = Entries[j];

                if (a.Collider.IsTrigger || b.Collider.IsTrigger) continue;
                if (!a.Aabb.Intersects(b.Aabb)) continue;

                bool aCanMove = a.Type == BodyType.Dynamic;
                bool bCanMove = b.Type == BodyType.Dynamic;
                if (!aCanMove && !bCanMove) continue;

                float overlapX = MathF.Min(a.Aabb.Right, b.Aabb.Right) - MathF.Max(a.Aabb.Left, b.Aabb.Left);
                float overlapY = MathF.Min(a.Aabb.Bottom, b.Aabb.Bottom) - MathF.Max(a.Aabb.Top, b.Aabb.Top);

                Vector2 normal;
                float pen;
                if (overlapX < overlapY)
                {
                    normal = new Vector2(a.Aabb.Center.X < b.Aabb.Center.X ? 1f : -1f, 0f);
                    pen = overlapX;
                }
                else
                {
                    normal = new Vector2(0f, a.Aabb.Center.Y < b.Aabb.Center.Y ? 1f : -1f);
                    pen = overlapY;
                }

                if (!aCanMove)
                    MoveEntity(world, b.Entity, normal * pen);
                else if (!bCanMove)
                    MoveEntity(world, a.Entity, -normal * pen);
                else
                {
                    MoveEntity(world, a.Entity, -normal * (pen / 2f));
                    MoveEntity(world, b.Entity, normal * (pen / 2f));
                }

                report.Collisions.Add(new CollisionEvent
                {
                    A = a.Entity,
                    B = b.Entity,
                    Normal = normal,
                    Penetration = pen
                });

                Entries[i] = a with { Aabb = GetEntityAabb(world, a.Entity, a.Collider) };
                Entries[j] = b with { Aabb = GetEntityAabb(world, b.Entity, b.Collider) };
            }
        }
    }

    static void MoveEntity(World world, Entity entity, Vector2 delta)
    {
        if (!world.TryGetComponent<Position>(entity, out var pos))
            return;

        pos.Value += delta;
        world.SetComponent(entity, pos);
    }

    static Rect GetEntityAabb(World world, Entity entity, BoxCollider collider)
    {
        var pos = world.GetComponent<Position>(entity);
        return GetAABB(pos.Value, collider);
    }

    static void UpdateGrounded(World world, List<Rect> staticColliders)
    {
        world.Query<Position, BoxCollider, Grounded>().ForEach((positions, colliders, groundeds) =>
        {
            for (int i = 0; i < positions.Length; i++)
            {
                var aabb = GetAABB(positions[i].Value, colliders[i]);
                var belowRect = new Rect(aabb.X, aabb.Y, aabb.Width, aabb.Height + GroundSnap);
                bool found = false;

                foreach (var plat in staticColliders)
                {
                    if (belowRect.Intersects(plat))
                    {
                        found = true;
                        break;
                    }
                }

                groundeds[i].Value = found;
            }
        });
    }

    public static Rect GetAABB(Vector2 pos, BoxCollider col)
    {
        return new Rect(
            pos.X + col.OffsetX - col.Width / 2,
            pos.Y + col.OffsetY - col.Height,
            col.Width, col.Height
        );
    }
}
