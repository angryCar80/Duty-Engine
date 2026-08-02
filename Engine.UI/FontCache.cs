using SDL3;

namespace Engine.UI;

public class Font : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    public Font(IntPtr handle)
    {
        _handle = handle;
    }

    public IntPtr Handle => _handle;

    public void Dispose()
    {
        if (!_disposed && _handle != IntPtr.Zero)
        {
            TTF.CloseFont(_handle);
            _handle = IntPtr.Zero;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~Font() => Dispose();
}

public class FontCache : IDisposable
{
    private readonly Dictionary<(string Path, int Size), Font> _fonts = new();

    public void Init()
    {
        if (!TTF.Init())
            throw new Exception($"SDL_ttf init failed: {SDL.GetError()}");
    }

    public Font Get(string path, int size)
    {
        if (_fonts.TryGetValue((path, size), out var cached))
            return cached;

        IntPtr handle = TTF.OpenFont(path, size);
        if (handle == IntPtr.Zero)
            throw new Exception($"Failed to load font '{path}' at size {size}: {SDL.GetError()}");

        var font = new Font(handle);
        _fonts[(path, size)] = font;
        return font;
    }

    public void Dispose()
    {
        foreach (var font in _fonts.Values)
            font.Dispose();
        _fonts.Clear();
        TTF.Quit();
    }
}
