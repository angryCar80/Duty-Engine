using Engine.Core;
using Engine.Rendering;
using Engine.Math;
using Engine.MapFormat;
using SDL3;
using Game;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var map = new MapParser().ParseFile("Assets/room1.map");

var tilemap = new TilemapRenderer();
var camera = new Camera(1280, 720);
SpriteRenderer? sr = null;
Texture2D? playerTex = null;

var playerPos = new Vector2(0, 0);
var playerVel = new Velocity();
var player = Player.Create();
var collider = new Collider { Width = 28, Height = 48 };
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
        playerPos = new Vector2(e.X * map.TileWidth + map.TileWidth / 2, (e.Y + 1) * map.TileHeight);
    }
    else
    {
        playerPos = new Vector2(200, 500);
    }

    camera.Position = new Vector2(playerPos.X, playerPos.Y - collider.Height / 2);
});

engine.OnUpdate((dt) =>
{
    if (engine.Input.IsKeyPressed(SDL.Keycode.Escape))
        engine.Quit();

    playerVel.VX = 0;
    if (engine.Input.IsKeyDown(SDL.Keycode.A) || engine.Input.IsKeyDown(SDL.Keycode.Left))
    {
        playerVel.VX = -player.Speed;
        player.FacingRight = false;
    }
    if (engine.Input.IsKeyDown(SDL.Keycode.D) || engine.Input.IsKeyDown(SDL.Keycode.Right))
    {
        playerVel.VX = player.Speed;
        player.FacingRight = true;
    }

    if (player.Grounded && (engine.Input.IsKeyPressed(SDL.Keycode.Space) || engine.Input.IsKeyPressed(SDL.Keycode.W) || engine.Input.IsKeyPressed(SDL.Keycode.Up)))
    {
        playerVel.VY = player.JumpForce;
        player.Grounded = false;
    }

    Physics.Update(ref playerPos, ref playerVel, ref player, dt, platforms);

    camera.Follow(new Vector2(playerPos.X, playerPos.Y - collider.Height / 2), dt, 8f);
});

engine.OnRender((renderer) =>
{
    if (sr == null || playerTex == null) return;

    sr.Begin(camera);
    tilemap.Render(sr, "background");
    tilemap.Render(sr, "collision");

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
