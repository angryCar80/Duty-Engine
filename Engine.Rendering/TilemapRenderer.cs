using SDL3;
using Engine.Math;
using Engine.MapFormat;
using Engine.Physics;

namespace Engine.Rendering;

public class TilemapRenderer
{
    private MapData? _map;
    private Texture2D? _tileset;
    private int _tileWidth;
    private int _tileHeight;
    private int _columns;

    public MapData? Map => _map;

    public void Load(IntPtr renderer, MapData map)
    {
        _map = map;
        _tileWidth = map.TileWidth;
        _tileHeight = map.TileHeight;

        if (!string.IsNullOrEmpty(map.TilesetPath))
        {
            _tileset = Texture2D.Load(renderer, map.TilesetPath);
            _columns = _tileset.Width / _tileWidth;
        }
    }

    public void Render(SpriteRenderer sr, string layerName)
    {
        if (_map == null || _tileset == null) return;

        int idx = _map.Layers.FindIndex(l => l.Name == layerName);
        if (idx < 0) return;
        var layer = _map.Layers[idx];

        int w = layer.Data.GetLength(0);
        int h = layer.Data.GetLength(1);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int id = layer.Data[x, y];
                if (id == 0) continue;

                int tileIdx = id - 1;
                float srcX = (tileIdx % _columns) * _tileWidth;
                float srcY = (tileIdx / _columns) * _tileHeight;

                var src = new SDL.FRect { X = srcX, Y = srcY, W = _tileWidth, H = _tileHeight };
                var pos = new Vector2(x * _tileWidth, y * _tileHeight);

                sr.Draw(_tileset, src, pos);
            }
        }
    }

    public List<CollisionRect> GetCollisionRects()
    {
        var rects = new List<CollisionRect>();
        if (_map == null) return rects;

        int idx = _map.Layers.FindIndex(l => l.Name == "collision");
        if (idx < 0) return rects;
        var layer = _map.Layers[idx];

        int w = layer.Data.GetLength(0);
        int h = layer.Data.GetLength(1);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (layer.Data[x, y] == 0) continue;

                int id = layer.Data[x, y];
                if (id == 0) continue;

                var tileDef = _map.Tiles.Find(t => t.Id == id);
                bool isOneWay = tileDef.IsOneWay;

                rects.Add(new CollisionRect(x * _tileWidth, y * _tileHeight, _tileWidth, _tileHeight, isOneWay));
            }
        }

        return rects;
    }

    public MapEvent? GetEvent(string name)
    {
        if (_map == null) return null;
        int idx = _map.Events.FindIndex(e => e.Name == name);
        if (idx < 0) return null;
        return _map.Events[idx];
    }

    public void Dispose()
    {
        _tileset?.Dispose();
    }
}
