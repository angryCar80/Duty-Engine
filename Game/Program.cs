using Engine.Core;
using Engine.Rendering;
using Engine.Math;
using Engine.MapFormat;
using Engine.Physics;
using Engine.Ecs;
using SDL3;
using Game;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var map = new MapParser().ParseFile("Assets/room1.map");

var tilemap = new TilemapRenderer();
var camera = new Camera(1280, 720);
SpriteRenderer? sr = null;
Texture2D? playerTex = null;

var world = new World();

var player = world.Create();
world.AddComponent(player, new Engine.Physics.Position { Value = new Vector2(0, 0) });
world.AddComponent(player, new Velocity());
world.AddComponent(player, new BoxCollider { Width = 28, Height = 48 });
world.AddComponent(player, new RigidBody { GravityScale = 1, UseGravity = true, Mass = 1 });
world.AddComponent(player, new Grounded());
world.AddComponent(player, PlayerState.Create());

var platforms = new List<Rect>();

var engine = new EngineApp("Platformer", 1280, 720);

engine.OnInit(() =>
{
    sr = new SpriteRenderer(engine.Renderer);
    playerTex = Texture2D.Create(engine.Renderer, 28, 48, 50, 130, 255);

    tilemap.Load(engine.Renderer, map);
    platforms = tilemap.GetCollisionRects();

    var spawn = tilemap.GetEvent("spawn_player");
    if (spawn != null)
    {
        var e = spawn.Value;
        var p = new Vector2(e.X * map.TileWidth + map.TileWidth / 2, (e.Y + 1) * map.TileHeight);
        world.Query<Engine.Physics.Position>().ForEach(positions =>
        {
            for (int i = 0; i < positions.Length; i++)
                positions[i] = new Engine.Physics.Position { Value = p };
        });
    }
});

engine.OnUpdate((dt) =>
{
    if (engine.Input.IsKeyPressed(SDL.Keycode.Escape))
        engine.Quit();

    world.Query<Velocity, PlayerState>().ForEach((velocities, states) =>
    {
        for (int i = 0; i < velocities.Length; i++)
        {
            velocities[i].VX = 0;
            if (engine.Input.IsKeyDown(SDL.Keycode.A) || engine.Input.IsKeyDown(SDL.Keycode.Left))
            {
                velocities[i].VX = -states[i].Speed;
                states[i].FacingRight = false;
            }
            if (engine.Input.IsKeyDown(SDL.Keycode.D) || engine.Input.IsKeyDown(SDL.Keycode.Right))
            {
                velocities[i].VX = states[i].Speed;
                states[i].FacingRight = true;
            }
        }
    });

    bool doJump = engine.Input.IsKeyPressed(SDL.Keycode.Space)
               || engine.Input.IsKeyPressed(SDL.Keycode.W)
               || engine.Input.IsKeyPressed(SDL.Keycode.Up);

    world.Query<Velocity, Grounded, PlayerState>().ForEach((velocities, groundeds, states) =>
    {
        for (int i = 0; i < velocities.Length; i++)
        {
            if (groundeds[i].Value && doJump)
            {
                velocities[i].VY = states[i].JumpForce;
                groundeds[i].Value = false;
            }
        }
    });

    PhysicsSystem.Update(world, dt, platforms);

    var playerPos = new Vector2();
    world.Query<Engine.Physics.Position>().ForEach(positions =>
    {
        playerPos = positions[0].Value;
    });

    camera.Follow(new Vector2(playerPos.X, playerPos.Y - 24), dt, 8f);
});

engine.OnRender((renderer) =>
{
    if (sr == null || playerTex == null) return;

    sr.Begin(camera);
    tilemap.Render(sr, "background");
    tilemap.Render(sr, "collision");

    var playerPos = new Vector2();
    world.Query<Engine.Physics.Position>().ForEach(positions =>
    {
        playerPos = positions[0].Value;
    });

    var playerDrawPos = new Vector2(
        playerPos.X - playerTex.Width / 2f,
        playerPos.Y - playerTex.Height
    );
    sr.Draw(playerTex, playerDrawPos);
    sr.End();
});

engine.OnShutdown(() =>
{
    playerTex?.Dispose();
    tilemap.Dispose();
});

engine.Run();
