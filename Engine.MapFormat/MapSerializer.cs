using System.Text;

namespace Engine.MapFormat;

public static class MapSerializer
{
    public static string Serialize(MapData map)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"# Map — {map.Width}x{map.Height}");
        sb.AppendLine();

        sb.AppendLine($"TILESET \"{map.TilesetPath}\" {{");
        foreach (var tile in map.Tiles)
        {
            sb.Append($"    TILE {tile.Name} {{ ");
            sb.Append($"id={tile.Id}");
            sb.Append($", solid={tile.Solid.ToString().ToLower()}");
            if (tile.Damage > 0) sb.Append($", damage={tile.Damage}");
            if (tile.Trigger != null) sb.Append($", trigger=\"{tile.Trigger}\"");
            sb.AppendLine(" }");
        }
        sb.AppendLine("}");
        sb.AppendLine();

        foreach (var layer in map.Layers)
        {
            sb.AppendLine($"LAYER {layer.Name} {{");
            for (int y = 0; y < map.Height; y++)
            {
                var row = new List<string>();
                for (int x = 0; x < map.Width; x++)
                    row.Add(layer.Data[x, y].ToString());
                sb.AppendLine("    " + string.Join(" ", row));
            }
            sb.AppendLine("}");
            sb.AppendLine();
        }

        foreach (var evt in map.Events)
        {
            sb.Append($"EVENT {evt.Name} {{ x={evt.X}, y={evt.Y}");
            if (evt.Target != null) sb.Append($", target=\"{evt.Target}\"");
            sb.AppendLine(" }");
        }

        return sb.ToString();
    }

    public static void Save(MapData map, string path)
    {
        File.WriteAllText(path, Serialize(map));
    }
}
