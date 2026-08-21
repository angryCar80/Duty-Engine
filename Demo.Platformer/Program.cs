using Engine.Core;
using Engine.Rendering;
using Engine.Math;
using Engine.MapFormat;
using Engine.Physics;
using Engine.Ecs;
using SDL3;
using Demo.Platformer;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var map = new MapParser().ParseFile("Assets/room1.map");

var tilemap = new TilemapRenderer();
var camera = new Camera(1280, 720);
SpriteRenderer? sr = null;
Texture2D? playerTex = null;
Texture2D? crateTex = null;
Texture2D? coinTex = null;

var world = new World();
var report = new CollisionReport();

var player = world.Create();
world.AddComponent(player, new Engine.Physics.Position { Value = new Vector2(0, 0) });
world.AddComponent(player, new Velocity());
world.AddComponent(player, new BoxCollider { Width = 28, Height = 48 });
world.AddComponent(player, new RigidBody { Type = BodyType.Dynamic, GravityScale = 1, UseGravity = true, Mass = 1 });
world.AddComponent(player, new Grounded());
world.AddComponent(player, PlayerState.Create());

var crate = world.Create();
world.AddComponent(crate, new Engine.Physics.Position { Value = new Vector2(0, 0) });
world.AddComponent(crate, new Velocity());
world.AddComponent(crate, new BoxCollider { Width = 32, Height = 32 });
world.AddComponent(crate, new RigidBody { Type = BodyType.Dynamic, UseGravity = true, Mass = 1 });

var coin = world.Create();
bool coinCollected = false;
world.AddComponent(coin, new Engine.Physics.Position { Value = new Vector2(0, 0) });
world.AddComponent(coin, new BoxCollider { Width = 24, Height = 24, IsTrigger = true });



var platforms = new List<CollisionRect>();
bool collisionLogged = false;

var engine = new EngineApp("Platformer", 1280, 720);

engine.OnInit(() =>
{
    sr = new SpriteRenderer(engine.Renderer);
    playerTex = Texture2D.Create(engine.Renderer, 28, 48, 50, 130, 255);
    crateTex = Texture2D.Create(engine.Renderer, 32, 32, 180, 70, 70);
    coinTex = Texture2D.Create(engine.Renderer, 24, 24, 255, 200, 0);

    tilemap.Load(engine.Renderer, map);
    platforms = tilemap.GetCollisionRects();

    var spawn = tilemap.GetEvent("spawn_player");
    if (spawn != null)
    {
        var e = spawn.Value;
        var p = new Vector2(e.X * map.TileWidth + map.TileWidth / 2, (e.Y + 1) * map.TileHeight);

        world.SetComponent(player, new Engine.Physics.Position { Value = p });
        world.SetComponent(crate, new Engine.Physics.Position { Value = p + new Vector2(80, 0) });
        world.SetComponent(coin, new Engine.Physics.Position { Value = p + new Vector2(160, 0) });
    }
});

engine.OnUpdate((dt) =>
{
    if (engine.Input.IsKeyPressed(SDL.Keycode.Escape))
        engine.Quit();

    world.Query<Velocity, Grounded, PlayerState>().ForEach((velocities, groundeds, states) =>
    {
        for (int i = 0; i < velocities.Length; i++)
        {
            float inputDir = 0f;
            if (engine.Input.IsKeyDown(SDL.Keycode.A) || engine.Input.IsKeyDown(SDL.Keycode.Left))
            {
                inputDir = -1f;
                states[i].FacingRight = false;
            }
            if (engine.Input.IsKeyDown(SDL.Keycode.D) || engine.Input.IsKeyDown(SDL.Keycode.Right))
            {
                inputDir = 1f;
                states[i].FacingRight = true;
            }

            float target = inputDir * states[i].Speed;
            bool grounded = groundeds[i].Value;
            float accel = grounded ? 1800f : 900f;
            float decel = grounded ? 2400f : 1200f;
            float step = MathF.Abs(target) > 0.1f ? accel : decel;
            velocities[i].VX = MathHelper.Approach(velocities[i].VX, target, step * dt);
        }
    });

    bool doJump = engine.Input.IsKeyPressed(SDL.Keycode.Space)
               || engine.Input.IsKeyPressed(SDL.Keycode.W)
               || engine.Input.IsKeyPressed(SDL.Keycode.Up);

    world.Query<Velocity, Grounded, PlayerState>().ForEach((velocities, groundeds, states) =>
    {
        for (int i = 0; i < velocities.Length; i++)
        {
            if (doJump && !groundeds[i].Value)
                states[i].JumpBufferTimer = 0.1f;

            if (groundeds[i].Value && (doJump || states[i].JumpBufferTimer > 0))
            {
                velocities[i].VY = states[i].JumpForce;
                groundeds[i].Value = false;
                states[i].JumpBufferTimer = 0;
            }

            if (states[i].JumpBufferTimer > 0)
                states[i].JumpBufferTimer -= dt;
        }

    });

    PhysicsSystem.Update(world, dt, platforms, report);

    foreach (var trig in report.Triggers)
    {
        if (trig.Trigger == coin && trig.Entered && !coinCollected)
        {
            coinCollected = true;
            world.DestroyEntity(coin);
        }
    }

    foreach (var col in report.Collisions)
    {
        if (!collisionLogged && (col.A == player && col.B == crate || col.A == crate && col.B == player))
        {
            collisionLogged = true;
            Console.WriteLine($"PLAYER <-> CRATE COLLISION (pen={col.Penetration:F1}, normal={col.Normal})");
        }
    }

    var playerPos = world.GetComponent<Engine.Physics.Position>(player).Value;
    camera.Follow(new Vector2(playerPos.X, playerPos.Y - 24), dt, 8f);
});

engine.OnRender((renderer) =>
{
    if (sr == null || playerTex == null || crateTex == null || coinTex == null) return;

    sr.Begin(camera);
    tilemap.Render(sr, "background");
    tilemap.Render(sr, "collision");

    var playerPos = world.GetComponent<Engine.Physics.Position>(player).Value;
    var cratePos = world.GetComponent<Engine.Physics.Position>(crate).Value;
    var coinPos = world.GetComponent<Engine.Physics.Position>(coin).Value;

    sr.Draw(playerTex, new Vector2(playerPos.X - playerTex.Width / 2f, playerPos.Y - playerTex.Height));
    sr.Draw(crateTex, new Vector2(cratePos.X - crateTex.Width / 2f, cratePos.Y - crateTex.Height));
    if (!coinCollected) sr.Draw(coinTex, new Vector2(coinPos.X - coinTex.Width / 2f, coinPos.Y - coinTex.Height));
    sr.End();
});

engine.OnShutdown(() =>
{
    playerTex?.Dispose();
    crateTex?.Dispose();
    coinTex?.Dispose();
    tilemap.Dispose();
});

engine.Run();
