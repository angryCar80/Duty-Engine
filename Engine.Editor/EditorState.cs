using Engine.MapFormat;

namespace Engine.Editor;

public class EditorState
{
    public MapData Map = new();
    public string? FilePath;
    public bool Dirty;

    // Tile palette
    public int SelectedTileId = 1;
    public int TilesetColumns;

    // Layers
    public int ActiveLayerIndex;

    // Events
    public int SelectedEventIndex = -1;

    // Canvas
    public float PanX, PanY;
    public float Zoom = 1f;
    public bool ShowGrid = true;
    public bool ShowCollision;
}
