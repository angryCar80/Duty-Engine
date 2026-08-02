using SDL3;
using Engine.Math;
using Engine.Rendering;

namespace Engine.UI;

public class TextRenderer : IDisposable
{
    private readonly IntPtr _renderer;
    private readonly FontCache _fonts;
    private readonly Dictionary<TextKey, Texture2D> _cache = new();
    private readonly List<TextKey> _order = new();

    private const int MaxCached = 256;

    private readonly struct TextKey : IEquatable<TextKey>
    {
        public readonly string FontPath;
        public readonly int Size;
        public readonly Color Color;
        public readonly string Text;

        public TextKey(string fontPath, int size, Color color, string text)
        {
            FontPath = fontPath;
            Size = size;
            Color = color;
            Text = text;
        }

        public bool Equals(TextKey other)
            => FontPath == other.FontPath
            && Size == other.Size
            && Color == other.Color
            && Text == other.Text;

        public override bool Equals(object? obj) => obj is TextKey k && Equals(k);

        public override int GetHashCode() => HashCode.Combine(FontPath, Size, Color, Text);
    }

    public TextRenderer(IntPtr renderer, FontCache fonts)
    {
        _renderer = renderer;
        _fonts = fonts;
    }

    public void Draw(string fontPath, int size, string text, float x, float y, Color color, float scale = 1f)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var key = new TextKey(fontPath, size, color, text);
        if (!_cache.TryGetValue(key, out var tex))
        {
            tex = RenderText(key);
            _cache[key] = tex;
            _order.Add(key);
            EvictIfNeeded();
        }

        var dst = new SDL.FRect
        {
            X = x,
            Y = y,
            W = tex.Width * scale,
            H = tex.Height * scale
        };

        SDL.RenderTexture(_renderer, tex.Handle, IntPtr.Zero, in dst);
    }

    public void DrawCentered(string fontPath, int size, string text, float centerX, float centerY, Color color, float scale = 1f)
    {
        var (w, h) = Measure(fontPath, size, text);
        Draw(fontPath, size, text, centerX - w * scale / 2f, centerY - h * scale / 2f, color, scale);
    }

    public (float Width, float Height) Measure(string fontPath, int size, string text)
    {
        if (string.IsNullOrEmpty(text))
            return (0, 0);

        var font = _fonts.Get(fontPath, size);
        if (!TTF.GetStringSize(font.Handle, text, (UIntPtr)text.Length, out int w, out int h))
            return (0, 0);

        return (w, h);
    }

    private Texture2D RenderText(TextKey key)
    {
        var font = _fonts.Get(key.FontPath, key.Size);

        IntPtr surface = TTF.RenderTextBlended(
            font.Handle,
            key.Text,
            (UIntPtr)key.Text.Length,
            new SDL.Color { R = key.Color.R, G = key.Color.G, B = key.Color.B, A = key.Color.A }
        );

        if (surface == IntPtr.Zero)
            throw new Exception($"Failed to render text '{key.Text}': {SDL.GetError()}");

        var tex = Texture2D.FromSurface(_renderer, surface);
        SDL.DestroySurface(surface);

        tex.SetBlendMode(SDL.BlendMode.Blend);

        return tex;
    }

    private void EvictIfNeeded()
    {
        while (_order.Count > MaxCached)
        {
            var oldest = _order[0];
            _order.RemoveAt(0);
            if (_cache.Remove(oldest, out var tex))
                tex.Dispose();
        }
    }

    public void ClearCache()
    {
        foreach (var tex in _cache.Values)
            tex.Dispose();
        _cache.Clear();
        _order.Clear();
    }

    public void Dispose()
    {
        ClearCache();
        GC.SuppressFinalize(this);
    }

    ~TextRenderer() => Dispose();
}
