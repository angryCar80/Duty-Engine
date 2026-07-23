using Engine.Math;

namespace Game;

static class Physics
{
    public static void Update(ref Vector2 position, ref Velocity vel, ref Player player, float dt, List<Rect> platforms)
    {
        // Apply gravity
        if (!player.Grounded)
            vel.VY += player.Gravity * dt;

        // Clamp fall speed
        if (vel.VY > 600f)
            vel.VY = 600f;

        // Move X first
        position = new Vector2(position.X + vel.VX * dt, position.Y);

        var collider = new Collider { Width = 28, Height = 48 };
        var playerRect = collider.GetRect(position);

        foreach (var plat in platforms)
        {
            if (!playerRect.Intersects(plat))
                continue;

            // Resolve X collision
            if (vel.VX > 0)
            {
                // Moving right → push left
                position = new Vector2(plat.Left - collider.Width / 2, position.Y);
            }
            else if (vel.VX < 0)
            {
                // Moving left → push right
                position = new Vector2(plat.Right + collider.Width / 2, position.Y);
            }
            vel.VX = 0;
        }

        // Move Y
        player.Grounded = false;
        position = new Vector2(position.X, position.Y + vel.VY * dt);
        playerRect = collider.GetRect(position);

        foreach (var plat in platforms)
        {
            if (!playerRect.Intersects(plat))
                continue;

            if (vel.VY > 0)
            {
                // Falling → land on top
                position = new Vector2(position.X, plat.Top);
                vel.VY = 0;
                player.Grounded = true;
            }
            else if (vel.VY < 0)
            {
                // Jumping → hit ceiling
                position = new Vector2(position.X, plat.Bottom + collider.Height);
                vel.VY = 0;
            }
        }

        // Don't fall off the world
        if (position.Y > 800)
        {
            position = new Vector2(200, 100);
            vel.VX = 0;
            vel.VY = 0;
        }
    }
}
