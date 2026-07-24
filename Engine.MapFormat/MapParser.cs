namespace Engine.MapFormat;

public class MapParser
{
    private List<Token> _tokens = new();
    private int _pos;

    public MapData Parse(string text)
    {
        var tokenizer = new Tokenizer();
        _tokens = tokenizer.Tokenize(text);
        _pos = 0;

        var map = new MapData();

        while (_pos < _tokens.Count && Current().Type != TokenType.EOF)
        {
            var token = Current();

            if (token.Type == TokenType.KEYWORD)
            {
                switch (token.Value)
                {
                    case "TILESET": ParseTileset(map); break;
                    case "LAYER": ParseLayer(map); break;
                    case "EVENT": ParseEvent(map); break;
                    default: _pos++; break;
                }
            }
            else
            {
                _pos++;
            }
        }

        return map;
    }

    public MapData ParseFile(string path)
    {
        string text = File.ReadAllText(path);
        return Parse(text);
    }

    // ─── Helpers ────────────────────────────────────────────────────

    private Token Current()
    {
        if (_pos >= _tokens.Count)
            return new Token(TokenType.EOF, "");
        return _tokens[_pos];
    }

    private Token Advance()
    {
        var token = Current();
        _pos++;
        return token;
    }

    private void SkipNewlines()
    {
        while (Current().Type == TokenType.NEWLINE)
            _pos++;
    }

    private void Expect(TokenType type)
    {
        var token = Advance();
        if (token.Type != type)
            throw new Exception($"Expected {type} but got {token.Type}:{token.Value} at token {_pos}");
    }

    private void Expect(string value)
    {
        var token = Advance();
        if (token.Value != value)
            throw new Exception($"Expected '{value}' but got '{token.Value}' at token {_pos}");
    }

    // ─── Parse TILESET ─────────────────────────────────────────────

    private void ParseTileset(MapData map)
    {
        Expect("TILESET");
        map.TilesetPath = Advance().Value;
        Expect(TokenType.LBRACE);
        SkipNewlines();

        while (Current().Type != TokenType.RBRACE && Current().Type != TokenType.EOF)
        {
            if (Current().Value == "TILE")
                ParseTileDef(map);
            else
                _pos++;
            SkipNewlines();
        }

        Expect(TokenType.RBRACE);
        SkipNewlines();
    }

    private void ParseTileDef(MapData map)
    {
        Expect("TILE");
        string name = Advance().Value;
        Expect(TokenType.LBRACE);
        SkipNewlines();

        var tile = new TileDef { Name = name };

        while (Current().Type != TokenType.RBRACE && Current().Type != TokenType.EOF)
        {
            string key = Advance().Value;
            Expect(TokenType.EQUAL);
            string val = Advance().Value;

            switch (key)
            {
                case "id": tile.Id = int.Parse(val); break;
                case "solid": tile.Solid = val == "true"; break;
                case "damage": tile.Damage = int.Parse(val); break;
                case "trigger": tile.Trigger = val; break;
            }

            if (Current().Type == TokenType.COMMA)
                _pos++;
            SkipNewlines();
        }

        Expect(TokenType.RBRACE);
        SkipNewlines();
        map.Tiles.Add(tile);
    }

    // ─── Parse LAYER ───────────────────────────────────────────────

    private void ParseLayer(MapData map)
    {
        Expect("LAYER");
        string name = Advance().Value;
        Expect(TokenType.LBRACE);
        SkipNewlines();

        var rows = new List<List<int>>();

        while (Current().Type != TokenType.RBRACE && Current().Type != TokenType.EOF)
        {
            if (Current().Type == TokenType.NUMBER)
            {
                var row = new List<int>();
                while (Current().Type == TokenType.NUMBER)
                {
                    row.Add(int.Parse(Advance().Value));
                    if (Current().Type == TokenType.COMMA)
                        _pos++;
                }
                if (row.Count > 0) rows.Add(row);
            }

            if (Current().Type == TokenType.NEWLINE)
                _pos++;
            else
                _pos++;
        }

        Expect(TokenType.RBRACE);
        SkipNewlines();

        if (rows.Count > 0)
        {
            int width = rows[0].Count;
            int height = rows.Count;

            if (width > map.Width) map.Width = width;
            if (height > map.Height) map.Height = height;

            var layer = new Layer(name, width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width && x < rows[y].Count; x++)
                    layer.Data[x, y] = rows[y][x];

            map.Layers.Add(layer);
        }
    }

    // ─── Parse EVENT ───────────────────────────────────────────────

    private void ParseEvent(MapData map)
    {
        Expect("EVENT");
        string name = Advance().Value;
        Expect(TokenType.LBRACE);
        SkipNewlines();

        var evt = new MapEvent { Name = name };

        while (Current().Type != TokenType.RBRACE && Current().Type != TokenType.EOF)
        {
            string key = Advance().Value;
            Expect(TokenType.EQUAL);

            if (Current().Type == TokenType.STRING)
            {
                string val = Advance().Value;
                switch (key)
                {
                    case "target": evt.Target = val; break;
                }
            }
            else if (Current().Type == TokenType.NUMBER)
            {
                int val = int.Parse(Advance().Value);
                switch (key)
                {
                    case "x": evt.X = val; break;
                    case "y": evt.Y = val; break;
                }
            }

            if (Current().Type == TokenType.COMMA)
                _pos++;
            SkipNewlines();
        }

        Expect(TokenType.RBRACE);
        SkipNewlines();
        map.Events.Add(evt);
    }
}
