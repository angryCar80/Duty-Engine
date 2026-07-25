using Engine.Math;

namespace Game;

static class Physics
{
    public static void Update(ref Vector2 position, ref Velocity vel, ref Player player, float dt, List<Rect> platforms)
    {
        if (!player.Grounded)
            vel.VY += player.Gravity * dt;

        if (vel.VY > 600f)
            vel.VY = 600f;

        var collider = new Collider { Width = 28, Height = 48 };

        // Move X
        position = new Vector2(position.X + vel.VX * dt, position.Y);

        var playerRect = collider.GetRect(position);

        foreach (var plat in platforms)
        {
            if (!playerRect.Intersects(plat))
                continue;

            if (vel.VX > 0)
                position = new Vector2(plat.Left - collider.Width / 2, position.Y);
            else if (vel.VX < 0)
                position = new Vector2(plat.Right + collider.Width / 2, position.Y);

            vel.VX = 0;
            playerRect = collider.GetRect(position);
        }

        // Move Y
        position = new Vector2(position.X, position.Y + vel.VY * dt);
        playerRect = collider.GetRect(position);

        bool foundGround = false;

        foreach (var plat in platforms)
        {
            if (!playerRect.Intersects(plat))
                continue;

            if (vel.VY >= 0)
            {
                position = new Vector2(position.X, plat.Top);
                vel.VY = 0;
                foundGround = true;
            }
            else if (vel.VY < 0)
            {
                position = new Vector2(position.X, plat.Bottom + collider.Height);
                vel.VY = 0;
            }
        }

        // Always check for ground using a 2px extended rect below the player.
        // This handles the edge-touching case where player bottom == ground top.
        if (!foundGround)
        {
            var belowRect = new Rect(
                playerRect.X, playerRect.Y,
                playerRect.Width, playerRect.Height + 2f
            );
            foreach (var plat in platforms)
            {
                if (!belowRect.Intersects(plat))
                    continue;

                if (vel.VY >= 0)
                {
                    position = new Vector2(position.X, plat.Top);
                    vel.VY = 0;
                    foundGround = true;
                }
                break;
            }
        }

        player.Grounded = foundGround;

        // Don't fall off the world
        if (position.Y > 800)
        {
            position = new Vector2(200, 100);
            vel.VX = 0;
            vel.VY = 0;
        }
    }
}
