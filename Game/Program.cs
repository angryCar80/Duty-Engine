using Engine.Core;
using Engine.Rendering;
using Engine.Math;
using Engine.MapFormat;
using SDL3;
using Game;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var logFile = "/tmp/engine_debug.log";
File.WriteAllText(logFile, $"=== Engine Debug Log ===\n");
File.AppendAllText(logFile, $"CWD: {Directory.GetCurrentDirectory()}\n");
File.AppendAllText(logFile, $"BaseDir: {AppContext.BaseDirectory}\n");
File.AppendAllText(logFile, $"Map exists: {File.Exists("Assets/room1.map")}\n");
File.AppendAllText(logFile, $"Tileset exists: {File.Exists("Assets/tileset.png")}\n");

var map = new MapParser().ParseFile("Assets/room1.map");
File.AppendAllText(logFile, $"Map loaded: {map.Width}x{map.Height}, layers={map.Layers.Count}, tiles={map.Tiles.Count}\n");
File.AppendAllText(logFile, $"TilesetPath: '{map.TilesetPath}'\n");
File.AppendAllText(logFile, $"TileWidth: {map.TileWidth}, TileHeight: {map.TileHeight}\n");

for (int i = 0; i < map.Layers.Count; i++)
{
    var l = map.Layers[i];
    File.AppendAllText(logFile, $"  Layer[{i}] '{l.Name}' data={l.Data.GetLength(0)}x{l.Data.GetLength(1)}\n");
}
for (int i = 0; i < map.Events.Count; i++)
{
    File.AppendAllText(logFile, $"  Event[{i}] '{map.Events[i].Name}' x={map.Events[i].X} y={map.Events[i].Y}\n");
}

var tilemap = new TilemapRenderer();
var camera = new Camera(1280, 720);
SpriteRenderer? sr = null;
Texture2D? playerTex = null;
IntPtr rendererHandle = 0;

var playerPos = new Vector2(0, 0);
var playerVel = new Velocity();
var player = Player.Create();
var collider = new Collider { Width = 28, Height = 48 };
var platforms = new List<Rect>();

int frameCount = 0;


var engine = new EngineApp("Platformer", 1280, 720);

engine.OnInit(() =>
{
    rendererHandle = engine.Renderer;
    sr = new SpriteRenderer(rendererHandle);
    playerTex = Texture2D.Create(rendererHandle, 28, 48, 50, 130, 255);

    File.AppendAllText(logFile, $"Renderer ptr: {rendererHandle}\n");
    File.AppendAllText(logFile, $"PlayerTex ptr: {playerTex.Handle}, size: {playerTex.Width}x{playerTex.Height}\n");

    tilemap.Load(rendererHandle, map);
    platforms = tilemap.GetCollisionRects();
    File.AppendAllText(logFile, $"Collision rects: {platforms.Count}\n");
    if (platforms.Count > 0)
    {
        File.AppendAllText(logFile, $"  First rect: {platforms[0]}\n");
        File.AppendAllText(logFile, $"  Last rect: {platforms[platforms.Count - 1]}\n");
    }

    File.AppendAllText(logFile, $"TilemapRenderer tileset: {tilemap.Map?.TilesetPath}\n");

    var spawn = tilemap.GetEvent("spawn_player");
    if (spawn != null)
    {
        var e = spawn.Value;
        playerPos = new Vector2(e.X * map.TileWidth + map.TileWidth / 2, (e.Y + 1) * map.TileHeight);
        File.AppendAllText(logFile, $"Spawn at: {playerPos.X}, {playerPos.Y}\n");
    }
    else
    {
        playerPos = new Vector2(200, 500);
        File.AppendAllText(logFile, "No spawn event, using default\n");
    }

    camera.Position = new Vector2(playerPos.X, playerPos.Y - collider.Height / 2);
    File.AppendAllText(logFile, $"Camera init pos: {camera.Position}\n");
    File.AppendAllText(logFile, $"=== Init complete ===\n");
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

    frameCount++;
    if (frameCount <= 120)
    {
        File.AppendAllText(logFile,
            $"F{frameCount:D4} dt={dt:F4} pos=({playerPos.X:F1},{playerPos.Y:F1}) " +
            $"vel=({playerVel.VX:F1},{playerVel.VY:F1}) grounded={player.Grounded} " +
            $"cam=({camera.Position.X:F1},{camera.Position.Y:F1})\n");
    }
});

engine.OnRender((renderer) =>
{
    if (sr == null || playerTex == null) return;

    sr.Begin(camera);

    if (frameCount <= 5)
    {
        var testDst = new SDL.FRect { X = 100, Y = 100, W = 200, H = 200 };
        SDL.SetRenderDrawColor(renderer, 255, 0, 0, 255);
        SDL.RenderFillRect(renderer, in testDst);
        File.AppendAllText(logFile, $"F{frameCount} DREW TEST RECT at (100,100) 200x200\n");
    }

    tilemap.Render(sr, "background");
    tilemap.Render(sr, "collision");

    var playerDrawPos = new Vector2(
        playerPos.X - playerTex.Width / 2f,
        playerPos.Y - playerTex.Height
    );
    sr.Draw(playerTex, playerDrawPos);

    sr.End();

    if (frameCount <= 5)
    {
        var pdScreen = camera.WorldToScreen(playerDrawPos);
        File.AppendAllText(logFile,
            $"F{frameCount} playerDraw=({playerDrawPos.X:F1},{playerDrawPos.Y:F1}) " +
            $"screen=({pdScreen.X:F1},{pdScreen.Y:F1}) " +
            $"texSize={playerTex.Width}x{playerTex.Height}\n");
    }
});

engine.OnShutdown(() =>
{
    File.AppendAllText(logFile, $"=== Shutdown (frames: {frameCount}) ===\n");
    playerTex?.Dispose();
    tilemap.Dispose();
});

engine.Run();
