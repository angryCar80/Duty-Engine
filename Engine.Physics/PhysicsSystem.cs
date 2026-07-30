using Engine.Ecs;
using Engine.Math;

namespace Engine.Physics;

public static class PhysicsSystem
{
    private const float TerminalVelocity = 600f;
    private const float GroundSnap = 2f;

    public static void Update(World world, float dt, List<Rect> staticColliders)
    {
        ApplyGravityAndForces(world, dt);
        MoveAndResolveX(world, dt, staticColliders);
        MoveAndResolveY(world, dt, staticColliders);
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
