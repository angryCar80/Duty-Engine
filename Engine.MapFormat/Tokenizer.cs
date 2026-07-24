namespace Engine.MapFormat;

public enum TokenType
{
    KEYWORD, STRING, NUMBER, IDENT,
    LBRACE, RBRACE, EQUAL, COMMA, NEWLINE, EOF
}

public struct Token
{
    public TokenType Type;
    public string Value;

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }

    public override string ToString() => $"{Type}:{Value}";
}

public class Tokenizer
{
    private string _text = "";
    private int _pos;

    private static readonly HashSet<string> Keywords = new()
    {
        "TILESET", "TILE", "LAYER", "EVENT",
        "true", "false"
    };

    public List<Token> Tokenize(string text)
    {
        _text = text;
        _pos = 0;
        var tokens = new List<Token>();

        while (_pos < _text.Length)
        {
            char c = _text[_pos];

            if (c == '\n') { tokens.Add(new Token(TokenType.NEWLINE, "\\n")); _pos++; continue; }
            if (c == '\r') { _pos++; continue; }
            if (c == ' ' || c == '\t') { _pos++; continue; }
            if (c == '#') { SkipComment(); continue; }
            if (c == '{') { tokens.Add(new Token(TokenType.LBRACE, "{")); _pos++; continue; }
            if (c == '}') { tokens.Add(new Token(TokenType.RBRACE, "}")); _pos++; continue; }
            if (c == '=') { tokens.Add(new Token(TokenType.EQUAL, "=")); _pos++; continue; }
            if (c == ',') { tokens.Add(new Token(TokenType.COMMA, ",")); _pos++; continue; }
            if (c == '"') { tokens.Add(ReadString()); continue; }
            if (char.IsDigit(c)) { tokens.Add(ReadNumber()); continue; }
            if (char.IsLetter(c) || c == '_') { tokens.Add(ReadWord()); continue; }

            _pos++;
        }

        tokens.Add(new Token(TokenType.EOF, ""));
        return tokens;
    }

    private void SkipComment()
    {
        while (_pos < _text.Length && _text[_pos] != '\n')
            _pos++;
    }

    private Token ReadString()
    {
        _pos++;
        int start = _pos;
        while (_pos < _text.Length && _text[_pos] != '"')
            _pos++;
        string value = _text[start.._pos];
        if (_pos < _text.Length) _pos++;
        return new Token(TokenType.STRING, value);
    }

    private Token ReadNumber()
    {
        int start = _pos;
        while (_pos < _text.Length && (char.IsDigit(_text[_pos]) || _text[_pos] == '.'))
            _pos++;
        return new Token(TokenType.NUMBER, _text[start.._pos]);
    }

    private Token ReadWord()
    {
        int start = _pos;
        while (_pos < _text.Length && (char.IsLetterOrDigit(_text[_pos]) || _text[_pos] == '_'))
            _pos++;
        string word = _text[start.._pos];
        TokenType type = Keywords.Contains(word) ? TokenType.KEYWORD : TokenType.IDENT;
        return new Token(type, word);
    }
}
