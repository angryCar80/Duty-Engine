# Duty Engine

A custom 2D game engine built from scratch in C# using SDL3-CS.
The Engine Still Needs Improvments.

## Projects

| Project | Description |
|---------|-------------|
| **Engine.Core** | Game loop, input handling, timing |
| **Engine.Ecs** | Archetype-based Entity Component System |
| **Engine.Math** | Vector2, Rect, Color, Transform |
| **Engine.Rendering** | Texture2D, Camera, SpriteRenderer, SpriteBatch |
| **Engine.MapFormat** | Custom .map file parser & serializer |
| **Game** | Platformer demo |

## Building

```bash
dotnet build
```

## Running

```bash
dotnet run --project Game
```

## Controls (Platformer)

| Key | Action |
|-----|--------|
| A / Left Arrow | Move left |
| D / Right Arrow | Move right |
| Space / W / Up Arrow | Jump |
| Escape | Quit |

## Map Language ( Not Done Yet )

Custom text format for tilemaps with support for layers, tile properties, and events.

```
TILESET "tiles.png" {
    TILE grass { id=1, solid=false }
    TILE wall  { id=2, solid=true }
}

LAYER background {
    0 0 0 0
    0 1 1 0
}

EVENT spawn_player { x=1, y=1 }
```

## Requirements

- .NET 10.0
- SDL3-CS

## License

MIT License - see [LICENSE](LICENSE) for details.
