using Engine.Core;
using Engine.Rendering;
using Engine.Math;
using SDL3;
using Game;

var engine = new EngineApp("Platformer", 1280, 720);

var playerPos = new Vector2(200, 500);
var playerVel = new Velocity();
var player = Player.Create();
var collider = new Collider { Width = 28, Height = 48 };

var camera = new Camera(1280, 720);
SpriteRenderer? sr = null;
Texture2D? playerTex = null;
Texture2D? platformTex = null;

var platforms = new List<Rect>
{
    new(0, 600, 1280, 120),
    new(150, 480, 180, 18),
    new(420, 400, 180, 18),
    new(680, 330, 180, 18),
    new(950, 420, 180, 18),
    new(300, 240, 160, 18),
    new(600, 180, 160, 18),
    new(-40, 0, 40, 720),
    new(1280, 0, 40, 720),
};

engine.OnInit(() =>
{
    sr = new SpriteRenderer(engine.Renderer);
    playerTex = Texture2D.Create(engine.Renderer, 28, 48, 50, 130, 255);
    platformTex = Texture2D.Create(engine.Renderer, 1, 1, 60, 200, 80);
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
    if (sr == null || playerTex == null || platformTex == null) return;

    sr.Begin(camera);
    foreach (var p in platforms)
    {
        var drawPos = new Vector2(p.X, p.Y);
        sr.Draw(platformTex, drawPos, new Vector2(p.Width, p.Height), 0f);
    }

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
    platformTex?.Dispose();
});

engine.Run();
