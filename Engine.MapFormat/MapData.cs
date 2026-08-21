namespace Engine.MapFormat;

public class MapData
{
    public string TilesetPath = "";
    public int TileWidth = 32;
    public int TileHeight = 32;
    public int Width;
    public int Height;
    public List<TileDef> Tiles = new();
    public List<Layer> Layers = new();
    public List<MapEvent> Events = new();
}

public struct TileDef
{
    public int Id;
    public string Name;
    public bool Solid;
    public int Damage;
    public string? Trigger;
    public bool IsOneWay;

    public override string ToString()
        => $"Tile({Id}, {Name}, solid={Solid}, damage={Damage}, trigger={Trigger})";
}

public struct Layer
{
    public string Name;
    public int[,] Data;

    public Layer(string name, int width, int height)
    {
        Name = name;
        Data = new int[width, height];
    }
}

public struct MapEvent
{
    public string Name;
    public int X;
    public int Y;
    public string? Target;

    public override string ToString()
        => $"Event({Name}, {X},{Y}, target={Target})";
}
