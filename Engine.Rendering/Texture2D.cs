using SDL3;
using Engine.Math;

namespace Engine.Rendering;

public class Texture2D : IDisposable
{
    private IntPtr _handle;
    private int _width;
    private int _height;
    private bool _disposed;

    private Texture2D(IntPtr handle, int width, int height)
    {
        this._handle = handle;
        this._width = width;
        this._height = height;
    }


    public IntPtr Handle => _handle;
    public int Width => _width;
    public int Height => _height;
    public Vector2 Size => new(_width, _height);


    public static Texture2D Load(IntPtr renderer, string path)
    {
        IntPtr surface = SDL.LoadPNG(path);
        if (surface == IntPtr.Zero)
        {
            throw new Exception($"Faild to load image: {path} - {SDL.GetError()}");
        }
        IntPtr texture = SDL.CreateTextureFromSurface(renderer, surface);

        SDL.DestroySurface(surface);

        if (texture == IntPtr.Zero)
            throw new Exception($"Failed to create texture: {SDL.GetError()}");

        SDL.GetTextureSize(texture, out float w, out float h);

        return new Texture2D(texture, (int)w, (int)h);
    }
    public static Texture2D Create(IntPtr renderer, int width, int height, byte r = 255, byte g = 255, byte b = 255, byte a = 255)
    {
        IntPtr surface = SDL.CreateSurface(width, height, SDL.PixelFormat.RGBA8888);
        if (surface == IntPtr.Zero)
            throw new Exception($"Failed to create surface: {SDL.GetError()}");

        SDL.ClearSurface(surface, r / 255f, g / 255f, b / 255f, a / 255f);

        IntPtr texture = SDL.CreateTextureFromSurface(renderer, surface);
        SDL.DestroySurface(surface);

        if (texture == IntPtr.Zero)
            throw new Exception($"Failed to create texture: {SDL.GetError()}");

        return new Texture2D(texture, width, height);
    }

    public void SetColorMod(byte r, byte g, byte b) => SDL.SetTextureColorMod(_handle, r, g, b);
    public void SetAlphaMod(byte alpha) => SDL.SetTextureAlphaMod(_handle, alpha);
    public void SetBlendMode(SDL.BlendMode mode) => SDL.SetTextureBlendMode(_handle, mode);
    public void Dispose()
    {
        if (!_disposed && _handle != IntPtr.Zero)
        {
            SDL.DestroyTexture(_handle);
            _handle = IntPtr.Zero;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
    ~Texture2D() => Dispose();
}



